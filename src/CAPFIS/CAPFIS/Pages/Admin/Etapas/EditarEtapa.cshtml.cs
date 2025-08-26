using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CAPFIS.Pages.Admin.Etapas
{
    public class EditarEtapaModuloModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditarEtapaModuloModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedEtapaId { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList EtapasList { get; set; } = default!;
        public SelectList ModulosList { get; set; } = default!;
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
            // cargar listas
            EtapasList = new SelectList(_context.Etapas.OrderBy(e => e.Titulo).ToList(), "Id", "Titulo");
            ModulosList = new SelectList(_context.Modulos.OrderBy(m => m.Titulo).ToList(), "Id", "Titulo");

            if (SelectedEtapaId.HasValue)
            {
                var etapa = _context.Etapas.Find(SelectedEtapaId.Value);
                if (etapa != null)
                {
                    Input = new InputModel
                    {
                        ModuloInteractivoId = etapa.ModuloInteractivoId,
                        Titulo = etapa.Titulo,
                        Tipo = etapa.Tipo,
                        ContenidoUrl = etapa.ContenidoUrl,
                        ContenidoTexto = etapa.ContenidoTexto,
                        ContenidoJson = etapa.ContenidoJson,
                        Orden = etapa.Orden,
                        EstaPublicado = etapa.EstaPublicado
                    };
                }
            }
        }

        public IActionResult OnPost()
        {
            EtapasList = new SelectList(_context.Etapas.OrderBy(e => e.Titulo).ToList(), "Id", "Titulo");
            ModulosList = new SelectList(_context.Modulos.OrderBy(m => m.Titulo).ToList(), "Id", "Titulo");

            if (!SelectedEtapaId.HasValue)
            {
                StatusMessage = "⚠️ Debe seleccionar una etapa.";
                return Page();
            }

            var etapa = _context.Etapas.Find(SelectedEtapaId.Value);
            if (etapa == null)
            {
                StatusMessage = "❌ La etapa no existe.";
                return Page();
            }

            if (!ModelState.IsValid) return Page();

            // actualizar
            etapa.ModuloInteractivoId = Input.ModuloInteractivoId;
            etapa.Titulo = Input.Titulo;
            etapa.Tipo = Input.Tipo!.Value;
            etapa.Orden = Input.Orden;
            etapa.EstaPublicado = Input.EstaPublicado;

            switch (Input.Tipo)
            {
                case TipoEtapa.Video:
                    etapa.ContenidoUrl = Input.ContenidoUrl;
                    etapa.ContenidoTexto = null;
                    etapa.ContenidoJson = null;
                    break;

                case TipoEtapa.Ahorcado:
                    etapa.ContenidoTexto = Input.ContenidoTexto;
                    etapa.ContenidoJson = null;
                    etapa.ContenidoUrl = null;
                    break;

                case TipoEtapa.Quiz:
                case TipoEtapa.SopaDeLetras:
                    etapa.ContenidoJson = Input.ContenidoJson;
                    etapa.ContenidoTexto = null;
                    etapa.ContenidoUrl = null;
                    break;

                case TipoEtapa.EncuentraLaPalabra:
                    etapa.ContenidoTexto = Input.ContenidoTexto;
                    etapa.ContenidoJson = Input.ContenidoJson;
                    etapa.ContenidoUrl = null;
                    break;
            }

            _context.Etapas.Update(etapa);
            _context.SaveChanges();

            StatusMessage = "✅ Etapa actualizada correctamente.";
            return Page();
        }
    }
}
