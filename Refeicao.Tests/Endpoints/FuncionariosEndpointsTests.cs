using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;
using Refeicao.Tests.TestDoubles;

namespace Refeicao.Tests.Endpoints;

public class FuncionariosEndpointsTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";
    private const string ChaveAdminTeste = "chave-de-cadastro-de-teste";

    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

    private readonly FakeFuncionarioRepository _funcionarioRepository = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static FuncionariosEndpointsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("ADMIN_KEY", ChaveAdminTeste);
    }

    public FuncionariosEndpointsTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IFuncionarioRepository>();
                services.AddSingleton<IFuncionarioRepository>(_funcionarioRepository);
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
    public async Task CadastroFuncionario_ComChaveValida_DeveCriarECodigo201()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/funcionarios",
            new CadastroFuncionarioRequest(ChaveAdminTeste, "joao.silva", "senha123"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<CadastroFuncionarioResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal("joao.silva", corpo!.NmUsuario);

        var funcionarioCriado = await _funcionarioRepository.GetByUsernameAsync("joao.silva");
        Assert.NotNull(funcionarioCriado);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<Funcionario>().VerifyHashedPassword(
                funcionarioCriado!, funcionarioCriado.HashSenha, "senha123"));
    }

    [Fact]
    public async Task CadastroFuncionario_ComChaveInvalida_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/funcionarios",
            new CadastroFuncionarioRequest("chave-errada", "joao.silva", "senha123"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Null(await _funcionarioRepository.GetByUsernameAsync("joao.silva"));
    }

    [Fact]
    public async Task CadastroFuncionario_ComNomeUsuarioJaExistente_DeveRetornarConflict()
    {
        var existente = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva" };
        _funcionarioRepository.Adicionar(existente);

        var resposta = await _client.PostAsJsonAsync(
            "/api/funcionarios",
            new CadastroFuncionarioRequest(ChaveAdminTeste, "joao.silva", "senha123"));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastroFuncionario_ComCamposVazios_DeveRetornarBadRequest()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/funcionarios",
            new CadastroFuncionarioRequest(ChaveAdminTeste, "", ""));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
