using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Tests.TestDoubles;

namespace Refeicao.Tests.Config;

public class CorsTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";
    private const string ChaveCadastroTeste = "chave-de-cadastro-de-teste";
    private const string ConnectionStringTeste = "server=localhost;port=3306;database=refeicao;user=root;password=;";

    private readonly List<WebApplicationFactory<Program>> _factories = [];

    static CorsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("Cadastro__Chave", ChaveCadastroTeste);
        Environment.SetEnvironmentVariable("REFEICAO_CONNECTION_STRING", ConnectionStringTeste);
    }

    public void Dispose()
    {
        foreach (var factory in _factories)
            factory.Dispose();
    }

    private HttpClient CriarClient(string ambiente, IDictionary<string, string?>? configuracaoAdicional = null)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(ambiente);

            if (configuracaoAdicional is not null)
            {
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(configuracaoAdicional));
            }

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUsuarioRepository>();
                services.AddSingleton<IUsuarioRepository>(new FakeUsuarioRepository());
            });
        });

        _factories.Add(factory);
        return factory.CreateClient();
    }

    private static async Task<HttpResponseMessage> EnviarComOrigin(HttpClient client, string origin)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/auth/usuario/login")
        {
            Content = JsonContent.Create(new LoginRequest("inexistente", "qualquer")),
        };
        requisicao.Headers.Add("Origin", origin);

        return await client.SendAsync(requisicao);
    }

    [Fact]
    public async Task Cors_EmDesenvolvimento_DevePermitirQualquerOrigin()
    {
        var client = CriarClient(Environments.Development);

        var resposta = await EnviarComOrigin(client, "https://qualquer-origem.exemplo.com");

        Assert.True(resposta.Headers.TryGetValues("Access-Control-Allow-Origin", out var valores));
        Assert.Equal("*", Assert.Single(valores!));
    }

    [Fact]
    public async Task Cors_EmProducao_DevePermitirOrigemConfigurada()
    {
        var client = CriarClient(Environments.Production, new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.refeicaoiscal.com",
        });

        var resposta = await EnviarComOrigin(client, "https://app.refeicaoiscal.com");

        Assert.True(resposta.Headers.TryGetValues("Access-Control-Allow-Origin", out var valores));
        Assert.Equal("https://app.refeicaoiscal.com", Assert.Single(valores!));
    }

    [Fact]
    public async Task Cors_EmProducao_DeveBloquearOrigemNaoConfigurada()
    {
        var client = CriarClient(Environments.Production, new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.refeicaoiscal.com",
        });

        var resposta = await EnviarComOrigin(client, "https://origem-nao-autorizada.com");

        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_EmProducao_SemOrigensConfiguradas_DeveBloquearTudo()
    {
        var client = CriarClient(Environments.Production);

        var resposta = await EnviarComOrigin(client, "https://qualquer-origem.exemplo.com");

        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
