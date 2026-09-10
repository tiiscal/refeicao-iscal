using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Tests.TestDoubles;

namespace Refeicao.Tests.Middleware;

public class GlobalExceptionHandlerTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";
    private const string ChaveCadastroTeste = "chave-de-cadastro-de-teste";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static GlobalExceptionHandlerTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("Cadastro__Chave", ChaveCadastroTeste);
    }

    public GlobalExceptionHandlerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUsuarioRepository>();
                services.AddSingleton<IUsuarioRepository, ThrowingUsuarioRepository>();
            });
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ExcecaoNaoTratada_DeveRetornarInternalServerErrorComCorpoGenerico()
    {
        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest("user01", "qualquer"));

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ThrowingUsuarioRepository.MensagemErro, corpo);
        Assert.DoesNotContain("StackTrace", corpo, StringComparison.OrdinalIgnoreCase);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problema);
        Assert.Equal(StatusCodes.Status500InternalServerError, problema!.Status);
    }
}
