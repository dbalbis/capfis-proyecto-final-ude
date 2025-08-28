using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAPFIS.Pages.Admin.Etapas
{
    public class EliminarEtapaModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EliminarEtapaModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        public List<EtapaModulo> Etapas { get; set; } = new();

        [BindProperty]
        public EtapaModulo SelectedEtapa { get; set; } = new();

        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            Etapas = _context.Etapas.OrderBy(e => e.Titulo).ToList();

            if (SelectedId.HasValue)
            {
                SelectedEtapa = _context.Etapas.Find(SelectedId.Value);
            }
            else
            {
                SelectedEtapa = null;
            }
        }

        public IActionResult OnPost()
        {
            if (SelectedEtapa == null || SelectedEtapa.Id == 0)
            {
                StatusMessage = "❌ No se pudo eliminar la etapa.";
                return Page();
            }

            var etapa = _context.Etapas.Find(SelectedEtapa.Id);
            if (etapa == null)
            {
                StatusMessage = "❌ Etapa no encontrada.";
                return Page();
            }

            _context.Etapas.Remove(etapa);
            _context.SaveChanges();

            StatusMessage = "✅ Etapa eliminada correctamente.";

            // Recargar lista y limpiar selección
            Etapas = _context.Etapas.OrderBy(e => e.Titulo).ToList();
            SelectedEtapa = null;
            SelectedId = null;

            return Page();
        }
    }
}