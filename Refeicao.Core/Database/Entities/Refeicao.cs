namespace Refeicao.Core.Database.Entities;

public class Refeicao
{
    public int IdRefeicao { get; set; }
    public string DsRefeicao { get; set; } = string.Empty;
    public string IdUsuario { get; set; } = string.Empty;

    public Usuario? Usuario { get; set; }
    public ICollection<Cardapio> Cardapios { get; set; } = [];
    public ICollection<AcompanhamentoRefeicao> AcompanhamentosRefeicao { get; set; } = [];
}
