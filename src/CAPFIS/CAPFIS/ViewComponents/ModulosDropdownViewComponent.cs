using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAPFIS.ViewComponents
{
    public class ModulosDropdownViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ModulosDropdownViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var modulos = await _context.Modulos
                .Where(m => m.EstaPublicado)
                .OrderBy(m => m.Orden)
                .ToListAsync();

            return View(modulos);
        }
    }
}