using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.Entities;

public class UsuarioTests
{
    [Fact]
    public void Usuario_PrimeiroAcesso_DeveSerTruePorPadrao()
    {
        var usuario = new Usuario();
        Assert.True(usuario.PrimeiroAcesso);
    }

    [Fact]
    public void Usuario_DtInativacao_DeveSerNullPorPadrao()
    {
        var usuario = new Usuario();
        Assert.Null(usuario.DtInativacao);
    }

    [Fact]
    public void Usuario_Refeicoes_DeveIniciarVazio()
    {
        var usuario = new Usuario();
        Assert.Empty(usuario.Refeicoes);
    }

    [Fact]
    public void Usuario_Cardapios_DeveIniciarVazio()
    {
        var usuario = new Usuario();
        Assert.Empty(usuario.Cardapios);
    }

    [Fact]
    public void Usuario_DeveAtribuirPropriedadesCorretamente()
    {
        var usuario = new Usuario
        {
            IdUsuario = "user01",
            NmUsuario = "Samuel Silva",
            HashSenha = "hash123",
            PrimeiroAcesso = false,
            DtCadastro = new DateTime(2024, 1, 15),
            DtInativacao = new DateTime(2025, 6, 1)
        };

        Assert.Equal("user01", usuario.IdUsuario);
        Assert.Equal("Samuel Silva", usuario.NmUsuario);
        Assert.Equal("hash123", usuario.HashSenha);
        Assert.False(usuario.PrimeiroAcesso);
        Assert.Equal(new DateTime(2024, 1, 15), usuario.DtCadastro);
        Assert.Equal(new DateTime(2025, 6, 1), usuario.DtInativacao);
    }

    [Fact]
    public void Usuario_Refeicoes_DeveAceitarNovaRefeicao()
    {
        var usuario = new Usuario();
        usuario.Refeicoes.Add(new Refeicao.Core.Database.Entities.Refeicao { IdRefeicao = 1 });
        Assert.Single(usuario.Refeicoes);
    }

    [Fact]
    public void Usuario_Cardapios_DeveAceitarNovoCardapio()
    {
        var usuario = new Usuario();
        usuario.Cardapios.Add(new Cardapio { IdCardapio = 1 });
        Assert.Single(usuario.Cardapios);
    }
}
