using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Database.Entities;
using RefeicaoEntity = Refeicao.Core.Database.Entities.Refeicao;

namespace Refeicao.Core.Database.Context;

public class RefeicaoContext : DbContext
{
    private string ConnectionString = Environment.GetEnvironmentVariable("REFEICAO_CONNECTION_STRING")?? throw new InvalidOperationException("A variável de ambiente 'REFEICAO_CONNECTION_STRING' não está definida.");

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<RefeicaoEntity> Refeicoes => Set<RefeicaoEntity>();
    public DbSet<Cardapio> Cardapios => Set<Cardapio>();
    public DbSet<FuncionarioCardapio> FuncionarioCardapios => Set<FuncionarioCardapio>();
    public DbSet<Acompanhamento> Acompanhamentos => Set<Acompanhamento>();
    public DbSet<AcompanhamentoRefeicao> AcompanhamentosRefeicao => Set<AcompanhamentoRefeicao>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySQL(ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsuario(modelBuilder);
        ConfigureFuncionario(modelBuilder);
        ConfigureRefeicao(modelBuilder);
        ConfigureCardapio(modelBuilder);
        ConfigureFuncionarioCardapio(modelBuilder);
        ConfigureAcompanhamento(modelBuilder);
        ConfigureAcompanhamentoRefeicao(modelBuilder);
    }

    private static void ConfigureUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");

            entity.HasKey(u => u.IdUsuario);

            entity.Property(u => u.IdUsuario).HasColumnName("ID_USUARIO");
            entity.Property(u => u.NmUsuario).HasColumnName("NM_USUARIO");
            entity.Property(u => u.HashSenha).HasColumnName("HASH_SENHA");
            entity.Property(u => u.PrimeiroAcesso).HasColumnName("SN_PRIMEIRO_ACESSO");
            entity.Property(u => u.DtInativacao).HasColumnName("DT_INATIVACAO");
            entity.Property(u => u.DtCadastro).HasColumnName("DT_CADASTRO");

            entity.HasMany(u => u.Refeicoes)
                .WithOne(r => r.Usuario)
                .HasForeignKey(r => r.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.Cardapios)
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFuncionario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.ToTable("FUNCIONARIO");

            entity.HasKey(f => f.IdFuncionario);

            entity.Property(f => f.IdFuncionario).HasColumnName("ID_FUNCIONARIO");
            entity.Property(f => f.NmUsuario).HasColumnName("NM_USUARIO");
            entity.Property(f => f.HashSenha).HasColumnName("HASH_SENHA");
            entity.Property(f => f.PrimeiroAcesso).HasColumnName("SN_PRIMEIRO_ACESSO");
            entity.Property(f => f.DtCadastro).HasColumnName("DT_CADASTRO");
            entity.Property(f => f.DtInativacao).HasColumnName("DT_INATIVACAO");

            entity.HasMany(f => f.FuncionarioCardapios)
                .WithOne(fc => fc.Funcionario)
                .HasForeignKey(fc => fc.IdFuncionario)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRefeicao(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefeicaoEntity>(entity =>
        {
            entity.ToTable("REFEICAO");

            entity.HasKey(r => r.IdRefeicao);

            entity.Property(r => r.IdRefeicao).HasColumnName("ID_REFEICAO");
            entity.Property(r => r.DsRefeicao).HasColumnName("DS_REFEICAO");
            entity.Property(r => r.IdUsuario).HasColumnName("ID_USUARIO");

            entity.HasMany(r => r.Cardapios)
                .WithOne(c => c.Refeicao)
                .HasForeignKey(c => c.IdRefeicao)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(r => r.AcompanhamentosRefeicao)
                .WithOne(ar => ar.Refeicao)
                .HasForeignKey(ar => ar.IdRefeicao)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCardapio(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cardapio>(entity =>
        {
            entity.ToTable("CARDAPIO");

            entity.HasKey(c => c.IdCardapio);

            entity.Property(c => c.IdCardapio).HasColumnName("ID_CARDAPIO");
            entity.Property(c => c.DtCardapio).HasColumnName("DT_CARDAPIO");
            entity.Property(c => c.IdUsuario).HasColumnName("ID_USUARIO");
            entity.Property(c => c.IdRefeicao).HasColumnName("ID_REFEICAO");
            entity.Property(c => c.SnFechado).HasColumnName("SN_FECHADO");

            entity.Property(c => c.TpCardapio)
                .HasColumnName("TP_CARDAPIO")
                .HasConversion<string>()
                .HasColumnType("char(1)");

            entity.HasIndex(c => new { c.DtCardapio, c.TpCardapio }).IsUnique();

            entity.HasMany(c => c.FuncionarioCardapios)
                .WithOne(fc => fc.Cardapio)
                .HasForeignKey(fc => fc.IdCardapio)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureFuncionarioCardapio(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FuncionarioCardapio>(entity =>
        {
            entity.ToTable("FUNCIONARIO_CARDAPIO");

            entity.HasKey(fc => new { fc.IdFuncionario, fc.IdCardapio });

            entity.Property(fc => fc.IdFuncionario).HasColumnName("ID_FUNCIONARIO");
            entity.Property(fc => fc.IdCardapio).HasColumnName("ID_CARDAPIO");
            entity.Property(fc => fc.DtConfirmacao).HasColumnName("DT_CONFIRMACAO");
        });
    }

    private static void ConfigureAcompanhamento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Acompanhamento>(entity =>
        {
            entity.ToTable("ACOMPANHAMENTO");

            entity.HasKey(a => a.IdAcompanhamento);

            entity.Property(a => a.IdAcompanhamento).HasColumnName("ID_ACOMPANHAMENTO");
            entity.Property(a => a.DsAcompanhamento).HasColumnName("DS_ACOMPANHAMENTO");
            entity.Property(a => a.IdUsuario).HasColumnName("ID_USUARIO");
            entity.Property(a => a.IdUsuarioDeletou).HasColumnName("ID_USUARIO_DELETOU");
            entity.Property(a => a.SnDeletado).HasColumnName("SN_DELETADO");

            entity.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.UsuarioDeletou)
                .WithMany()
                .HasForeignKey(a => a.IdUsuarioDeletou)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAcompanhamentoRefeicao(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcompanhamentoRefeicao>(entity =>
        {
            entity.ToTable("ACOMPANHAMENTO_REFEICAO");

            entity.HasKey(ar => new { ar.IdAcompanhamento, ar.IdRefeicao });

            entity.Property(ar => ar.IdAcompanhamento).HasColumnName("ID_ACOMPANHAMENTO");
            entity.Property(ar => ar.IdRefeicao).HasColumnName("ID_REFEICAO");

            entity.HasOne(ar => ar.Acompanhamento)
                .WithMany(a => a.AcompanhamentosRefeicao)
                .HasForeignKey(ar => ar.IdAcompanhamento)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
