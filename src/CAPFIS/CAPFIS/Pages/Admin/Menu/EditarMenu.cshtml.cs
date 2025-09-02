using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CAPFIS.Pages.Admin.Menu
{
    public class EditarMenuModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditarMenuModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ModuloInteractivo> Modulos { get; set; } = new List<ModuloInteractivo>();

        [BindProperty]
        public string OrdenJson { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            Modulos = await _context.Modulos.OrderBy(m => m.Orden ?? int.MaxValue).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(OrdenJson))
            {
                // Parseamos el JSON enviado por JS
                var ordenes = JsonConvert.DeserializeObject<List<OrdenItem>>(OrdenJson);

                foreach (var item in ordenes)
                {
                    var modulo = await _context.Modulos.FindAsync(int.Parse(item.id));
                    if (modulo != null)
                    {
                        modulo.Orden = item.orden;
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "✅ Menú actualizado correctamente.";
            }

            // Recargar la lista con los nuevos valores
            Modulos = await _context.Modulos.OrderBy(m => m.Orden ?? int.MaxValue).ToListAsync();
            return Page();
        }

        public class OrdenItem
        {
            public string id { get; set; }
            public int orden { get; set; }
        }
    }
}
