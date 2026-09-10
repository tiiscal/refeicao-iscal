namespace Refeicao.Api.Contracts;

public record CadastrarRefeicaoRequest(string DsRefeicao);

public record RefeicaoResponse(int IdRefeicao, string DsRefeicao);

public record AcompanhamentoItem(int IdAcompanhamento, string DsAcompanhamento);

public record RefeicaoComAcompanhamentosResponse(
    int IdRefeicao,
    string DsRefeicao,
    IReadOnlyList<AcompanhamentoItem> Acompanhamentos);

public record CadastrarAcompanhamentoRefeicaoRequest(int IdAcompanhamento);
