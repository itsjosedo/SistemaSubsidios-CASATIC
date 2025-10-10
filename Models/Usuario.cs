using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class Usuario
{
    [Key]
    public int Id_Usuario { get; set; }
    public string? Nombre { get; set; }
    public string? Correo { get; set; }
    public string? Contrasena { get; set; }
    public string? Rol { get; set; } // administrador, operador, beneficiario
    public string Estado { get; set; } = "activo";

    public Entidad? Entidad { get; set; } // relación opcional
    public ICollection<Notificacion>? Notificaciones { get; set; }

    public virtual Beneficiario? Beneficiario { get; set; }
    //public ICollection<Beneficiario>? Beneficiarios { get; set; }
} 