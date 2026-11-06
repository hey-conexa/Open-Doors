using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenDoors.Api.Models;

namespace OpenDoors.Api.Middleware
{
    /// <summary>
    /// Atributo customizado para validar se o usuário autenticado possui um dos tipos de usuário permitidos.
    /// Uso: [AuthorizeRole(TiposUsuario.Estudante)] ou [AuthorizeRole(TiposUsuario.Estudante, TiposUsuario.Empresa)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly TiposUsuario[] _allowedRoles;

        public AuthorizeRoleAttribute(params TiposUsuario[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var tipoUsuarioClaim = user.FindFirst("tipo_usuario")?.Value;

            if (string.IsNullOrEmpty(tipoUsuarioClaim))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!Enum.TryParse<TiposUsuario>(tipoUsuarioClaim, out var tipoUsuario))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!_allowedRoles.Contains(tipoUsuario))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}