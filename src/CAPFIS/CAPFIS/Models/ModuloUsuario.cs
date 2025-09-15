using CAPFIS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ModuloUsuario
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Usuario")]
    public string UserId { get; set; } = null!; // FK a ApplicationUser

    public int ModuloId { get; set; } // FK a ModuloInteractivo

    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

    public int Progreso { get; set; } = 0;

    public int EtapaActualOrden { get; set; } = 1;

    public bool Completado { get; set; } = false;

    // Relaciones
    public ApplicationUser? Usuario { get; set; }

    public ModuloInteractivo? Modulo { get; set; }
}