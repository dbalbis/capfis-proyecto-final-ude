using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAPFIS.Pages.Aprendizaje
{
    public class MiAprendizajeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MiAprendizajeModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ModuloUsuario> ModulosSubscripto { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            // Traemos los módulos con el progreso del usuario
            ModulosSubscripto = await _context.ModulosUsuarios
                .Where(mu => mu.UserId == user.Id)
                .Include(mu => mu.Modulo)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDarseDeBajaAsync(int moduloId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var suscripcion = await _context.ModulosUsuarios
                .FirstOrDefaultAsync(mu => mu.UserId == user.Id && mu.ModuloId == moduloId);

            if (suscripcion != null)
            {
                _context.ModulosUsuarios.Remove(suscripcion);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
