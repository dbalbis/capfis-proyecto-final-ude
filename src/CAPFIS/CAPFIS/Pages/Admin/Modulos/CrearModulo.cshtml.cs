using CAPFIS.Data;
using CAPFIS.Models;
using CAPFIS.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

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
            [Required(ErrorMessage = "El título es obligatorio")]
            public string Titulo { get; set; } = string.Empty;

            [Required(ErrorMessage = "El título detallado es obligatorio")]
            public string TituloDetallado { get; set; } = string.Empty;

            [Required(ErrorMessage = "La descripción es obligatoria")]
            public string Descripcion { get; set; } = string.Empty;

            public bool Publicado { get; set; } = true;

            [Required(ErrorMessage = "La imagen Hero es obligatoria")]
            public IFormFile? HeroImageFile { get; set; }
        }

        public void OnGet()
        {
            // Solo muestra el formulario vacío
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Sanitizar inputs
            Input.Titulo = InputSanitizer.SanitizeText(Input.Titulo);

            // Generar slug siempre a partir del título
            string slug = GenerarSlug(Input.Titulo);

            // Validar slug único
            if (_context.Modulos.Any(m => m.Slug == slug))
            {
                ModelState.AddModelError("Input.Titulo", "Ya existe un módulo con un título similar.");
                return Page();
            }

            // Validación de imagen obligatoria
            if (Input.HeroImageFile == null || Input.HeroImageFile.Length == 0)
            {
                ModelState.AddModelError("Input.HeroImageFile", "Debe seleccionar una imagen Hero.");
                return Page();
            }

            string? heroImagePath = null;

            if (Input.HeroImageFile != null && Input.HeroImageFile.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                if (Input.HeroImageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("Input.HeroImageFile", "El archivo no debe superar los 5 MB.");
                    return Page();
                }

                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(Input.HeroImageFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Input.HeroImageFile", "Solo se permiten imágenes en formato JPG, PNG, GIF o WebP.");
                    return Page();
                }

                if (!Input.HeroImageFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("Input.HeroImageFile", "El archivo no es una imagen válida.");
                    return Page();
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/modulos");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Input.HeroImageFile.CopyTo(fileStream);
                }

                heroImagePath = $"/uploads/modulos/{uniqueFileName}";
            }

            var modulo = new ModuloInteractivo
            {
                Titulo = Input.Titulo,
                TituloDetallado = Input.TituloDetallado,
                Slug = slug,
                Descripcion = InputSanitizer.SanitizeHtml(Input.Descripcion),
                EstaPublicado = Input.Publicado,
                ImagenHero = heroImagePath
            };

            try
            {
                _context.Modulos.Add(modulo);
                _context.SaveChanges();

                StatusMessage = "✅ Módulo creado correctamente.";

                // Limpiar formulario
                ModelState.Clear();
                Input = new InputModel();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error al guardar el módulo: {ex.Message}";
            }

            return Page();
        }

        private string GenerarSlug(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            string normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            string slug = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            slug = slug.ToLowerInvariant();
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}
