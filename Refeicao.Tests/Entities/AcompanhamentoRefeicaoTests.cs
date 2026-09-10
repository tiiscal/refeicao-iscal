using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class AcompanhamentoRefeicaoTests
{
    [Fact]
    public void AcompanhamentoRefeicao_Acompanhamento_DeveSerNullPorPadrao()
    {
        var vinculo = new AcompanhamentoRefeicao();
        Assert.Null(vinculo.Acompanhamento);
    }

    [Fact]
    public void AcompanhamentoRefeicao_Refeicao_DeveSerNullPorPadrao()
    {
        var vinculo = new AcompanhamentoRefeicao();
        Assert.Null(vinculo.Refeicao);
    }

    [Fact]
    public void AcompanhamentoRefeicao_DeveAtribuirPropriedadesCorretamente()
    {
        var vinculo = new AcompanhamentoRefeicao
        {
            IdAcompanhamento = 3,
            IdRefeicao = 5
        };

        Assert.Equal(3, vinculo.IdAcompanhamento);
        Assert.Equal(5, vinculo.IdRefeicao);
    }

    [Fact]
    public void AcompanhamentoRefeicao_DeveAtribuirNavigationAcompanhamento()
    {
        var acompanhamento = new Acompanhamento { IdAcompanhamento = 3 };
        var vinculo = new AcompanhamentoRefeicao { Acompanhamento = acompanhamento };

        Assert.NotNull(vinculo.Acompanhamento);
        Assert.Equal(3, vinculo.Acompanhamento.IdAcompanhamento);
    }

    [Fact]
    public void AcompanhamentoRefeicao_DeveAtribuirNavigationRefeicao()
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao { IdRefeicao = 5 };
        var vinculo = new AcompanhamentoRefeicao { Refeicao = refeicao };

        Assert.NotNull(vinculo.Refeicao);
        Assert.Equal(5, vinculo.Refeicao.IdRefeicao);
    }
}
