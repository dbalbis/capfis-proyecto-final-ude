namespace CAPFIS.Models
{
    public class EtapaModulo
    {
        public int Id { get; set; }

        public int ModuloInteractivoId { get; set; }
        public ModuloInteractivo Modulo { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public TipoEtapa Tipo { get; set; }

        // Guardar contenido según tipo
        public string? ContenidoUrl { get; set; }    // Para videos

        public string? ContenidoJson { get; set; }   // Para quizzes, sopas, etc.
        public string? ContenidoTexto { get; set; }  // Texto plano

        public int Orden { get; set; } // posición dentro del módulo

        public bool EstaPublicado { get; set; } // Publicar/despublicar etapas individualmente
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