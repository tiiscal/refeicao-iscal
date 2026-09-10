namespace Refeicao.Core.Database.Entities;

public class Funcionario
{
    public int IdFuncionario { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string HashSenha { get; set; } = string.Empty;
    public bool PrimeiroAcesso { get; set; } = true;
    public DateTime DtCadastro { get; set; }
    public DateTime? DtInativacao { get; set; }

    public ICollection<FuncionarioCardapio> FuncionarioCardapios { get; set; } = [];
}
