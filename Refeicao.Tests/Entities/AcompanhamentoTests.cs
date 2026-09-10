using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class AcompanhamentoTests
{
    [Fact]
    public void Acompanhamento_AcompanhamentosRefeicao_DeveIniciarVazio()
    {
        var acompanhamento = new Acompanhamento();
        Assert.Empty(acompanhamento.AcompanhamentosRefeicao);
    }

    [Fact]
    public void Acompanhamento_DeveAtribuirPropriedadesCorretamente()
    {
        var acompanhamento = new Acompanhamento
        {
            IdAcompanhamento = 3,
            DsAcompanhamento = "Arroz Integral"
        };

        Assert.Equal(3, acompanhamento.IdAcompanhamento);
        Assert.Equal("Arroz Integral", acompanhamento.DsAcompanhamento);
    }

    [Fact]
    public void Acompanhamento_AcompanhamentosRefeicao_DeveAceitarNovoVinculo()
    {
        var acompanhamento = new Acompanhamento();
        acompanhamento.AcompanhamentosRefeicao.Add(new AcompanhamentoRefeicao { IdAcompanhamento = 3, IdRefeicao = 1 });
        Assert.Single(acompanhamento.AcompanhamentosRefeicao);
    }
}
