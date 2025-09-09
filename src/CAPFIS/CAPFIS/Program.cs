using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Configuración de la cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";         // Si no está autenticado, redirige al Index
    options.AccessDeniedPath = "/";  // Si no tiene permisos, redirige al Index
});

// Autorización: policy para administradores
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmins", policy =>
        policy.RequireRole("Administrador"));
});

// Autorizar todas las páginas dentro de /Admin
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "SoloAdmins");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseAuthentication(); // Debe ir antes de UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Crear roles y usuario admin
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await CrearRoles(roleManager);

    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await CrearUsuarioAdministrador(userManager, services);
}

app.Run();

async Task CrearUsuarioAdministrador(UserManager<ApplicationUser> userManager, IServiceProvider services)
{
    string adminEmail = "admin@capfis.com";
    string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var nuevoAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "CAPFIS",
            Country = "Uruguay",
            Gender = "Otro",
            BirthDate = new DateTime(1990, 1, 1),
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(nuevoAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(nuevoAdmin, "Administrador");
        }
        else
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            foreach (var error in result.Errors)
            {
                logger.LogError($"Error al crear usuario admin: {error.Description}");
            }
        }
    }
}

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
