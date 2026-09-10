namespace Refeicao.Core.Database.Entities;

public class Acompanhamento
{
    public int IdAcompanhamento { get; set; }
    public string DsAcompanhamento { get; set; } = string.Empty;
    public string IdUsuario { get; set; } = string.Empty;
    public string? IdUsuarioDeletou { get; set; }
    public bool SnDeletado { get; set; }

    public Usuario? Usuario { get; set; }
    public Usuario? UsuarioDeletou { get; set; }
    public ICollection<AcompanhamentoRefeicao> AcompanhamentosRefeicao { get; set; } = [];
}
