namespace Refeicao.Core.Database.Entities;

public class Usuario
{
    public string IdUsuario { get; set; } = string.Empty;
    public string NmUsuario { get; set; } = string.Empty;
    public string HashSenha { get; set; } = string.Empty;
    public bool PrimeiroAcesso { get; set; } = true;
    public DateTime? DtInativacao { get; set; }
    public DateTime DtCadastro { get; set; }
    public ICollection<Refeicao> Refeicoes { get; set; } = [];
    public ICollection<Cardapio> Cardapios { get; set; } = [];
}
