using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CAPFIS.Pages.Admin.Modulos
{
    public class OrdenarModuloModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrdenarModuloModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ModuloInteractivo> Modulos { get; set; } = new List<ModuloInteractivo>();
        public IList<EtapaModulo> Etapas { get; set; } = new List<EtapaModulo>();
        public ModuloInteractivo? SelectedModulo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        [BindProperty]
        public string OrdenJson { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            Modulos = await _context.Modulos.OrderBy(m => m.Orden).ToListAsync();

            if (SelectedId.HasValue)
            {
                SelectedModulo = await _context.Modulos
                    .Include(m => m.Etapas)
                    .FirstOrDefaultAsync(m => m.Id == SelectedId.Value);

                if (SelectedModulo != null)
                {
                    Etapas = SelectedModulo.Etapas.OrderBy(e => e.Orden).ToList();
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Modulos = await _context.Modulos.OrderBy(m => m.Orden).ToListAsync();

            if (!string.IsNullOrEmpty(OrdenJson))
            {
                var ordenes = JsonConvert.DeserializeObject<List<OrdenItem>>(OrdenJson);

                foreach (var item in ordenes)
                {
                    var etapa = await _context.Etapas.FindAsync(int.Parse(item.id));
                    if (etapa != null)
                    {
                        etapa.Orden = item.orden;
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Orden de etapas actualizado correctamente.";
            }

            if (SelectedId.HasValue)
            {
                SelectedModulo = await _context.Modulos
                    .Include(m => m.Etapas)
                    .FirstOrDefaultAsync(m => m.Id == SelectedId.Value);

                if (SelectedModulo != null)
                {
                    Etapas = SelectedModulo.Etapas.OrderBy(e => e.Orden).ToList();
                }
            }

            return Page();
        }

        public class OrdenItem
        {
            public string id { get; set; }
            public int orden { get; set; }
        }
    }
}