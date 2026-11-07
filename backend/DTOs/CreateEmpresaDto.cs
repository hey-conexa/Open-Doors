namespace OpenDoors.Api.DTOs
{
    /// <summary>
    /// DTO de entrada para criar uma Empresa.
    /// O Id da empresa é gerado pelo backend no cadastro; o campo Id enviado é ignorado.
    /// </summary>
    public class CreateEmpresaDto
    {
        public Guid Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;

        // Cadastro pelo Google: ID token do botão "Entrar com o Google" (dispensa a senha)
        public string? GoogleCredential { get; set; }
        public string? Telefone { get; set; }
        public string? Site { get; set; }
        public string? LogoUrl { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Cep { get; set; }
        public string? Setor { get; set; }
        public string? Porte { get; set; }
        public string? Descricao { get; set; }
        public string? ResponsavelNome { get; set; }
        public string? ResponsavelCargo { get; set; }
        public string? ResponsavelEmail { get; set; }
        public string? Status { get; set; }
    }
}