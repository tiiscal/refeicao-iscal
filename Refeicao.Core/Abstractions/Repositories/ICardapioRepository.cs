using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Abstractions.Repositories;

public interface ICardapioRepository
{
    Task<IReadOnlyList<Cardapio>> GetByPeriodoAsync(DateOnly inicio, DateOnly fim);
    Task<bool> ExistePorDataETipoAsync(DateOnly dtCardapio, TipoCardapio tpCardapio);
    Task AddAsync(Cardapio cardapio);
    Task<int> SaveChangesAsync();
}
