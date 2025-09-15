using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAPFIS.Pages.Modulo
{
    public class AprenderModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AprenderModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string Slug { get; set; } = string.Empty;

        public ModuloInteractivo? Modulo { get; set; }
        public EtapaModulo? EtapaActual { get; set; }
        public ModuloUsuario? ModuloUsuario { get; set; }

        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            Modulo = await _context.Modulos
                .Include(m => m.Etapas)
                .FirstOrDefaultAsync(m => m.Slug.ToLower() == slug.ToLower() && m.EstaPublicado);

            if (Modulo == null) return NotFound();

            ModuloUsuario = await _context.ModulosUsuarios
                .FirstOrDefaultAsync(mu => mu.UserId == user.Id && mu.ModuloId == Modulo.Id);

            if (ModuloUsuario == null)
            {
                ModuloUsuario = new ModuloUsuario
                {
                    UserId = user.Id,
                    ModuloId = Modulo.Id,
                    Progreso = 0,
                    Completado = false,
                    EtapaActualOrden = 1,
                    FechaInscripcion = DateTime.UtcNow
                };
                _context.ModulosUsuarios.Add(ModuloUsuario);
                await _context.SaveChangesAsync();
            }

            EtapaActual = Modulo.Etapas
                .Where(e => e.EstaPublicado)
                .OrderBy(e => e.Orden)
                .FirstOrDefault(e => e.Orden == ModuloUsuario.EtapaActualOrden);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int moduloId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var moduloUsuario = await _context.ModulosUsuarios
                .FirstOrDefaultAsync(mu => mu.UserId == user.Id && mu.ModuloId == moduloId);

            if (moduloUsuario == null) return NotFound();

            var modulo = await _context.Modulos
                .Include(m => m.Etapas)
                .FirstOrDefaultAsync(m => m.Id == moduloId);

            if (modulo == null) return NotFound();

            var etapasPublicadas = modulo.Etapas.OrderBy(e => e.Orden).ToList();
            if (moduloUsuario.EtapaActualOrden < etapasPublicadas.Count)
            {
                moduloUsuario.EtapaActualOrden++;
                moduloUsuario.Progreso = (int)((double)moduloUsuario.EtapaActualOrden / etapasPublicadas.Count * 100);
            }
            else
            {
                moduloUsuario.Completado = true;
                moduloUsuario.Progreso = 100;
            }

            _context.ModulosUsuarios.Update(moduloUsuario);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { slug = modulo.Slug });
        }
    }
}