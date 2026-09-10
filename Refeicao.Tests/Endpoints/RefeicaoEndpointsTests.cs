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
using RefeicaoEntity = Refeicao.Core.Database.Entities.Refeicao;

namespace Refeicao.Tests.Endpoints;

public class RefeicaoEndpointsTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";

    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly FakeFuncionarioRepository _funcionarioRepository = new();
    private readonly FakeRefeicaoRepository _refeicaoRepository = new();
    private readonly FakeAcompanhamentoRefeicaoRepository _acompanhamentoRefeicaoRepository = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static RefeicaoEndpointsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("Cadastro__Chave", "chave-de-cadastro-de-teste");
    }

    public RefeicaoEndpointsTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUsuarioRepository>();
                services.AddSingleton<IUsuarioRepository>(_usuarioRepository);

                services.RemoveAll<IFuncionarioRepository>();
                services.AddSingleton<IFuncionarioRepository>(_funcionarioRepository);

                services.RemoveAll<IRefeicaoRepository>();
                services.AddSingleton<IRefeicaoRepository>(_refeicaoRepository);

                services.RemoveAll<IAcompanhamentoRefeicaoRepository>();
                services.AddSingleton<IAcompanhamentoRefeicaoRepository>(_acompanhamentoRefeicaoRepository);
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

    private async Task<string> LoginFuncionarioAsync(string id, string senha)
    {
        var resposta = await _client.PostAsJsonAsync("/api/auth/funcionario/login", new LoginRequest(id, senha));
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
    public async Task CadastrarRefeicao_SemToken_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync("/api/refeicao/", new CadastrarRefeicaoRequest("Frango Grelhado"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarRefeicao_ComTokenDeUsuarioValido_DeveRetornarCreated()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/")
        {
            Content = JsonContent.Create(new CadastrarRefeicaoRequest("Frango Grelhado")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<RefeicaoResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal("Frango Grelhado", corpo!.DsRefeicao);
        Assert.True(corpo.IdRefeicao > 0);

        var refeicaoSalva = await _refeicaoRepository.GetByIdAsync(corpo.IdRefeicao);
        Assert.NotNull(refeicaoSalva);
        Assert.Equal("user01", refeicaoSalva!.IdUsuario);
    }

    [Fact]
    public async Task CadastrarRefeicao_ComDescricaoEmBranco_DeveRetornarBadRequest()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/")
        {
            Content = JsonContent.Create(new CadastrarRefeicaoRequest("   ")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarRefeicao_ComUsuarioInativadoAposLogin_DeveRetornarUnauthorized()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        (await _usuarioRepository.GetByIdAsync("user01"))!.DtInativacao = DateTime.UtcNow;

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/")
        {
            Content = JsonContent.Create(new CadastrarRefeicaoRequest("Frango Grelhado")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarRefeicao_ComTokenDeFuncionario_DeveRetornarForbidden()
    {
        var funcionario = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva", PrimeiroAcesso = false };
        funcionario.HashSenha = new PasswordHasher<Funcionario>().HashPassword(funcionario, "senha123");
        _funcionarioRepository.Adicionar(funcionario);
        var token = await LoginFuncionarioAsync("joao.silva", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/")
        {
            Content = JsonContent.Create(new CadastrarRefeicaoRequest("Frango Grelhado")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarRefeicao_ComPrimeiroAcessoPendente_DeveRetornarForbidden()
    {
        AdicionarUsuarioAtivo("user01", "senha123", primeiroAcesso: true);
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/")
        {
            Content = JsonContent.Create(new CadastrarRefeicaoRequest("Frango Grelhado")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task ListarRefeicoes_SemToken_DeveRetornarUnauthorized()
    {
        var resposta = await _client.GetAsync("/api/refeicao/");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task ListarRefeicoes_ComToken_DeveRetornarOkComLista()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        var refeicao1 = new RefeicaoEntity { IdRefeicao = 1, DsRefeicao = "Frango Grelhado", IdUsuario = "user01" };
        var refeicao2 = new RefeicaoEntity { IdRefeicao = 2, DsRefeicao = "Peixe Cozido", IdUsuario = "user01" };
        await _refeicaoRepository.AddAsync(refeicao1);
        await _refeicaoRepository.AddAsync(refeicao2);

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/refeicao/");
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<List<RefeicaoComAcompanhamentosResponse>>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal(2, corpo!.Count);
        Assert.Contains(corpo, r => r.DsRefeicao == "Frango Grelhado");
        Assert.Contains(corpo, r => r.DsRefeicao == "Peixe Cozido");
    }

    [Fact]
    public async Task ListarRefeicoes_SemRefeicoes_DeveRetornarListaVazia()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/refeicao/");
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<List<RefeicaoComAcompanhamentosResponse>>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Empty(corpo!);
    }

    [Fact]
    public async Task CadastrarAcompanhamentoRefeicao_ComRefeicaoInexistente_DeveRetornarNotFound()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/999/acompanhamentos")
        {
            Content = JsonContent.Create(new CadastrarAcompanhamentoRefeicaoRequest(1)),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarAcompanhamentoRefeicao_ComValoresValidos_DeveRetornarCreated()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        var refeicao = new RefeicaoEntity { IdRefeicao = 1, DsRefeicao = "Frango Grelhado", IdUsuario = "user01" };
        await _refeicaoRepository.AddAsync(refeicao);

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/1/acompanhamentos")
        {
            Content = JsonContent.Create(new CadastrarAcompanhamentoRefeicaoRequest(1)),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var acompanhamentoSalvo = await _acompanhamentoRefeicaoRepository.GetByIdAsync((1, 1));
        Assert.NotNull(acompanhamentoSalvo);
        Assert.Equal(1, acompanhamentoSalvo!.IdAcompanhamento);
        Assert.Equal(1, acompanhamentoSalvo.IdRefeicao);
    }

    [Fact]
    public async Task CadastrarAcompanhamentoRefeicao_ComAcompanhamentoDuplicado_DeveRetornarConflict()
    {
        AdicionarUsuarioAtivo("user01", "senha123");
        var token = await LoginUsuarioAsync("user01", "senha123");

        var refeicao = new RefeicaoEntity { IdRefeicao = 1, DsRefeicao = "Frango Grelhado", IdUsuario = "user01" };
        await _refeicaoRepository.AddAsync(refeicao);

        var acompanhamentoRefeicao = new AcompanhamentoRefeicao { IdAcompanhamento = 1, IdRefeicao = 1 };
        await _acompanhamentoRefeicaoRepository.AddAsync(acompanhamentoRefeicao);

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/refeicao/1/acompanhamentos")
        {
            Content = JsonContent.Create(new CadastrarAcompanhamentoRefeicaoRequest(1)),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }
}
