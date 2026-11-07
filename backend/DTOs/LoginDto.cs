namespace OpenDoors.Api.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginGoogleDto
    {
        // ID token devolvido pelo botão "Entrar com o Google"
        public string Credential { get; set; } = string.Empty;
    }

    public class EsqueciSenhaDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class RedefinirSenhaDto
    {
        public string Email { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string NovaSenha { get; set; } = string.Empty;
    }

    public class AlterarSenhaDto
    {
        public string SenhaAtual { get; set; } = string.Empty;
        public string NovaSenha { get; set; } = string.Empty;
    }
}
