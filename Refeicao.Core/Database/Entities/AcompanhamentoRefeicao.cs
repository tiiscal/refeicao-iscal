namespace Refeicao.Core.Database.Entities;

public class AcompanhamentoRefeicao
{
    public int IdAcompanhamento { get; set; }
    public int IdRefeicao { get; set; }

    public Acompanhamento? Acompanhamento { get; set; }
    public Refeicao? Refeicao { get; set; }
}
