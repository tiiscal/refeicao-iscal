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

public class UsuariosEndpointsTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";
    private const string ChaveAdminTeste = "chave-de-cadastro-de-teste";

    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static UsuariosEndpointsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("ADMIN_KEY", ChaveAdminTeste);
    }

    public UsuariosEndpointsTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUsuarioRepository>();
                services.AddSingleton<IUsuarioRepository>(_usuarioRepository);
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
    public async Task CadastroUsuario_ComChaveValida_DeveCriarECodigo201()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CadastroUsuarioRequest(ChaveAdminTeste, "user01", "Samuel Silva", "senha123"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<CadastroUsuarioResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal("user01", corpo!.IdUsuario);
        Assert.Equal("Samuel Silva", corpo.NmUsuario);

        var usuarioCriado = await _usuarioRepository.GetByIdAsync("user01");
        Assert.NotNull(usuarioCriado);
        Assert.NotEqual("senha123", usuarioCriado!.HashSenha);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<Usuario>().VerifyHashedPassword(usuarioCriado, usuarioCriado.HashSenha, "senha123"));
    }

    [Fact]
    public async Task CadastroUsuario_ComChaveInvalida_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CadastroUsuarioRequest("chave-errada", "user01", "Samuel Silva", "senha123"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Null(await _usuarioRepository.GetByIdAsync("user01"));
    }

    [Fact]
    public async Task CadastroUsuario_ComIdJaExistente_DeveRetornarConflict()
    {
        var existente = new Usuario { IdUsuario = "user01", NmUsuario = "Ja Existe" };
        _usuarioRepository.Adicionar(existente);

        var resposta = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CadastroUsuarioRequest(ChaveAdminTeste, "user01", "Samuel Silva", "senha123"));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastroUsuario_ComCamposVazios_DeveRetornarBadRequest()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CadastroUsuarioRequest(ChaveAdminTeste, "", "", ""));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Delete_ComChaveValida_DesativaUsuario()
    {
        var usuario = new Usuario
        {
            IdUsuario = "user01",
            NmUsuario = "Samuel Silva",
            HashSenha = "hash-senha",
            DtCadastro = DateTime.Now
        };

        _usuarioRepository.Adicionar(usuario);

        var response = await _client.DeleteAsync($"/api/usuarios/user01?chave={ChaveAdminTeste}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(usuario.DtInativacao);
    }

    [Fact]
    public async Task Delete_SemChave_RetornaUnauthorized()
    {
        var response = await _client.DeleteAsync("/api/usuarios/user01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ComChaveInvalida_RetornaUnauthorized()
    {
        var response = await _client.DeleteAsync("/api/usuarios/user01?chave=chave-invalida");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UsuarioNaoEncontrado_RetornaNotFound()
    {
        var response = await _client.DeleteAsync($"/api/usuarios/user999?chave={ChaveAdminTeste}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UsuarioJaDesativado_RetornaBadRequest()
    {
        var usuario = new Usuario
        {
            IdUsuario = "user01",
            NmUsuario = "Samuel Silva",
            HashSenha = "hash-senha",
            DtCadastro = DateTime.Now,
            DtInativacao = DateTime.Now.AddDays(-1)
        };

        _usuarioRepository.Adicionar(usuario);

        var response = await _client.DeleteAsync($"/api/usuarios/user01?chave={ChaveAdminTeste}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
