using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAPFIS.Pages
{
    public class ModuloDetalleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ModuloDetalleModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<IActionResult> OnPostInscribirseAsync(int moduloId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Redirige a login si no hay sesión
            }

            bool yaInscripto = await _context.ModulosUsuarios
                .AnyAsync(mu => mu.UserId == user.Id && mu.ModuloId == moduloId);

            if (!yaInscripto)
            {
                var inscripcion = new ModuloUsuario
                {
                    UserId = user.Id,
                    ModuloId = moduloId,
                    Progreso = 0,
                    FechaInscripcion = DateTime.UtcNow
                };

                _context.ModulosUsuarios.Add(inscripcion);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { slug = Slug });
        }
    }
}