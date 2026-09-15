namespace OpenDoors.Api.DTOs
{
    /// <summary>
    /// DTO de entrada para criar uma nova Vaga.
    /// NÃO inclui campos gerados pelo banco (id, criadoEm, atualizadoEm)
    /// nem contadores que começam em zero (candidaturasRecebidas).
    /// </summary>
    public class CreateVagaDto
    {
        public Guid EmpresaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Area { get; set; }
        public string? Nivel { get; set; }
        public List<string>? CursosAceitos { get; set; }
        public int? SemestreMinimo { get; set; }
        public List<string>? HabilidadesRequeridas { get; set; }
        public List<string>? HabilidadesDiferenciais { get; set; }
        public string? CargaHoraria { get; set; }
        public string? Modalidade { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public decimal? Bolsa { get; set; }
        public List<string>? Beneficios { get; set; }
        public int? VagasDisponiveis { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiraEm { get; set; }
    }
}