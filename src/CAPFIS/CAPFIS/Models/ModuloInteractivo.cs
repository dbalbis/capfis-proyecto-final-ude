namespace CAPFIS.Models
{
    public class ModuloInteractivo
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string TituloDetallado { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int? Orden { get; set; }
        public bool EstaPublicado { get; set; }

        public string? ImagenHero { get; set; }
        public string? BotonTexto { get; set; }
        public string? BotonUrl { get; set; }

        public ICollection<EtapaModulo> Etapas { get; set; } = new List<EtapaModulo>();
    }
}