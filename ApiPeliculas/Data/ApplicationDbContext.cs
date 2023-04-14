using ApiPeliculas.Modelos;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Data
{
    /* Herencia de la clase IdentityDbContext, esto nos servirá para las migraciones */
    public class ApplicationDbContext : IdentityDbContext<AppUsuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        // Agregar los modelos aqui
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Pelicula> Pelicula { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<AppUsuario> AppUsuario { get; set; }
    }
}
