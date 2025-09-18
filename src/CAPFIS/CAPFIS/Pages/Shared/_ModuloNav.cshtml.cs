using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAPFIS.Pages.Shared
{
    public class ModuloNavModel : PageModel
    {
        public string NombreModulo { get; set; } = "Módulo";

        public void OnGet(string nombreModulo)
        {
            if (!string.IsNullOrEmpty(nombreModulo))
            {
                NombreModulo = nombreModulo;
            }
        }
    }
}