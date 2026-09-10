using System.Net;
using System.Net.Http.Headers;
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

public class AcompanhamentoEndpointsTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";

    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly FakeFuncionarioRepository _funcionarioRepository = new();
    private readonly FakeAcompanhamentoRepository _acompanhamentoRepository = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static AcompanhamentoEndpointsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("Cadastro__Chave", "chave-de-cadastro-de-teste");
    }

    public AcompanhamentoEndpointsTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUsuarioRepository>();
                services.AddSingleton<IUsuarioRepository>(_usuarioRepository);

                services.RemoveAll<IFuncionarioRepository>();
                services.AddSingleton<IFuncionarioRepository>(_funcionarioRepository);

                services.RemoveAll<IAcompanhamentoRepository>();
                services.AddSingleton<IAcompanhamentoRepository>(_acompanhamentoRepository);
            });
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<string> LoginUsuarioAsync(string id, string senha)
    {
        var resposta = await _client.PostAsJsonAsync("/api/auth/usuario/login", new LoginRequest(id, senha));
        var corpo = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson);
        return corpo!.Token;
    }

    private void AdicionarUsuarioAtivo(string id, string senha, bool primeiroAcesso = false)
    {
        var usuario = new Usuario { IdUsuario = id, NmUsuario = "Samuel Silva", PrimeiroAcesso = primeiroAcesso };
        usuario.HashSenha = new PasswordHasher<Usuario>().HashPassword(usuario, senha);
        _usuarioRepository.Adicionar(usuario);
    }

    [Fact]
    public async Task ListarAcompanhamentos_SemToken_DeveRetornarUnauthorized()
    {
        var resposta = await _client.GetAsync("/api/acompanhamentos/");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task ListarAcompanhamentos_ComToken_DeveRetornarOkComLista()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        var acomp1 = new Acompanhamento { IdAcompanhamento = 1, DsAcompanhamento = "Salada", IdUsuario = "user01", SnDeletado = false };
        var acomp2 = new Acompanhamento { IdAcompanhamento = 2, DsAcompanhamento = "Arroz", IdUsuario = "user01", SnDeletado = false };
        _acompanhamentoRepository.Adicionar(acomp1);
        _acompanhamentoRepository.Adicionar(acomp2);

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/acompanhamentos/");
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<List<CadastroAcompanhamentoResponse>>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal(2, corpo!.Count);
        Assert.Contains(corpo, a => a.DsAcompanhamento == "Salada");
        Assert.Contains(corpo, a => a.DsAcompanhamento == "Arroz");
    }

    [Fact]
    public async Task ListarAcompanhamentos_SemAcompanhamentos_DeveRetornarListaVazia()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/acompanhamentos/");
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<List<CadastroAcompanhamentoResponse>>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Empty(corpo!);
    }

    [Fact]
    public async Task CadastrarAcompanhamento_SemToken_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync("/api/acompanhamentos/", new CadastroAcompanhamentoRequest("Salada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarAcompanhamento_ComTokenValido_DeveRetornarCreated()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/acompanhamentos/")
        {
            Content = JsonContent.Create(new CadastroAcompanhamentoRequest("Salada")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<CadastroAcompanhamentoResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal("Salada", corpo!.DsAcompanhamento);
        Assert.True(corpo.IdAcompanhamento > 0);
    }

    [Fact]
    public async Task CadastrarAcompanhamento_ComDescricaoEmBranco_DeveRetornarBadRequest()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/acompanhamentos/")
        {
            Content = JsonContent.Create(new CadastroAcompanhamentoRequest("   ")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarAcompanhamento_ComDescricaoDuplicada_DeveRetornarConflict()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        var acompanhamento = new Acompanhamento { IdAcompanhamento = 1, DsAcompanhamento = "Salada", IdUsuario = "user01", SnDeletado = false };
        _acompanhamentoRepository.Adicionar(acompanhamento);

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/acompanhamentos/")
        {
            Content = JsonContent.Create(new CadastroAcompanhamentoRequest("Salada")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task ObterAcompanhamento_ComIdValido_DeveRetornarOk()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        var acompanhamento = new Acompanhamento { IdAcompanhamento = 1, DsAcompanhamento = "Salada", IdUsuario = "user01", SnDeletado = false };
        _acompanhamentoRepository.Adicionar(acompanhamento);

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/acompanhamentos/1");
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<CadastroAcompanhamentoResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal("Salada", corpo!.DsAcompanhamento);
        Assert.Equal(1, corpo.IdAcompanhamento);
    }

    [Fact]
    public async Task ObterAcompanhamento_ComIdInvalido_DeveRetornarNotFound()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/acompanhamentos/999");
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}
