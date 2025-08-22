using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAPFIS.Pages.Admin.Modulos
{
    public class EliminarModuloModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EliminarModuloModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        public List<ModuloInteractivo> Modulos { get; set; } = new();

        [BindProperty]
        public ModuloInteractivo SelectedModulo { get; set; } = new();

        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();

            if (SelectedId.HasValue)
            {
                SelectedModulo = _context.Modulos.Find(SelectedId.Value);
            }
            else
            {
                SelectedModulo = null;
            }
        }

        public IActionResult OnPost()
        {
            if (SelectedModulo == null || SelectedModulo.Id == 0)
            {
                StatusMessage = "❌ No se pudo eliminar el módulo.";
                return Page();
            }

            var modulo = _context.Modulos.Find(SelectedModulo.Id);
            if (modulo == null)
            {
                StatusMessage = "❌ Módulo no encontrado.";
                return Page();
            }

            _context.Modulos.Remove(modulo);
            _context.SaveChanges();

            StatusMessage = "✅ Módulo eliminado correctamente.";

            // Recargar lista y limpiar selección
            Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();
            SelectedModulo = null;
            SelectedId = null;

            return Page();
        }
    }
}
