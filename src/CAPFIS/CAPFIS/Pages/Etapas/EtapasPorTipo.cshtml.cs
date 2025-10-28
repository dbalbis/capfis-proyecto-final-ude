using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAPFIS.Pages.Etapas
{
    public class EtapasPorTipoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EtapasPorTipoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Tipo { get; set; } = string.Empty;

        public List<EtapaConModulo> Etapas { get; set; } = new();

        public string TipoFormateado { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return NotFound();

            // Validar que el tipo sea válido
            var tiposValidos = new[] { "Video", "Ahorcado", "Quiz", "SopaDeLetras", "EncuentraLaPalabra" };
            if (!tiposValidos.Contains(tipo, StringComparer.OrdinalIgnoreCase))
                return NotFound();

            Tipo = tipo;
            TipoFormateado = FormatearTipo(tipo);

            // Obtener todas las etapas segun su tipo y mostrando el modulo al que pertenecen
            Etapas = await _context.Etapas
                .Where(e => e.Tipo.ToString().ToLower() == tipo.ToLower())
                .Include(e => e.Modulo)
                .Where(e => e.Modulo != null && e.Modulo.EstaPublicado)
                .OrderBy(e => e.Modulo.Titulo)
                .ThenBy(e => e.Orden)
                .Select(e => new EtapaConModulo
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Orden = e.Orden,
                    ModuloTitulo = e.Modulo.Titulo,
                    ModuloSlug = e.Modulo.Slug,
                    ModuloId = e.Modulo.Id
                })
                .ToListAsync();

            return Page();
        }

        private string FormatearTipo(string tipo)
        {
            return tipo switch
            {
                "Video" => "Videos",
                "Ahorcado" => "Ahorcado",
                "Quiz" => "Quiz",
                "SopaDeLetras" => "Sopa de Letras",
                "EncuentraLaPalabra" => "Encuentra la Palabra",
                _ => tipo
            };
        }
    }

    // Clase auxiliar
    public class EtapaConModulo
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        public string ModuloTitulo { get; set; } = string.Empty;
        public string ModuloSlug { get; set; } = string.Empty;
        public int ModuloId { get; set; }
    }
}