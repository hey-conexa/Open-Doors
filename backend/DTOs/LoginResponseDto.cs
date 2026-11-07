namespace OpenDoors.Api.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEm { get; set; }
        public Guid ContaId { get; set; }

        // "Estudante" ou "Empresa"
        public string TipoUsuario { get; set; } = string.Empty;
    }
}
