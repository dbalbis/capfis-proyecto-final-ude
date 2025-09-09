using CAPFIS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CAPFIS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ModuloInteractivo> Modulos { get; set; }
        public DbSet<EtapaModulo> Etapas { get; set; }

        public DbSet<ModuloUsuario> ModulosUsuarios { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}