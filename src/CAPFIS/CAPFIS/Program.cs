using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Conexion SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configuración de la cookie para proteger urls
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";         // Si no está autenticado, redirige al Index
    options.AccessDeniedPath = "/";  // Si no tiene permisos, redirige al Index
});

// Autorización solo para administradores
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmins", policy =>
        policy.RequireRole("Administrador"));
});

// Proteger todas las páginas dentro de /Admin y /Aprendizaje
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "SoloAdmins");

    options.Conventions.AuthorizeFolder("/Aprendizaje");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Crear roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await CrearRoles(roleManager);

    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
}

app.Run();

async Task CrearRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roles = new[] { "Administrador", "Estudiante" };

    foreach (var rol in roles)
    {
        bool existeRol = await roleManager.RoleExistsAsync(rol);
        if (!existeRol)
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }
}