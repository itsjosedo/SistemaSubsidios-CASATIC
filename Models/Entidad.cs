using System.ComponentModel.DataAnnotations;

public class Entidad
{
    [Key]
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public ICollection<Beneficiario>? Beneficiarios { get; set; }
}