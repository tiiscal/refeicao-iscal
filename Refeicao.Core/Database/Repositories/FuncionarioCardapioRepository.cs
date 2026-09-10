using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Database.Repositories;

public class FuncionarioCardapioRepository(RefeicaoContext context) : IFuncionarioCardapioRepository
{
    public Task<FuncionarioCardapio?> GetByIdAsync((int IdFuncionario, int IdCardapio) id) =>
        context.FuncionarioCardapios.FirstOrDefaultAsync(
            fc => fc.IdFuncionario == id.IdFuncionario && fc.IdCardapio == id.IdCardapio);
}
