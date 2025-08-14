using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAPFIS.Pages.Admin
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirige a la primera opci�n por defecto
            return RedirectToPage("/Admin/Modulos/CrearModulo");
        }
    }
}