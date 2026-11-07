using System.Text.Json.Serialization;

namespace OpenDoors.Api.Models
{
    public class Vaga
    {
        public int Id { get; set; }

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

        public int? CandidaturasRecebidas { get; set; }

        public string? Status { get; set; }

        public DateTime? CriadoEm { get; set; }

        public DateTime? ExpiraEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }
    }
}