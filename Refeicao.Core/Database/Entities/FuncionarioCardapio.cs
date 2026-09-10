namespace Refeicao.Core.Database.Entities;

public class FuncionarioCardapio
{
    public int IdFuncionario { get; set; }
    public int IdCardapio { get; set; }
    public DateTime DtConfirmacao { get; set; }

    public Funcionario? Funcionario { get; set; }
    public Cardapio? Cardapio { get; set; }
}
