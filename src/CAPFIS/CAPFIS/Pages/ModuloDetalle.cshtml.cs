using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAPFIS.Pages
{
    public class ModuloDetalleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ModuloDetalleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Slug { get; set; } = string.Empty;

        public ModuloInteractivo? Modulo { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            Modulo = await _context.Modulos
                .FirstOrDefaultAsync(m => m.Slug.ToLower() == slug.ToLower() && m.EstaPublicado);

            if (Modulo == null)
                return NotFound();

            return Page();
        }
    }
}
