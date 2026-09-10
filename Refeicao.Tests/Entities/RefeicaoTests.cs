using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class RefeicaoTests
{
    [Fact]
    public void Refeicao_Usuario_DeveSerNullPorPadrao()
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao();
        Assert.Null(refeicao.Usuario);
    }

    [Fact]
    public void Refeicao_Cardapios_DeveIniciarVazio()
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao();
        Assert.Empty(refeicao.Cardapios);
    }

    [Fact]
    public void Refeicao_AcompanhamentosRefeicao_DeveIniciarVazio()
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao();
        Assert.Empty(refeicao.AcompanhamentosRefeicao);
    }

    [Fact]
    public void Refeicao_DeveAtribuirPropriedadesCorretamente()
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao
        {
            IdRefeicao = 5,
            DsRefeicao = "Frango Grelhado",
            IdUsuario = "user01"
        };

        Assert.Equal(5, refeicao.IdRefeicao);
        Assert.Equal("Frango Grelhado", refeicao.DsRefeicao);
        Assert.Equal("user01", refeicao.IdUsuario);
    }

    [Fact]
    public void Refeicao_Cardapios_DeveAceitarNovoCardapio()
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao();
        refeicao.Cardapios.Add(new Cardapio { IdCardapio = 1 });
        Assert.Single(refeicao.Cardapios);
    }

    [Fact]
    public void Refeicao_AcompanhamentosRefeicao_DeveAceitarNovoAcompanhamento()
    {
        var refeicao = new Refeicao.Core.Database.Entities.Refeicao();
        refeicao.AcompanhamentosRefeicao.Add(new AcompanhamentoRefeicao { IdRefeicao = 1, IdAcompanhamento = 2 });
        Assert.Single(refeicao.AcompanhamentosRefeicao);
    }
}
