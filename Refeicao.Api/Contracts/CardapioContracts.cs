namespace Refeicao.Api.Contracts;

public record CardapioSemanaResponse(
    int IdCardapio,
    DateOnly DtCardapio,
    string TpCardapio,
    string DsRefeicao,
    IReadOnlyList<string> Acompanhamentos,
    bool SnFechado);

public record CadastrarCardapioRequest(int IdRefeicao, DateOnly DtCardapio, string TpCardapio);

public record CardapioResponse(int IdCardapio, DateOnly DtCardapio, string TpCardapio, int IdRefeicao, bool SnFechado);
