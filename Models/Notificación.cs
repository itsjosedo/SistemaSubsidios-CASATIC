using System.ComponentModel.DataAnnotations;

public class Notificacion
{
    [Key]
    public int Id_Notificacion { get; set; }
    public string? Mensaje { get; set; }
    public string? Tipo { get; set; } // correo, portal
    public DateTime Fecha { get; set; } = DateTime.Now;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}