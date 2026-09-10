namespace Refeicao.Core.Database.Entities;

public class Cardapio
{
    public int IdCardapio { get; set; }
    public TipoCardapio TpCardapio { get; set; }
    public DateOnly DtCardapio { get; set; }
    public string IdUsuario { get; set; } = string.Empty;
    public int IdRefeicao { get; set; }
    public bool SnFechado { get; set; } = false;

    public Usuario? Usuario { get; set; }
    public Refeicao? Refeicao { get; set; }
    public ICollection<FuncionarioCardapio> FuncionarioCardapios { get; set; } = [];
}
