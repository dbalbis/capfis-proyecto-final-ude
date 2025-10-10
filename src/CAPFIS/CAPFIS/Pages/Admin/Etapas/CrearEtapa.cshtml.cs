using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CAPFIS.Pages.Admin.Etapas
{
    public class CrearEtapaModuloModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CrearEtapaModuloModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<ModuloInteractivo> Modulos { get; set; } = new();

        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Debe seleccionar un módulo")]
            public int ModuloInteractivoId { get; set; }

            [Required(ErrorMessage = "El título es obligatorio")]
            public string Titulo { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe seleccionar un tipo")]
            public TipoEtapa? Tipo { get; set; }

            [Url(ErrorMessage = "Ingrese una URL válida")]
            public string? ContenidoUrl { get; set; }

            public string? ContenidoTexto { get; set; }

            public string? ContenidoJson { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a cero")]
            public int Orden { get; set; } = 1;

            public bool EstaPublicado { get; set; }
        }

        public void OnGet()
        {
            Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();
                return Page();
            }

            // Validar JSON obligatorio para ciertos tipos
            if ((Input.Tipo == TipoEtapa.Quiz || Input.Tipo == TipoEtapa.SopaDeLetras || Input.Tipo == TipoEtapa.EncuentraLaPalabra)
                && string.IsNullOrWhiteSpace(Input.ContenidoJson))
            {
                ModelState.AddModelError(string.Empty, "El contenido JSON no puede estar vacío.");
                Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();
                return Page();
            }

            // 🔹 Calcular el orden automáticamente
            var ultimoOrden = _context.Etapas
                .Where(e => e.ModuloInteractivoId == Input.ModuloInteractivoId)
                .Max(e => (int?)e.Orden) ?? 0;

            int nuevoOrden = ultimoOrden + 1;

            var etapa = new EtapaModulo
            {
                ModuloInteractivoId = Input.ModuloInteractivoId,
                Titulo = Input.Titulo,
                Tipo = Input.Tipo!.Value,
                Orden = nuevoOrden, // 👈 Se asigna el nuevo orden
                EstaPublicado = true,
            };

            switch (Input.Tipo)
            {
                case TipoEtapa.Video:
                    etapa.ContenidoUrl = Input.ContenidoUrl;
                    etapa.ContenidoTexto = Input.ContenidoTexto;
                    break;

                case TipoEtapa.Ahorcado:
                    etapa.ContenidoTexto = Input.ContenidoTexto;
                    break;

                case TipoEtapa.Quiz:
                case TipoEtapa.SopaDeLetras:
                    etapa.ContenidoJson = Input.ContenidoJson;
                    break;

                case TipoEtapa.EncuentraLaPalabra:
                    etapa.ContenidoTexto = Input.ContenidoTexto;
                    etapa.ContenidoJson = Input.ContenidoJson;
                    break;
            }

            _context.Etapas.Add(etapa);
            _context.SaveChanges();

            StatusMessage = "✅ Etapa creada correctamente.";

            ModelState.Clear();
            Input = new InputModel();
            Modulos = _context.Modulos.OrderBy(m => m.Titulo).ToList();

            return Page();
        }
    }
}