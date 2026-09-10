using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class CardapioTests
{
    [Fact]
    public void Cardapio_SnFechado_DeveSerFalsePorPadrao()
    {
        var cardapio = new Cardapio();
        Assert.False(cardapio.SnFechado);
    }

    [Fact]
    public void Cardapio_Usuario_DeveSerNullPorPadrao()
    {
        var cardapio = new Cardapio();
        Assert.Null(cardapio.Usuario);
    }

    [Fact]
    public void Cardapio_Refeicao_DeveSerNullPorPadrao()
    {
        var cardapio = new Cardapio();
        Assert.Null(cardapio.Refeicao);
    }

    [Fact]
    public void Cardapio_FuncionarioCardapios_DeveIniciarVazio()
    {
        var cardapio = new Cardapio();
        Assert.Empty(cardapio.FuncionarioCardapios);
    }

    [Fact]
    public void Cardapio_DeveAtribuirPropriedadesCorretamente()
    {
        var cardapio = new Cardapio
        {
            IdCardapio = 10,
            TpCardapio = TipoCardapio.A,
            DtCardapio = new DateOnly(2024, 6, 10),
            IdUsuario = "user01",
            IdRefeicao = 5,
            SnFechado = true
        };

        Assert.Equal(10, cardapio.IdCardapio);
        Assert.Equal(TipoCardapio.A, cardapio.TpCardapio);
        Assert.Equal(new DateOnly(2024, 6, 10), cardapio.DtCardapio);
        Assert.Equal("user01", cardapio.IdUsuario);
        Assert.Equal(5, cardapio.IdRefeicao);
        Assert.True(cardapio.SnFechado);
    }

    [Theory]
    [InlineData(TipoCardapio.A)]
    [InlineData(TipoCardapio.J)]
    public void Cardapio_TpCardapio_DeveAceitarTodosOsTipos(TipoCardapio tipo)
    {
        var cardapio = new Cardapio { TpCardapio = tipo };
        Assert.Equal(tipo, cardapio.TpCardapio);
    }

    [Fact]
    public void Cardapio_FuncionarioCardapios_DeveAceitarNovoVinculo()
    {
        var cardapio = new Cardapio();
        cardapio.FuncionarioCardapios.Add(new FuncionarioCardapio { IdFuncionario = 1, IdCardapio = 10 });
        Assert.Single(cardapio.FuncionarioCardapios);
    }
}
