namespace OpenDoors.Api.Models
{
    /// <summary>
    /// Código de 6 dígitos enviado por e-mail para redefinir a senha ("esqueci minha senha").
    /// Só o hash do código é guardado.
    /// </summary>
    public class CodigoRedefinicaoSenha
    {
        // ID auto-incremento
        public int Id { get; set; }

        // Conta (Login.ContaId) que pediu a redefinição
        public Guid ContaId { get; set; }

        // Hash (BCrypt) do código enviado por e-mail
        public string CodigoHash { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; }
        public DateTime ExpiraEm { get; set; }

        // Tentativas erradas; o código é invalidado ao atingir o limite
        public int Tentativas { get; set; }

        public bool Usado { get; set; }
    }
}
