namespace OpenDoors.Api.Exceptions
{
    /// <summary>
    /// Exceção lançada quando email ou senha estão incorretos.
    /// </summary>
    public class InvalidCredentialsException : AuthenticationException
    {
        public InvalidCredentialsException() : base("Email ou senha incorretos.") { }

        public InvalidCredentialsException(string message) : base(message) { }
    }
}