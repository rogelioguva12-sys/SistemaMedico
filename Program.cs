using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SistemaMedico.Data;
using SistemaMedico.Helpers.Security;
using SistemaMedico.Repositories;
using SistemaMedico.Repositories.Interfaces;
using SistemaMedico.Services;
using SistemaMedico.Services.Interfaces;

// 1. Comando CLI para generar clave maestra
if (args.Contains("--genkey"))
{
    Console.WriteLine(SecretProtector.GenerateKey());
    return;
}

var builder = WebApplication.CreateBuilder(args);

// 2. Comandos CLI para cifrar / descifrar desde consola
if (args is ["--encrypt" or "--decrypt", var arg, ..])
{
    var p = new SecretProtector(builder.Configuration["Secrets:Masterkey"]);
    Console.WriteLine(args[0] == "--encrypt" ? p.Encrypt(arg) : p.Decrypt(arg));
    return;
}

// 3. Proceso de descifrado en memoria (DEBE EJECUTARSE ANTES DE LEER CUALQUIER CONFIGURACIÓN)
var secretProtector = new SecretProtector(builder.Configuration["Secrets:Masterkey"]);
var protectedPaths = builder.Configuration.GetSection("Secrets:ProtectedPaths").Get<string[]>() ?? [];
builder.Configuration.DecryptProtectedValues(secretProtector);
builder.Services.AddSingleton<ISecretProtector>(secretProtector);

#if DEBUG
// Imprime en consola la versión cifrada del appsettings en modo Debug
if (builder.Configuration.GetValue("Secrets:PrintEncryptedSnapshot", true))
{
    SecureConfiguration.PrintEncryptedSnapshot(builder.Environment, secretProtector, protectedPaths);
}
#endif

// Add services to the container.
builder.Services.AddControllers();

// Leer la configuración del JWT (ahora lee el valor descifrado)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings.GetValue<string>("Secret")
    ?? throw new InvalidOperationException("La clave secreta 'Jwt:Secret' no está configurada.");

// Configurar el servicio de Autenticación con JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Configure OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Data Access Layer
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Register Repositories
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IMedicoRepository, MedicoRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Register Services (Business Logic Layer)
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IMedicoService, MedicoService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Centralizado de Atención Médica Universitaria API v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();