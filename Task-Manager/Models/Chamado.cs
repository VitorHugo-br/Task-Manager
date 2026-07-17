using System.ComponentModel.DataAnnotations;
using Task_Manager.Models.Enums;

namespace Task_Manager.Models;

public class Chamado
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public  string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public Status Status { get; set; } = Status.Pendente;
    public DateTime? DataInicio { get; set; } = null;
    public DateTime? DataTermino { get; set; } = null;
    public DateTime? Prazo { get; set; }
    public int RemetenteId { get; set; }
    public Usuario? Remetente { get; set; }
    public int? ResponsavelId { get; set; }
    public Usuario? Responsavel { get; set; }
    public DateTime? CriadoEm { get; set; } = DateTime.Now;
    public TimeSpan HorasGastas { get; set; } = TimeSpan.Zero;

}
