using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refeicao.Api.Authentication;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;
using Refeicao.Tests.TestDoubles;

namespace Refeicao.Tests.Endpoints;

public class AuthEndpointsTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";

    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly FakeFuncionarioRepository _funcionarioRepository = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static AuthEndpointsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("Cadastro__Chave", "chave-de-cadastro-de-teste");
    }

    public AuthEndpointsTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUsuarioRepository>();
                services.AddSingleton<IUsuarioRepository>(_usuarioRepository);

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

    private static string HashSenha(Usuario usuario, string senha) =>
        new PasswordHasher<Usuario>().HashPassword(usuario, senha);

    private static string HashSenha(Funcionario funcionario, string senha) =>
        new PasswordHasher<Funcionario>().HashPassword(funcionario, senha);

    private async Task<string> LoginUsuarioAsync(string id, string senha)
    {
        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest(id, senha));
        var corpo = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson);
        return corpo!.Token;
    }

    private async Task<string> LoginFuncionarioAsync(string id, string senha)
    {
        var resposta = await _client.PostAsJsonAsync("/api/auth/funcionario/login", new LoginRequest(id, senha));
        var corpo = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson);
        return corpo!.Token;
    }

    [Fact]
    public async Task UsuarioLogin_ComCredenciaisValidas_DeveRetornarTokenEPapel()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva" };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);

        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest("user01", "senha123"));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.False(string.IsNullOrWhiteSpace(corpo!.Token));
        Assert.Equal(Papeis.Usuario, corpo.Papel);
        Assert.True(corpo.PrimeiroAcesso);
    }

    [Fact]
    public async Task UsuarioLogin_ComPrimeiroAcessoJaConcluido_DeveRetornarPrimeiroAcessoFalse()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva", PrimeiroAcesso = false };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);

        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest("user01", "senha123"));

        var corpo = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.False(corpo!.PrimeiroAcesso);
    }

    [Fact]
    public async Task UsuarioLogin_ComSenhaIncorreta_DeveRetornarUnauthorized()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva" };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);

        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest("user01", "senhaErrada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task UsuarioLogin_ComIdInexistente_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest("inexistente", "qualquer"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task UsuarioLogin_ComUsuarioInativo_DeveRetornarUnauthorized()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva", DtInativacao = DateTime.UtcNow };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);

        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest("user01", "senha123"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task FuncionarioLogin_ComCredenciaisValidas_DeveRetornarTokenEPapel()
    {
        var funcionario = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva" };
        funcionario.HashSenha = HashSenha(funcionario, "senha123");
        _funcionarioRepository.Adicionar(funcionario);

        var resposta = await _client.PostAsJsonAsync("/api/auth/funcionario/login", new LoginRequest("joao.silva", "senha123"));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.False(string.IsNullOrWhiteSpace(corpo!.Token));
        Assert.Equal(Papeis.Funcionario, corpo.Papel);
        Assert.True(corpo.PrimeiroAcesso);
    }

    [Fact]
    public async Task FuncionarioLogin_ComPrimeiroAcessoJaConcluido_DeveRetornarPrimeiroAcessoFalse()
    {
        var funcionario = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva", PrimeiroAcesso = false };
        funcionario.HashSenha = HashSenha(funcionario, "senha123");
        _funcionarioRepository.Adicionar(funcionario);

        var resposta = await _client.PostAsJsonAsync(
            "/api/auth/funcionario/login", new LoginRequest("joao.silva", "senha123"));

        var corpo = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.False(corpo!.PrimeiroAcesso);
    }

    [Fact]
    public async Task FuncionarioLogin_ComSenhaIncorreta_DeveRetornarUnauthorized()
    {
        var funcionario = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva" };
        funcionario.HashSenha = HashSenha(funcionario, "senha123");
        _funcionarioRepository.Adicionar(funcionario);

        var resposta = await _client.PostAsJsonAsync("/api/auth/funcionario/login", new LoginRequest("joao.silva", "senhaErrada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task FuncionarioLogin_ComUsuarioInexistente_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync("/api/auth/funcionario/login", new LoginRequest("inexistente", "qualquer"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task FuncionarioLogin_ComFuncionarioInativo_DeveRetornarUnauthorized()
    {
        var funcionario = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva", DtInativacao = DateTime.UtcNow };
        funcionario.HashSenha = HashSenha(funcionario, "senha123");
        _funcionarioRepository.Adicionar(funcionario);

        var resposta = await _client.PostAsJsonAsync("/api/auth/funcionario/login", new LoginRequest("joao.silva", "senha123"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task UsuarioLogin_ApósDezRequisicoes_DeveRetornarTooManyRequests()
    {
        for (var i = 0; i < 10; i++)
        {
            var resposta = await _client.PostAsJsonAsync(
                "/api/auth/usuario/login", new LoginRequest("inexistente", "qualquer"));

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        var respostaExcedente = await _client.PostAsJsonAsync(
            "/api/auth/usuario/login", new LoginRequest("inexistente", "qualquer"));

        Assert.Equal(HttpStatusCode.TooManyRequests, respostaExcedente.StatusCode);
    }

    [Fact]
    public async Task UsuarioAlterarSenha_ComSenhaAtualCorreta_DeveRetornarNoContentEDesativarPrimeiroAcesso()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva" };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/auth/usuario/alterar-senha")
        {
            Content = JsonContent.Create(new AlterarSenhaRequest("senha123", "novaSenha456")),
        };
        requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.False((await _usuarioRepository.GetByIdAsync("user01"))!.PrimeiroAcesso);
    }

    [Fact]
    public async Task UsuarioAlterarSenha_SemToken_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/auth/usuario/alterar-senha", new AlterarSenhaRequest("senha123", "novaSenha456"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task UsuarioAlterarSenha_ComSenhaAtualIncorreta_DeveRetornarUnauthorized()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva" };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/auth/usuario/alterar-senha")
        {
            Content = JsonContent.Create(new AlterarSenhaRequest("senhaErrada", "novaSenha456")),
        };
        requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task UsuarioAlterarSenha_ComNovaSenhaIgualAAtual_DeveRetornarBadRequest()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva" };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/auth/usuario/alterar-senha")
        {
            Content = JsonContent.Create(new AlterarSenhaRequest("senha123", "senha123")),
        };
        requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task FuncionarioAlterarSenha_ComSenhaAtualCorreta_DeveRetornarNoContentEDesativarPrimeiroAcesso()
    {
        var funcionario = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva" };
        funcionario.HashSenha = HashSenha(funcionario, "senha123");
        _funcionarioRepository.Adicionar(funcionario);
        var token = await LoginFuncionarioAsync("joao.silva", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/auth/funcionario/alterar-senha")
        {
            Content = JsonContent.Create(new AlterarSenhaRequest("senha123", "novaSenha456")),
        };
        requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.False((await _funcionarioRepository.GetByIdAsync(1))!.PrimeiroAcesso);
    }

    [Fact]
    public async Task FuncionarioAlterarSenha_ComTokenDeUsuario_DeveRetornarForbidden()
    {
        var usuario = new Usuario { IdUsuario = "user01", NmUsuario = "Samuel Silva" };
        usuario.HashSenha = HashSenha(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/auth/funcionario/alterar-senha")
        {
            Content = JsonContent.Create(new AlterarSenhaRequest("senha123", "novaSenha456")),
        };
        requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
