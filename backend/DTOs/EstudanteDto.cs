namespace OpenDoors.Api.DTOs
{
    public class EstudanteDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? Cpf { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public string? Instituicao { get; set; }
        public string? Curso { get; set; }
        public int? Semestre { get; set; }
        public string? Turno { get; set; }
        public string? PrevisaoConclusao { get; set; }
        public string? CurriculoUrl { get; set; }
        public List<string>? HabilidadesExtraidas { get; set; }
        public bool? TemCurriculo { get; set; }
        public bool? TemTesteVocacional { get; set; }
        public string? Status { get; set; }
        public DateTime? CriadoEm { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }
}