namespace OpenDoors.Api.DTOs
{
    /// <summary>
    /// DTO de entrada para criar uma Empresa.
    /// IMPORTANTE: o Id é OBRIGATÓRIO e deve ser o UUID gerado
    /// pelo Supabase Auth no momento do cadastro do usuário.
    /// </summary>
    public class CreateEmpresaDto
    {
        public Guid Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
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