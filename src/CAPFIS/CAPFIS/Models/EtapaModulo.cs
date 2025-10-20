namespace CAPFIS.Models
{
    public class EtapaModulo
    {
        public int Id { get; set; }

        public int ModuloInteractivoId { get; set; }
        public ModuloInteractivo Modulo { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public TipoEtapa Tipo { get; set; }

        public string? ContenidoUrl { get; set; }

        public string? ContenidoJson { get; set; }
        public string? ContenidoTexto { get; set; }

        public int Orden { get; set; }

        public bool EstaPublicado { get; set; }
    }

    public enum TipoEtapa
    {
        Video,
        Ahorcado,
        Quiz,
        SopaDeLetras,
        EncuentraLaPalabra
    }
}