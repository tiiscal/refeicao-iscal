using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Refeicao.Core.Abstractions.Repositories;

namespace Refeicao.Api.Authentication;

public class SenhaAlteradaHandler(IUsuarioRepository usuarioRepositorio, IFuncionarioRepository funcionarioRepositorio)
    : AuthorizationHandler<SenhaAlteradaRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, SenhaAlteradaRequirement requirement)
    {
        var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id is null)
            return;

        var primeiroAcesso = context.User.FindFirstValue(ClaimTypes.Role) switch
        {
            Papeis.Usuario => (await usuarioRepositorio.GetByIdAsync(id))?.PrimeiroAcesso,
            Papeis.Funcionario => (await funcionarioRepositorio.GetByIdAsync(int.Parse(id)))?.PrimeiroAcesso,
            _ => null,
        };

        if (primeiroAcesso == false)
            context.Succeed(requirement);
    }
}
