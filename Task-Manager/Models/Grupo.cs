
using System.ComponentModel.DataAnnotations;

namespace Task_Manager.Models;

public class Grupo
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public ICollection<Usuario> Users { get; set; } = [];
}
