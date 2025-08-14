using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CAPFIS.Pages.Admin.Modulos
{
    public class CrearModuloModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CrearModuloModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El t�tulo es obligatorio")]
            public string Titulo { get; set; } = string.Empty;

            [Required(ErrorMessage = "El slug es obligatorio")]
            public string Slug { get; set; } = string.Empty;

            [Required(ErrorMessage = "La descripci�n es obligatoria")]
            public string Descripcion { get; set; } = string.Empty;

            [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a cero")]
            public int Orden { get; set; }

            public bool Publicado { get; set; }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Generar slug autom�tico si no se ingres�
            if (string.IsNullOrWhiteSpace(Input.Slug))
            {
                Input.Slug = GenerarSlug(Input.Titulo);
            }

            var modulo = new ModuloInteractivo
            {
                Titulo = Input.Titulo,
                Slug = Input.Slug,
                Descripcion = Input.Descripcion,
                Orden = Input.Orden,
                EstaPublicado = Input.Publicado
            };

            _context.Modulos.Add(modulo);
            _context.SaveChanges();

            StatusMessage = "M�dulo creado correctamente.";

            ModelState.Clear();
            Input = new InputModel();

            return Page();
        }

        private string GenerarSlug(string texto)
        {
            return texto
                .ToLower()
                .Replace(" ", "-")
                .Replace("�", "a")
                .Replace("�", "e")
                .Replace("�", "i")
                .Replace("�", "o")
                .Replace("�", "u");
        }
    }
}
