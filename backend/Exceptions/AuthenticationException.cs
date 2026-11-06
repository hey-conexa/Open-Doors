namespace OpenDoors.Api.Exceptions
{
    /// <summary>
    /// Exceção lançada quando as credenciais de autenticação são inválidas.
    /// </summary>
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }
}