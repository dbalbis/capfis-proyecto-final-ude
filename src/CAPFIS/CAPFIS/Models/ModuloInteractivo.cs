namespace CAPFIS.Models
{
    public class ModuloInteractivo
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Slug { get; set; } // para URLs: "phishing", "vishing"
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public bool EstaPublicado { get; set; }

        public ICollection<EtapaModulo> Etapas { get; set; }
    }
}
