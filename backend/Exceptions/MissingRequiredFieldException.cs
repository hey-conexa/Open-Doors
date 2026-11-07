namespace OpenDoors.Api.Exceptions
{
    /// <summary>
    /// Exceção lançada quando um campo obrigatório está ausente ou vazio.
    /// </summary>
    public class MissingRequiredFieldException : AuthenticationException
    {
        public MissingRequiredFieldException(string fieldName) 
            : base($"{fieldName} é obrigatório.") { }
    }
}