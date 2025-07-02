using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAPFIS.Pages
{
    public class ModulosModel : PageModel
    {
        public int Porcentaje { get; set; }

        public void OnGet()
        {
            Porcentaje = 40;
        }
    }
}