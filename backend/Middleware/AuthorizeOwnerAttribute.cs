using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OpenDoors.Api.Middleware
{
    /// <summary>
    /// Atributo customizado para validar se o usuário autenticado é o proprietário do recurso.
    /// Extrai o parameterName (ex: "estudanteId", "empresaId") da rota e compara com a conta do usuário.
    /// Uso: [AuthorizeOwner("estudanteId")] ou [AuthorizeOwner("empresaId")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeOwnerAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _parameterName;

        public AuthorizeOwnerAttribute(string parameterName)
        {
            _parameterName = parameterName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var contaIdClaim = user.FindFirst("conta_id")?.Value;

            if (string.IsNullOrEmpty(contaIdClaim) || !Guid.TryParse(contaIdClaim, out var contaId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var routeData = context.RouteData.Values;

            if (!routeData.TryGetValue(_parameterName, out var parameterValue))
            {
                context.Result = new BadRequestObjectResult(new { erro = $"Parâmetro '{_parameterName}' não encontrado na rota" });
                return;
            }

            if (!Guid.TryParse(parameterValue?.ToString(), out var resourceId))
            {
                context.Result = new BadRequestObjectResult(new { erro = $"Parâmetro '{_parameterName}' não é um GUID válido" });
                return;
            }

            if (contaId != resourceId)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}