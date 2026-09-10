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

public class CardapioEndpointsTests : IDisposable
{
    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly FakeFuncionarioRepository _funcionarioRepository = new();
    private readonly FakeRefeicaoRepository _refeicaoRepository = new();
    private readonly FakeCardapioRepository _cardapioRepository = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static CardapioEndpointsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256");
        Environment.SetEnvironmentVariable("Cadastro__Chave", "chave-de-cadastro-de-teste");
    }

    public CardapioEndpointsTests()
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

                services.RemoveAll<ICardapioRepository>();
                services.AddSingleton<ICardapioRepository>(_cardapioRepository);
            });
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static (DateOnly Inicio, DateOnly Fim) ObterSemanaAtual()
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        var diaSemana = (int)hoje.DayOfWeek;
        var diasDesdeSegunda = diaSemana == 0 ? 6 : diaSemana - 1;
        var inicio = hoje.AddDays(-diasDesdeSegunda);
        return (inicio, inicio.AddDays(6));
    }

    private static void SeedCardapioComRefeicao(
        FakeCardapioRepository repositorio, int idCardapio, DateOnly data, string dsRefeicao, params string[] acompanhamentos)
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao
        {
            IdRefeicao = idCardapio,
            DsRefeicao = dsRefeicao,
            IdUsuario = "user01",
            AcompanhamentosRefeicao = acompanhamentos.Select((ds, i) => new AcompanhamentoRefeicao
            {
                IdAcompanhamento = i + 1,
                IdRefeicao = idCardapio,
                Acompanhamento = new Acompanhamento { IdAcompanhamento = i + 1, DsAcompanhamento = ds },
            }).ToList(),
        };

        var cardapio = new Cardapio
        {
            IdCardapio = idCardapio,
            TpCardapio = TipoCardapio.A,
            DtCardapio = data,
            IdUsuario = "user01",
            IdRefeicao = idCardapio,
            Refeicao = refeicao,
        };

        repositorio.Adicionar(cardapio);
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

    private void AdicionarUsuarioAtivo(string id, bool primeiroAcesso = false)
    {
        var usuario = new Usuario { IdUsuario = id, NmUsuario = "Samuel Silva", PrimeiroAcesso = primeiroAcesso };
        usuario.HashSenha = new PasswordHasher<Usuario>().HashPassword(usuario, "senha123");
        _usuarioRepository.Adicionar(usuario);
    }

    private void AdicionarRefeicao(int idRefeicao, string dsRefeicao = "Frango Grelhado") =>
        _refeicaoRepository.Adicionar(
            new Refeicao.Core.Database.Entities.Refeicao { IdRefeicao = idRefeicao, DsRefeicao = dsRefeicao, IdUsuario = "user01" });

    [Fact]
    public async Task SemanaAtual_SemToken_DeveRetornarApenasCardapiosDaSemanaAtual()
    {
        var (inicio, fim) = ObterSemanaAtual();

        SeedCardapioComRefeicao(_cardapioRepository, 1, inicio, "Frango Grelhado", "Arroz", "Feijão");
        SeedCardapioComRefeicao(_cardapioRepository, 2, fim, "Peixe Assado");
        SeedCardapioComRefeicao(_cardapioRepository, 3, inicio.AddDays(-1), "Fora da semana (antes)");
        SeedCardapioComRefeicao(_cardapioRepository, 4, fim.AddDays(1), "Fora da semana (depois)");

        var resposta = await _client.GetAsync("/api/cardapio/semana-atual");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<List<CardapioSemanaResponse>>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal(2, corpo!.Count);

        var primeiro = corpo.Single(c => c.IdCardapio == 1);
        Assert.Equal("Frango Grelhado", primeiro.DsRefeicao);
        Assert.Equal(new[] { "Arroz", "Feijão" }, primeiro.Acompanhamentos);
        Assert.Equal("A", primeiro.TpCardapio);

        Assert.Contains(corpo, c => c.IdCardapio == 2);
        Assert.DoesNotContain(corpo, c => c.IdCardapio is 3 or 4);
    }

    [Fact]
    public async Task SemanaAtual_SemCardapiosCadastrados_DeveRetornarListaVazia()
    {
        var resposta = await _client.GetAsync("/api/cardapio/semana-atual");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<List<CardapioSemanaResponse>>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Empty(corpo!);
    }

    [Fact]
    public async Task CadastrarCardapio_SemToken_DeveRetornarUnauthorized()
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/cardapio/", new CadastrarCardapioRequest(1, DateOnly.FromDateTime(DateTime.Now), "A"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarCardapio_ComDadosValidos_DeveRetornarCreated()
    {
        AdicionarUsuarioAtivo("user01");
        AdicionarRefeicao(10);
        var token = await LoginUsuarioAsync("user01", "senha123");
        var data = DateOnly.FromDateTime(DateTime.Now);

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/cardapio/")
        {
            Content = JsonContent.Create(new CadastrarCardapioRequest(10, data, "A")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<CardapioResponse>(OpcoesJson);
        Assert.NotNull(corpo);
        Assert.Equal(data, corpo!.DtCardapio);
        Assert.Equal("A", corpo.TpCardapio);
        Assert.Equal(10, corpo.IdRefeicao);
        Assert.False(corpo.SnFechado);
        Assert.True(corpo.IdCardapio > 0);
    }

    [Fact]
    public async Task CadastrarCardapio_ComTipoInvalido_DeveRetornarBadRequest()
    {
        AdicionarUsuarioAtivo("user01");
        AdicionarRefeicao(10);
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/cardapio/")
        {
            Content = JsonContent.Create(new CadastrarCardapioRequest(10, DateOnly.FromDateTime(DateTime.Now), "X")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarCardapio_ComRefeicaoInexistente_DeveRetornarBadRequest()
    {
        AdicionarUsuarioAtivo("user01");
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/cardapio/")
        {
            Content = JsonContent.Create(new CadastrarCardapioRequest(999, DateOnly.FromDateTime(DateTime.Now), "A")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarCardapio_ComDataETipoJaCadastrados_DeveRetornarConflict()
    {
        AdicionarUsuarioAtivo("user01");
        AdicionarRefeicao(10);
        AdicionarRefeicao(11, "Peixe Assado");
        var token = await LoginUsuarioAsync("user01", "senha123");
        var data = DateOnly.FromDateTime(DateTime.Now);

        SeedCardapioComRefeicao(_cardapioRepository, 1, data, "Frango Grelhado");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/cardapio/")
        {
            Content = JsonContent.Create(new CadastrarCardapioRequest(11, data, "A")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarCardapio_ComUsuarioInativadoAposLogin_DeveRetornarUnauthorized()
    {
        AdicionarUsuarioAtivo("user01");
        AdicionarRefeicao(10);
        var token = await LoginUsuarioAsync("user01", "senha123");

        (await _usuarioRepository.GetByIdAsync("user01"))!.DtInativacao = DateTime.UtcNow;

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/cardapio/")
        {
            Content = JsonContent.Create(new CadastrarCardapioRequest(10, DateOnly.FromDateTime(DateTime.Now), "A")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarCardapio_ComTokenDeFuncionario_DeveRetornarForbidden()
    {
        var funcionario = new Funcionario { IdFuncionario = 1, NmUsuario = "joao.silva", PrimeiroAcesso = false };
        funcionario.HashSenha = new PasswordHasher<Funcionario>().HashPassword(funcionario, "senha123");
        _funcionarioRepository.Adicionar(funcionario);
        var token = await LoginFuncionarioAsync("joao.silva", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/cardapio/")
        {
            Content = JsonContent.Create(new CadastrarCardapioRequest(10, DateOnly.FromDateTime(DateTime.Now), "A")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task CadastrarCardapio_ComPrimeiroAcessoPendente_DeveRetornarForbidden()
    {
        AdicionarUsuarioAtivo("user01", primeiroAcesso: true);
        var token = await LoginUsuarioAsync("user01", "senha123");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/cardapio/")
        {
            Content = JsonContent.Create(new CadastrarCardapioRequest(10, DateOnly.FromDateTime(DateTime.Now), "A")),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
