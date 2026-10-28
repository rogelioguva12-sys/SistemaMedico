using System.Security.Cryptography;
using System.Text;

namespace SistemaMedico.Helpers.Security;

public interface ISecretProtector
{
    string Encrypt(string plaintext);
    string Decrypt(string value);
    bool IsProtected(string? value);
}

public sealed class SecretProtector : ISecretProtector
{
    public const string Prefix = "ENC:";
    private const int NonceLen = 12;
    private const int TagLen = 16;
    private readonly byte[]? _key;

    public SecretProtector(string? masterkey)
    {
        _key = string.IsNullOrWhiteSpace(masterkey)
            ? null
            : SHA256.HashData(Encoding.UTF8.GetBytes(masterkey.Trim()));
    }

    private byte[] Key => _key ?? throw new InvalidOperationException(
        "No hay clave maestra de secretos. Defina 'Secrets:Masterkey' " +
        "(env: Secrets_Masterkey). Genere una con: dotnet run -- --genkey");

    public bool IsProtected(string? value) =>
        value is not null && value.StartsWith(Prefix, StringComparison.Ordinal);

    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        Span<byte> nonce = stackalloc byte[NonceLen];
        RandomNumberGenerator.Fill(nonce);
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plainBytes.Length];
        Span<byte> tag = stackalloc byte[TagLen];

        using var aes = new AesGcm(Key, TagLen);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var blob = new byte[NonceLen + TagLen + cipher.Length];
        nonce.CopyTo(blob);
        tag.CopyTo(blob.AsSpan(NonceLen));
        cipher.CopyTo(blob.AsSpan(NonceLen + TagLen));

        return Prefix + Convert.ToBase64String(blob);
    }

    public string Decrypt(string value)
    {
        if (!IsProtected(value)) return value;

        byte[] blob;
        try
        {
            blob = Convert.FromBase64String(value[Prefix.Length..]);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Valor 'ENC:' con base64 inválido.");
        }

        if (blob.Length < NonceLen + TagLen)
            throw new InvalidOperationException("Valor 'ENC:' truncado o corrupto.");

        var nonce = blob.AsSpan(0, NonceLen);
        var tag = blob.AsSpan(NonceLen, TagLen);
        var cipher = blob.AsSpan(NonceLen + TagLen);
        var plain = new byte[cipher.Length];

        try
        {
            using var aes = new AesGcm(Key, TagLen);
            aes.Decrypt(nonce, cipher, tag, plain);
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException(
                "No se pudo descifrar un valor 'ENC:'. La clave maestra no coincide con la que lo cifró.");
        }

        return Encoding.UTF8.GetString(plain);
    }

    public static string GenerateKey() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}