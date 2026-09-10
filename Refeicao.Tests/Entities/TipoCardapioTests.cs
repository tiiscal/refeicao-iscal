using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class TipoCardapioTests
{
    [Fact]
    public void TipoCardapio_DeveConterValorJanta()
    {
        Assert.True(Enum.IsDefined(typeof(TipoCardapio), TipoCardapio.J));
    }

    [Fact]
    public void TipoCardapio_DeveConterValorAlmoco()
    {
        Assert.True(Enum.IsDefined(typeof(TipoCardapio), TipoCardapio.A));
    }

    [Fact]
    public void TipoCardapio_DeveConterExatamenteDoisValores()
    {
        var valores = Enum.GetValues<TipoCardapio>();
        Assert.Equal(2, valores.Length);
    }

    [Theory]
    [InlineData("J")]
    [InlineData("A")]
    public void TipoCardapio_DeveConterNomeEsperado(string nome)
    {
        Assert.True(Enum.TryParse<TipoCardapio>(nome, out _));
    }
}
