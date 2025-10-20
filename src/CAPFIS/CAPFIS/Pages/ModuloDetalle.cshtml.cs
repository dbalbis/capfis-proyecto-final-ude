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

        public bool EstaInscripto { get; set; } = false;

        public ModuloUsuario? ModuloUsuario { get; set; }

        public ModuloInteractivo? Modulo { get; set; }

        public bool ModuloCompletado => ModuloUsuario != null && ModuloUsuario.Completado && ModuloUsuario.Progreso >= 100;

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            Modulo = await _context.Modulos
                .FirstOrDefaultAsync(m => m.Slug.ToLower() == slug.ToLower() && m.EstaPublicado);

            if (Modulo == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ModuloUsuario = await _context.ModulosUsuarios
                    .FirstOrDefaultAsync(mu => mu.UserId == user.Id && mu.ModuloId == Modulo.Id);

                EstaInscripto = ModuloUsuario != null;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostInscribirseAsync(int moduloId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Redirigimos a login si inicio sesion
            }

            var moduloUsuario = await _context.ModulosUsuarios
                .FirstOrDefaultAsync(mu => mu.UserId == user.Id && mu.ModuloId == moduloId);

            if (moduloUsuario == null)
            {
                moduloUsuario = new ModuloUsuario
                {
                    UserId = user.Id,
                    ModuloId = moduloId,
                    Progreso = 0,
                    Completado = false,
                    FechaInscripcion = DateTime.UtcNow
                };

                _context.ModulosUsuarios.Add(moduloUsuario);
                await _context.SaveChangesAsync();
            }

            return Redirect($"/Modulo/Aprender/{Slug}");
        }
    }
}