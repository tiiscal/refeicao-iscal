using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.TestDoubles;

public class FakeCardapioRepository : ICardapioRepository
{
    private readonly Dictionary<int, Cardapio> _cardapios = new();
    private int _proximoId = 1;

    public void Adicionar(Cardapio cardapio) => _cardapios[cardapio.IdCardapio] = cardapio;

    public Task<IReadOnlyList<Cardapio>> GetByPeriodoAsync(DateOnly inicio, DateOnly fim) =>
        Task.FromResult<IReadOnlyList<Cardapio>>(_cardapios.Values
            .Where(c => c.DtCardapio >= inicio && c.DtCardapio <= fim)
            .OrderBy(c => c.DtCardapio)
            .ThenBy(c => c.TpCardapio)
            .ToList());

    public Task<bool> ExistePorDataETipoAsync(DateOnly dtCardapio, TipoCardapio tpCardapio) =>
        Task.FromResult(_cardapios.Values.Any(c => c.DtCardapio == dtCardapio && c.TpCardapio == tpCardapio));

    public Task AddAsync(Cardapio cardapio)
    {
        cardapio.IdCardapio = _proximoId++;
        _cardapios[cardapio.IdCardapio] = cardapio;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
