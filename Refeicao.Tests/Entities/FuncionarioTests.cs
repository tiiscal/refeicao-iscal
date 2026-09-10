using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class FuncionarioTests
{
    [Fact]
    public void Funcionario_PrimeiroAcesso_DeveSerTruePorPadrao()
    {
        var funcionario = new Funcionario();
        Assert.True(funcionario.PrimeiroAcesso);
    }

    [Fact]
    public void Funcionario_DtInativacao_DeveSerNullPorPadrao()
    {
        var funcionario = new Funcionario();
        Assert.Null(funcionario.DtInativacao);
    }

    [Fact]
    public void Funcionario_FuncionarioCardapios_DeveIniciarVazio()
    {
        var funcionario = new Funcionario();
        Assert.Empty(funcionario.FuncionarioCardapios);
    }

    [Fact]
    public void Funcionario_DeveAtribuirPropriedadesCorretamente()
    {
        var dtCadastro = new DateTime(2024, 3, 10);
        var dtInativacao = new DateTime(2025, 1, 1);

        var funcionario = new Funcionario
        {
            IdFuncionario = 42,
            NmUsuario = "joao.silva",
            HashSenha = "hash456",
            PrimeiroAcesso = false,
            DtCadastro = dtCadastro,
            DtInativacao = dtInativacao
        };

        Assert.Equal(42, funcionario.IdFuncionario);
        Assert.Equal("joao.silva", funcionario.NmUsuario);
        Assert.Equal("hash456", funcionario.HashSenha);
        Assert.False(funcionario.PrimeiroAcesso);
        Assert.Equal(dtCadastro, funcionario.DtCadastro);
        Assert.Equal(dtInativacao, funcionario.DtInativacao);
    }

    [Fact]
    public void Funcionario_FuncionarioCardapios_DeveAceitarNovoVinculo()
    {
        var funcionario = new Funcionario();
        funcionario.FuncionarioCardapios.Add(new FuncionarioCardapio { IdFuncionario = 1, IdCardapio = 10 });
        Assert.Single(funcionario.FuncionarioCardapios);
    }
}
