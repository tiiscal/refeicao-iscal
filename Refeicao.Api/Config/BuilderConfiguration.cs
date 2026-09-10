using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Refeicao.Api.Authentication;
using Refeicao.Api.Middleware;
using Refeicao.Core.Abstractions.Authentication;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;
using Refeicao.Core.Database.Repositories;
using Refeicao.Core.Services;

namespace Refeicao.Api.Config;

public static class BuilderConfiguration
{
    public const string CorsPolicyName = "RefeicaoIscalCors";
    public const string AuthRateLimiterPolicyName = "AuthRateLimit";

    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddCors(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.AddPolicy(CorsPolicyName, policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            }
            else
            {
                var origensPermitidas = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];


                options.AddPolicy(CorsPolicyName, policy => policy
                    .WithOrigins(origensPermitidas)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            }
        });

        var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? throw new InvalidOperationException(
                "Variavel de ambiente 'JWT_SECRET_KEY' nao definida. Configure-a antes de iniciar a aplicacao.");

        var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
            ?? builder.Configuration["Jwt:Issuer"]
            ?? "RefeicaoIscal";
        var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience")
            ?? builder.Configuration["Jwt:Audience"]
            ?? "RefeicaoIscal";
        var jwtExpiresInMinutes = int.TryParse(
            Environment.GetEnvironmentVariable("Jwt__ExpiresInMinutes") ?? builder.Configuration["Jwt:ExpiresInMinutes"],
            out var minutes) ? minutes : 60;

        var jwtOptions = new JwtOptions
        {
            Key = jwtKey,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            ExpiresInMinutes = jwtExpiresInMinutes
        };

        builder.Services.Configure<JwtOptions>(_ =>
        {
            _.Key = jwtOptions.Key;
            _.Issuer = jwtOptions.Issuer;
            _.Audience = jwtOptions.Audience;
            _.ExpiresInMinutes = jwtOptions.ExpiresInMinutes;
        });

        var cadastroChave = Environment.GetEnvironmentVariable("ADMIN_KEY") ?? throw new InvalidOperationException("Variavel de ambiente 'admin_key' nao definida. Configure-a antes de iniciar a aplicacao.");

        var cadastroOptions = new CadastroOptions { Chave = cadastroChave };

        builder.Services.Configure<CadastroOptions>(_ => _.Chave = cadastroOptions.Chave);

        builder.Services.AddDbContext<RefeicaoContext>();

        builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        builder.Services.AddScoped<ICardapioRepository, CardapioRepository>();
        builder.Services.AddScoped<IRefeicaoRepository, RefeicaoRepository>();
        builder.Services.AddScoped<IAcompanhamentoRepository, AcompanhamentoRepository>();
        builder.Services.AddScoped<IAcompanhamentoRefeicaoRepository, AcompanhamentoRefeicaoRepository>();

        builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
        builder.Services.AddSingleton<IPasswordHasher<Funcionario>, PasswordHasher<Funcionario>>();
        builder.Services.AddSingleton<ITokenService, TokenService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IAuthorizationHandler, SenhaAlteradaHandler>();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthRateLimiterPolicyName, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT: Bearer {token}",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() },
            });
        });
    }
}
