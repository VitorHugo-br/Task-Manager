using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager.Models;

public class Comentario
{
    [Key]
    [Column("comentario_id")]
    public int ComentarioId { get; set; }

    [Column("chamado_id")]
    public int ChamadoId { get; set; }
    public Chamado? Chamado { get; set; }

    [Column("remetente_id")]
    public int RemetenteId { get; set; }
    public Usuario? Remetente { get; set; }

    [Column("conteudo")]
    public string Conteudo { get; set; } = string.Empty;

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    
    [Column("deletado")]
    public bool Deletado { get; set; } = false;
}
