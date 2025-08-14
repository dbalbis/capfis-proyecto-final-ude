using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Habilitar lockout para nuevos usuarios
    options.Lockout.AllowedForNewUsers = true;

    // Cantidad máxima de intentos fallidos antes de bloquear
    options.Lockout.MaxFailedAccessAttempts = 3;

    // Tiempo de bloqueo tras llegar al límite (ejemplo: 5 minutos)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication(); // <-- ¡IMPORTANTE! Debe estar antes de UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

var scope = app.Services.CreateScope();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
await CrearRoles(roleManager);

var services = scope.ServiceProvider;
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

await CrearUsuarioAdministrador(userManager, services);

app.Run();

async Task CrearUsuarioAdministrador(UserManager<ApplicationUser> userManager, IServiceProvider services)
{
    //ESTO TIENE QUE IR EN UN ARCHIVO DE CONFIGURACION Y NO PLANO
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
