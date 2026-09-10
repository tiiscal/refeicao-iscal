namespace Refeicao.Api.Contracts;

public record CadastroAcompanhamentoRequest(string DsAcompanhamento);

public record CadastroAcompanhamentoResponse(int IdAcompanhamento, string DsAcompanhamento);
