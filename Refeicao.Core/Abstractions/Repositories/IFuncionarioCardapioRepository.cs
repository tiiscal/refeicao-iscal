using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Abstractions.Repositories;

public interface IFuncionarioCardapioRepository
{
    Task<FuncionarioCardapio?> GetByIdAsync((int IdFuncionario, int IdCardapio) id);
}
