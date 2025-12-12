using System.ComponentModel.DataAnnotations;

public class Log
{
    [Key]
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    [Required]
    [StringLength(200)]
    public string Accion { get; set; } = string.Empty;

    [Required]
    public DateTime Fecha { get; set; } = DateTime.Now;

    [StringLength(40)]
    public string? Ip { get; set; }

    [StringLength(4000)]
    public string? Datos { get; set; }
}
