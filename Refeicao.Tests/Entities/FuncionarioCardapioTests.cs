using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class FuncionarioCardapioTests
{
    [Fact]
    public void FuncionarioCardapio_Funcionario_DeveSerNullPorPadrao()
    {
        var vinculo = new FuncionarioCardapio();
        Assert.Null(vinculo.Funcionario);
    }

    [Fact]
    public void FuncionarioCardapio_Cardapio_DeveSerNullPorPadrao()
    {
        var vinculo = new FuncionarioCardapio();
        Assert.Null(vinculo.Cardapio);
    }

    [Fact]
    public void FuncionarioCardapio_DeveAtribuirPropriedadesCorretamente()
    {
        var dtConfirmacao = new DateTime(2024, 7, 5);

        var vinculo = new FuncionarioCardapio
        {
            IdFuncionario = 1,
            IdCardapio = 10,
            DtConfirmacao = dtConfirmacao
        };

        Assert.Equal(1, vinculo.IdFuncionario);
        Assert.Equal(10, vinculo.IdCardapio);
        Assert.Equal(dtConfirmacao, vinculo.DtConfirmacao);
    }

    [Fact]
    public void FuncionarioCardapio_DeveAtribuirNavigationFuncionario()
    {
        var funcionario = new Funcionario { IdFuncionario = 1 };
        var vinculo = new FuncionarioCardapio { Funcionario = funcionario };

        Assert.NotNull(vinculo.Funcionario);
        Assert.Equal(1, vinculo.Funcionario.IdFuncionario);
    }

    [Fact]
    public void FuncionarioCardapio_DeveAtribuirNavigationCardapio()
    {
        var cardapio = new Cardapio { IdCardapio = 10 };
        var vinculo = new FuncionarioCardapio { Cardapio = cardapio };

        Assert.NotNull(vinculo.Cardapio);
        Assert.Equal(10, vinculo.Cardapio.IdCardapio);
    }
}
