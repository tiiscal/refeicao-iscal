using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;
using Refeicao.Tests.TestDoubles;

namespace Refeicao.Tests.Endpoints;

public class FuncionarioEndpointsTests : IDisposable
{
    private const string ChaveJwtTeste = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";
    private const string ChaveAdminTeste = "chave-de-cadastro-de-teste";

    private readonly FakeFuncionarioRepository _funcionarioRepository = new();
    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    static FuncionarioEndpointsTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveJwtTeste);
        Environment.SetEnvironmentVariable("ADMIN_KEY", ChaveAdminTeste);
    }

    public FuncionarioEndpointsTests()
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

    [Fact]
    public async Task Delete_ComChaveValida_DesativaFuncionario()
    {
        var funcionario = new Funcionario
        {
            IdFuncionario = 1,
            NmUsuario = "funcionario-teste",
            HashSenha = "hash-senha",
            DtCadastro = DateTime.Now
        };

        _funcionarioRepository.Funcionarios[funcionario.IdFuncionario] = funcionario;

        var response = await _client.DeleteAsync($"/api/funcionario/1?chave={ChaveAdminTeste}");

        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(funcionario.DtInativacao);
    }

    [Fact]
    public async Task Delete_SemChave_RetornaUnauthorized()
    {
        var response = await _client.DeleteAsync("/api/funcionario/1");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ComChaveInvalida_RetornaUnauthorized()
    {
        var response = await _client.DeleteAsync("/api/funcionario/1?chave=chave-invalida");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_FuncionarioNaoEncontrado_RetornaNotFound()
    {
        var response = await _client.DeleteAsync($"/api/funcionario/999?chave={ChaveAdminTeste}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_FuncionarioJaDesativado_RetornaBadRequest()
    {
        var funcionario = new Funcionario
        {
            IdFuncionario = 1,
            NmUsuario = "funcionario-teste",
            HashSenha = "hash-senha",
            DtCadastro = DateTime.Now,
            DtInativacao = DateTime.Now.AddDays(-1)
        };

        _funcionarioRepository.Funcionarios[funcionario.IdFuncionario] = funcionario;

        var response = await _client.DeleteAsync($"/api/funcionario/1?chave={ChaveAdminTeste}");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
