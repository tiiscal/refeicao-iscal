using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;
using RefeicaoEntity = Refeicao.Core.Database.Entities.Refeicao;

namespace Refeicao.Tests.Database.Context;

public class RefeicaoContextTests
{
    static RefeicaoContextTests()
    {
        Environment.SetEnvironmentVariable(
            "REFEICAO_CONNECTION_STRING",
            "server=localhost;port=3306;database=refeicao;user=root;password=;");
    }

    private static IModel CriarModelo()
    {
        using var context = new RefeicaoContext();
        return context.Model;
    }

    private static IEntityType ObterEntidade<T>(IModel model) => model.FindEntityType(typeof(T))!;

    [Fact]
    public void RefeicaoContext_DeveSerConfiguradoParaMySql()
    {
        using var context = new RefeicaoContext();
        Assert.Equal("MySql.EntityFrameworkCore", context.Database.ProviderName);
    }

    [Theory]
    [InlineData(typeof(Usuario), "USUARIO")]
    [InlineData(typeof(Funcionario), "FUNCIONARIO")]
    [InlineData(typeof(RefeicaoEntity), "REFEICAO")]
    [InlineData(typeof(Cardapio), "CARDAPIO")]
    [InlineData(typeof(FuncionarioCardapio), "FUNCIONARIO_CARDAPIO")]
    [InlineData(typeof(Acompanhamento), "ACOMPANHAMENTO")]
    [InlineData(typeof(AcompanhamentoRefeicao), "ACOMPANHAMENTO_REFEICAO")]
    public void RefeicaoContext_DeveMapearNomeDaTabela(Type tipoEntidade, string nomeTabelaEsperado)
    {
        var modelo = CriarModelo();
        var entidade = modelo.FindEntityType(tipoEntidade)!;

        Assert.Equal(nomeTabelaEsperado, entidade.GetTableName());
    }

    [Fact]
    public void RefeicaoContext_Usuario_DeveMapearColunasEChavePrimaria()
    {
        var entidade = ObterEntidade<Usuario>(CriarModelo());

        Assert.Equal(new[] { "ID_USUARIO" }, entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));

        Assert.Equal("ID_USUARIO", entidade.FindProperty(nameof(Usuario.IdUsuario))!.GetColumnName());
        Assert.Equal("NM_USUARIO", entidade.FindProperty(nameof(Usuario.NmUsuario))!.GetColumnName());
        Assert.Equal("HASH_SENHA", entidade.FindProperty(nameof(Usuario.HashSenha))!.GetColumnName());
        Assert.Equal("SN_PRIMEIRO_ACESSO", entidade.FindProperty(nameof(Usuario.PrimeiroAcesso))!.GetColumnName());
        Assert.Equal("DT_INATIVACAO", entidade.FindProperty(nameof(Usuario.DtInativacao))!.GetColumnName());
        Assert.Equal("DT_CADASTRO", entidade.FindProperty(nameof(Usuario.DtCadastro))!.GetColumnName());
    }

    [Fact]
    public void RefeicaoContext_Funcionario_DeveMapearColunasEChavePrimaria()
    {
        var entidade = ObterEntidade<Funcionario>(CriarModelo());

        Assert.Equal(new[] { "ID_FUNCIONARIO" }, entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));

        Assert.Equal("ID_FUNCIONARIO", entidade.FindProperty(nameof(Funcionario.IdFuncionario))!.GetColumnName());
        Assert.Equal("NM_USUARIO", entidade.FindProperty(nameof(Funcionario.NmUsuario))!.GetColumnName());
        Assert.Equal("HASH_SENHA", entidade.FindProperty(nameof(Funcionario.HashSenha))!.GetColumnName());
        Assert.Equal("SN_PRIMEIRO_ACESSO", entidade.FindProperty(nameof(Funcionario.PrimeiroAcesso))!.GetColumnName());
        Assert.Equal("DT_CADASTRO", entidade.FindProperty(nameof(Funcionario.DtCadastro))!.GetColumnName());
        Assert.Equal("DT_INATIVACAO", entidade.FindProperty(nameof(Funcionario.DtInativacao))!.GetColumnName());
    }

    [Fact]
    public void RefeicaoContext_Refeicao_DeveMapearColunasEChavePrimaria()
    {
        var entidade = ObterEntidade<RefeicaoEntity>(CriarModelo());

        Assert.Equal(new[] { "ID_REFEICAO" }, entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));

        Assert.Equal("ID_REFEICAO", entidade.FindProperty(nameof(RefeicaoEntity.IdRefeicao))!.GetColumnName());
        Assert.Equal("DS_REFEICAO", entidade.FindProperty(nameof(RefeicaoEntity.DsRefeicao))!.GetColumnName());
        Assert.Equal("ID_USUARIO", entidade.FindProperty(nameof(RefeicaoEntity.IdUsuario))!.GetColumnName());
    }

    [Fact]
    public void RefeicaoContext_Cardapio_DeveMapearColunasEChavePrimaria()
    {
        var entidade = ObterEntidade<Cardapio>(CriarModelo());

        Assert.Equal(new[] { "ID_CARDAPIO" }, entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));

        Assert.Equal("ID_CARDAPIO", entidade.FindProperty(nameof(Cardapio.IdCardapio))!.GetColumnName());
        Assert.Equal("DT_CARDAPIO", entidade.FindProperty(nameof(Cardapio.DtCardapio))!.GetColumnName());
        Assert.Equal("ID_USUARIO", entidade.FindProperty(nameof(Cardapio.IdUsuario))!.GetColumnName());
        Assert.Equal("ID_REFEICAO", entidade.FindProperty(nameof(Cardapio.IdRefeicao))!.GetColumnName());
        Assert.Equal("SN_FECHADO", entidade.FindProperty(nameof(Cardapio.SnFechado))!.GetColumnName());
    }

    [Fact]
    public void RefeicaoContext_Cardapio_DeveConverterTpCardapioParaStringEmColunaChar1()
    {
        var entidade = ObterEntidade<Cardapio>(CriarModelo());
        var propriedade = entidade.FindProperty(nameof(Cardapio.TpCardapio))!;

        Assert.Equal("TP_CARDAPIO", propriedade.GetColumnName());
        Assert.Equal("char(1)", propriedade.GetColumnType());
        Assert.Equal(typeof(string), propriedade.FindRelationalTypeMapping()!.Converter!.ProviderClrType);
    }

    [Fact]
    public void RefeicaoContext_Cardapio_DeveTerIndiceUnicoPorDataETipo()
    {
        var entidade = ObterEntidade<Cardapio>(CriarModelo());
        var indice = Assert.Single(entidade.GetIndexes(), i => i.IsUnique);

        Assert.Equal(
            new[] { "DT_CARDAPIO", "TP_CARDAPIO" },
            indice.Properties.Select(p => p.GetColumnName()));
    }

    [Fact]
    public void RefeicaoContext_FuncionarioCardapio_DeveMapearColunasEChaveComposta()
    {
        var entidade = ObterEntidade<FuncionarioCardapio>(CriarModelo());

        Assert.Equal(
            new[] { "ID_FUNCIONARIO", "ID_CARDAPIO" },
            entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));

        Assert.Equal("ID_FUNCIONARIO", entidade.FindProperty(nameof(FuncionarioCardapio.IdFuncionario))!.GetColumnName());
        Assert.Equal("ID_CARDAPIO", entidade.FindProperty(nameof(FuncionarioCardapio.IdCardapio))!.GetColumnName());
        Assert.Equal("DT_CONFIRMACAO", entidade.FindProperty(nameof(FuncionarioCardapio.DtConfirmacao))!.GetColumnName());
    }

    [Fact]
    public void RefeicaoContext_Acompanhamento_DeveMapearColunasEChavePrimaria()
    {
        var entidade = ObterEntidade<Acompanhamento>(CriarModelo());

        Assert.Equal(new[] { "ID_ACOMPANHAMENTO" }, entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));

        Assert.Equal("ID_ACOMPANHAMENTO", entidade.FindProperty(nameof(Acompanhamento.IdAcompanhamento))!.GetColumnName());
        Assert.Equal("DS_ACOMPANHAMENTO", entidade.FindProperty(nameof(Acompanhamento.DsAcompanhamento))!.GetColumnName());
    }

    [Fact]
    public void RefeicaoContext_AcompanhamentoRefeicao_DeveMapearColunasEChaveComposta()
    {
        var entidade = ObterEntidade<AcompanhamentoRefeicao>(CriarModelo());

        Assert.Equal(
            new[] { "ID_ACOMPANHAMENTO", "ID_REFEICAO" },
            entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()));

        Assert.Equal("ID_ACOMPANHAMENTO", entidade.FindProperty(nameof(AcompanhamentoRefeicao.IdAcompanhamento))!.GetColumnName());
        Assert.Equal("ID_REFEICAO", entidade.FindProperty(nameof(AcompanhamentoRefeicao.IdRefeicao))!.GetColumnName());
    }

    private static IForeignKey ObterFkUnica(IEntityType entidade) => Assert.Single(entidade.GetForeignKeys());

    [Fact]
    public void RefeicaoContext_Refeicao_DeveTerRelacionamentoRestritoComUsuario()
    {
        var modelo = CriarModelo();
        var fk = ObterFkUnica(ObterEntidade<RefeicaoEntity>(modelo));

        Assert.Equal(nameof(Usuario), fk.PrincipalEntityType.ClrType.Name);
        Assert.Equal(nameof(RefeicaoEntity.IdUsuario), Assert.Single(fk.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void RefeicaoContext_Cardapio_DeveTerRelacionamentosComUsuarioERefeicao()
    {
        var modelo = CriarModelo();
        var foreignKeys = ObterEntidade<Cardapio>(modelo).GetForeignKeys().ToList();

        var fkUsuario = Assert.Single(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(Usuario));
        Assert.Equal(nameof(Cardapio.IdUsuario), Assert.Single(fkUsuario.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fkUsuario.DeleteBehavior);

        var fkRefeicao = Assert.Single(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(RefeicaoEntity));
        Assert.Equal(nameof(Cardapio.IdRefeicao), Assert.Single(fkRefeicao.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fkRefeicao.DeleteBehavior);
    }

    [Fact]
    public void RefeicaoContext_FuncionarioCardapio_DeveTerRelacionamentosEmCascataComFuncionarioECardapio()
    {
        var modelo = CriarModelo();
        var foreignKeys = ObterEntidade<FuncionarioCardapio>(modelo).GetForeignKeys().ToList();

        var fkFuncionario = Assert.Single(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(Funcionario));
        Assert.Equal(nameof(FuncionarioCardapio.IdFuncionario), Assert.Single(fkFuncionario.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fkFuncionario.DeleteBehavior);

        var fkCardapio = Assert.Single(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(Cardapio));
        Assert.Equal(nameof(FuncionarioCardapio.IdCardapio), Assert.Single(fkCardapio.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fkCardapio.DeleteBehavior);
    }

    [Fact]
    public void RefeicaoContext_AcompanhamentoRefeicao_DeveTerRelacionamentosEmCascataComAcompanhamentoERefeicao()
    {
        var modelo = CriarModelo();
        var foreignKeys = ObterEntidade<AcompanhamentoRefeicao>(modelo).GetForeignKeys().ToList();

        var fkAcompanhamento = Assert.Single(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(Acompanhamento));
        Assert.Equal(nameof(AcompanhamentoRefeicao.IdAcompanhamento), Assert.Single(fkAcompanhamento.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fkAcompanhamento.DeleteBehavior);

        var fkRefeicao = Assert.Single(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(RefeicaoEntity));
        Assert.Equal(nameof(AcompanhamentoRefeicao.IdRefeicao), Assert.Single(fkRefeicao.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fkRefeicao.DeleteBehavior);
    }

    [Theory]
    [InlineData(typeof(Usuario))]
    [InlineData(typeof(Funcionario))]
    [InlineData(typeof(RefeicaoEntity))]
    [InlineData(typeof(Cardapio))]
    [InlineData(typeof(FuncionarioCardapio))]
    [InlineData(typeof(Acompanhamento))]
    [InlineData(typeof(AcompanhamentoRefeicao))]
    public void RefeicaoContext_TodasAsEntidadesDevemEstarRegistradasNoModelo(Type tipoEntidade)
    {
        var modelo = CriarModelo();
        Assert.NotNull(modelo.FindEntityType(tipoEntidade));
    }
}
