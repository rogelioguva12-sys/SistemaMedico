using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SistemaMedico.Helpers.Security;

public static class SecureConfiguration
{
    public static int DecryptProtectedValues(this IConfigurationManager config, ISecretProtector protector)
    {
        var overrides = new Dictionary<string, string?>();
        foreach (var (key, value) in config.AsEnumerable())
        {
            if (protector.IsProtected(value))
                overrides[key] = protector.Decrypt(value!);
        }

        if (overrides.Count > 0)
            config.AddInMemoryCollection(overrides);

        return overrides.Count;
    }

    public static void PrintEncryptedSnapshot(
        IHostEnvironment env,
        ISecretProtector protector,
        IReadOnlyCollection<string> protectedPaths)
    {
        var raw = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        JsonNode tree = ToNode(raw.GetChildren(), string.Empty, protector, protectedPaths) ?? new JsonObject();
        var json = tree.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        Console.WriteLine("\n------------------------------------------------------------");
        Console.WriteLine(" appsettings con secretos cifrados (AES-GCM)");
        Console.WriteLine(" Copia los nodos sensibles a appsettings.json / appsettings.Production.json");
        Console.WriteLine("------------------------------------------------------------\n");
        Console.WriteLine(json);
        Console.WriteLine();
    }

    private static JsonNode? ToNode(
        IEnumerable<IConfigurationSection> sections,
        string parentPath,
        ISecretProtector protector,
        IReadOnlyCollection<string> protectedPaths)
    {
        var list = sections.ToList();
        if (list.Count == 0) return null;

        if (list.All(s => int.TryParse(s.Key, out _)))
        {
            var arr = new JsonArray();
            foreach (var s in list.OrderBy(s => int.Parse(s.Key)))
                arr.Add(Leaf(s, Combine(parentPath, s.Key), protector, protectedPaths));
            return arr;
        }

        var obj = new JsonObject();
        foreach (var s in list)
            obj[s.Key] = Leaf(s, Combine(parentPath, s.Key), protector, protectedPaths);

        return obj;
    }

    private static JsonNode? Leaf(
        IConfigurationSection section,
        string path,
        ISecretProtector protector,
        IReadOnlyCollection<string> protectedPaths)
    {
        var children = section.GetChildren().ToList();
        if (children.Count > 0)
            return ToNode(children, path, protector, protectedPaths);

        var value = section.Value;
        if (value is null) return null;

        if (protector.IsProtected(value)) return JsonValue.Create(value);

        if (!string.IsNullOrWhiteSpace(value) && IsProtectedPath(path, protectedPaths))
            return JsonValue.Create(protector.Encrypt(value));

        return PlainLeaf(value);
    }

    private static JsonNode PlainLeaf(string value)
    {
        if (bool.TryParse(value, out var b))
            return JsonValue.Create(b);

        var looksNumeric = value.Length > 0 && !(value.Length > 1 && value[0] == '0');
        if (looksNumeric)
        {
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                return JsonValue.Create(l);

            if (value.IndexOfAny(['.', 'e', 'E']) >= 0 &&
                double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return JsonValue.Create(d);
        }

        return JsonValue.Create(value)!;
    }

    private static bool IsProtectedPath(string path, IReadOnlyCollection<string> protectedPaths) =>
        protectedPaths.Any(p =>
            path.Equals(p, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(p + ":", StringComparison.OrdinalIgnoreCase));

    private static string Combine(string parent, string key) =>
        string.IsNullOrEmpty(parent) ? key : $"{parent}:{key}";
}