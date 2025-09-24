using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
public class Beneficiario
{
    [Key]
    public int Id_Beneficiario { get; set; }
    public string? Nombre { get; set; }
    public string? Dui { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string EstadoSubsidio { get; set; } = "pendiente";

    public int EntidadId { get; set; }
    public Entidad? Entidad { get; set; }

    public ICollection<Subsidio> Subsidios { get; set; }
}