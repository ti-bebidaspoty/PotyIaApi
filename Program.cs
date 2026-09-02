using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PotyIaApi.Helpers;
using PotyIaApi.Interfaces;
using PotyIaApi.Repositories;
using PotyIaApi.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONFIGURAÇÃO DO JWT
// ======================================================

var chaveSecreta = builder.Configuration["Jwt:Key"];
var emissor = builder.Configuration["Jwt:Issuer"];
var audiencia = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(chaveSecreta))
{
    throw new InvalidOperationException(
        "A configuração 'Jwt:Key' não foi encontrada no appsettings.json.");
}

if (string.IsNullOrWhiteSpace(emissor))
{
    throw new InvalidOperationException(
        "A configuração 'Jwt:Issuer' não foi encontrada no appsettings.json.");
}

if (string.IsNullOrWhiteSpace(audiencia))
{
    throw new InvalidOperationException(
        "A configuração 'Jwt:Audience' não foi encontrada no appsettings.json.");
}

// ======================================================
// CONTROLLERS
// ======================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ======================================================
// INJEÇÃO DE DEPENDÊNCIA
// ======================================================

builder.Services.AddScoped<IHelper, Helper>();

builder.Services.AddScoped<
    IBasicoRepositorio,
    BasicoRepositorio>();

builder.Services.AddScoped<
    IAutenticacaoRepositorio,
    AutenticacaoRepositorio>();

builder.Services.AddScoped<
    IUsuarioRepositorio,
    UsuarioRepositorio>();

builder.Services.AddScoped<
    IRefreshTokenRepositorio,
    RefreshTokenRepositorio>();

builder.Services.AddScoped<AutenticacaoService>();

builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddScoped<UsuarioService>();

// ======================================================
// CORS
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsLiberado", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ======================================================
// SWAGGER
// ======================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ======================================================
// AUTENTICAÇÃO JWT
// ======================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Em ambiente local permite que o servidor de autenticação
        // funcione mesmo durante testes de desenvolvimento.
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = emissor,
                ValidAudience = audiencia,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(chaveSecreta)),

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                context.Response.ContentType =
                    "application/json; charset=utf-8";

                var resultado = JsonSerializer.Serialize(new
                {
                    Status = 401,
                    Mensagem =
                        "Token inválido ou ausente. Por favor, autentique-se."
                });

                await context.Response.WriteAsync(resultado);
            }
        };
    });

builder.Services.AddAuthorization();

// ======================================================
// CRIAÇÃO DA APLICAÇÃO
// ======================================================

var app = builder.Build();

// ======================================================
// SWAGGER
// ======================================================

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Poty IA API v1");

    options.RoutePrefix = "swagger";
});

// Ao acessar a raiz, envia para o Swagger.
app.MapGet("/", () => Results.Redirect("/swagger"));

// ======================================================
// PIPELINE
// ======================================================

// Redireciona chamadas HTTP para HTTPS.
app.UseHttpsRedirection();

app.UseCors("CorsLiberado");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();