using CAPFIS.Data;
using CAPFIS.Models;
using CAPFIS.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.RegularExpressions;

namespace CAPFIS.Pages.Admin.Modulos
{
    public class EditarModuloModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditarModuloModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        public List<ModuloInteractivo> Modulos { get; set; } = new();

        [BindProperty]
        public ModuloInteractivo SelectedModulo { get; set; } = new();

        [BindProperty]
        public IFormFile? HeroImageFile { get; set; }

        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();

            if (SelectedId.HasValue)
            {
                var modulo = _context.Modulos.Find(SelectedId.Value);
                if (modulo != null)
                {
                    SelectedModulo = modulo;
                }
            }
            else
            {
                SelectedModulo = null;
            }
        }

        public IActionResult OnPost()
        {
            var modulo = _context.Modulos.Find(SelectedModulo.Id);
            if (modulo == null) return Page();

            // Validaciones
            if (string.IsNullOrWhiteSpace(SelectedModulo.Titulo))
                ModelState.AddModelError("SelectedModulo.Titulo", "El título es obligatorio.");

            if (string.IsNullOrWhiteSpace(SelectedModulo.TituloDetallado))
                ModelState.AddModelError("SelectedModulo.TituloDetallado", "El título detallado es obligatorio.");

            if (string.IsNullOrWhiteSpace(SelectedModulo.Descripcion))
                ModelState.AddModelError("SelectedModulo.Descripcion", "La descripción es obligatoria.");

            if (string.IsNullOrWhiteSpace(modulo.ImagenHero) && (HeroImageFile == null || HeroImageFile.Length == 0))
                ModelState.AddModelError("HeroImageFile", "Debe subir una imagen.");

            if (!ModelState.IsValid) return Page();

            // Sanitizar inputs
            modulo.Titulo = InputSanitizer.SanitizeText(SelectedModulo.Titulo);
            modulo.TituloDetallado = InputSanitizer.SanitizeText(SelectedModulo.TituloDetallado);
            modulo.Descripcion = InputSanitizer.SanitizeHtml(SelectedModulo.Descripcion);
            modulo.EstaPublicado = SelectedModulo.EstaPublicado;

            // Validar que el slug sea unico
            string newSlug = GenerarSlug(modulo.Titulo);
            if (_context.Modulos.Any(m => m.Id != modulo.Id && m.Slug == newSlug))
            {
                ModelState.AddModelError("SelectedModulo.Titulo", "Ya existe un módulo con un título similar.");
                return Page();
            }
            modulo.Slug = newSlug;

            // Subir imagen nueva
            if (HeroImageFile != null && HeroImageFile.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                if (HeroImageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("HeroImageFile", "El archivo no debe superar los 5 MB.");
                    return Page();
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/modulos");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(HeroImageFile.FileName).ToLowerInvariant();
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!permittedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("HeroImageFile", "Formato de imagen no permitido.");
                    return Page();
                }

                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                HeroImageFile.CopyTo(stream);

                modulo.ImagenHero = $"/uploads/modulos/{uniqueFileName}";
            }

            _context.SaveChanges();

            StatusMessage = "Módulo actualizado correctamente.";

            // Recargar lista y módulo
            Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();
            SelectedModulo = modulo;

            return Page();
        }

        private string GenerarSlug(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            string normalizedString = texto.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            string slug = stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
            slug = slug.ToLowerInvariant();
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}