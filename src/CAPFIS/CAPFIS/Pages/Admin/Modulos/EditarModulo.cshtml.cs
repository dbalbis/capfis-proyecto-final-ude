using CAPFIS.Data;
using CAPFIS.Models;
using CAPFIS.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
            if (!ModelState.IsValid) return Page();

            var modulo = _context.Modulos.Find(SelectedModulo.Id);
            if (modulo == null) return Page();

            // Actualizar campos
            modulo.Titulo = SelectedModulo.Titulo;
            modulo.Descripcion = InputSanitizer.SanitizeHtml(SelectedModulo.Descripcion);
            modulo.EstaPublicado = SelectedModulo.EstaPublicado;

            // Subir imagen nueva si se seleccionó
            if (HeroImageFile != null && HeroImageFile.Length > 0)
            {
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

            StatusMessage = "✅ Módulo actualizado correctamente.";

            // Recargar lista y módulo
            Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();
            SelectedModulo = modulo;

            return Page();
        }
    }
}