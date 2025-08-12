using Microsoft.AspNetCore.Mvc;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CAPFIS.Pages.Admin.Etapas
{
    public class CrearEtapaModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El título es obligatorio")]
            public string Titulo { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe seleccionar un tipo")]
            public TipoEtapa? Tipo { get; set; }

            [Url(ErrorMessage = "Ingrese una URL válida")]
            public string? ContenidoUrl { get; set; }

            public string? ContenidoJson { get; set; }

            public string? ContenidoTexto { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a cero")]
            public int Orden { get; set; }

            public bool EstaPublicado { get; set; }

            public string? StatusMessage { get; set; }
        }

        public void OnGet()
        {
            // Inicializaciones si necesitas
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Aquí guardas la etapa, por ejemplo con EF Core
            // Ejemplo:
            // var etapa = new EtapaModulo {
            //     Titulo = Input.Titulo,
            //     Tipo = Input.Tipo!.Value,
            //     ContenidoUrl = Input.ContenidoUrl,
            //     ContenidoJson = Input.ContenidoJson,
            //     ContenidoTexto = Input.ContenidoTexto,
            //     Orden = Input.Orden,
            //     EstaPublicado = Input.EstaPublicado,
            //     ModuloInteractivoId = algún_id, // debe venir o seleccionarse también
            // };

            // _context.Etapas.Add(etapa);
            // await _context.SaveChangesAsync();

            StatusMessage = "Etapa creada correctamente.";

            // Limpiar formulario (opcional)
            ModelState.Clear();
            Input = new InputModel();

            return Page();
        }
    }
}