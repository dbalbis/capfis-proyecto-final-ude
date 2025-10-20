using CAPFIS.Data;
using CAPFIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAPFIS.Pages.Admin.Usuarios
{
    public class EditarUsuarioModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditarUsuarioModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class UsuarioViewModel
        {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
        }

        public List<UsuarioViewModel> Usuarios { get; set; } = new();
        public string? StatusMessage { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            CurrentUserId = _userManager.GetUserId(User);

            var users = _userManager.Users.ToList();
            Usuarios = new List<UsuarioViewModel>();

            foreach (var user in users)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Administrador");
                Usuarios.Add(new UsuarioViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsAdmin = isAdmin
                });
            }
        }

        public async Task<IActionResult> OnPostToggleAdminAsync(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                StatusMessage = "Usuario no encontrado.";
                await OnGetAsync();
                return Page();
            }

            if (UserId == _userManager.GetUserId(User))
            {
                StatusMessage = "No puedes quitarte admin a ti mismo.";
                await OnGetAsync();
                return Page();
            }

            if (!await _roleManager.RoleExistsAsync("Administrador"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Administrador"));
            }

            if (await _userManager.IsInRoleAsync(user, "Administrador"))
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, "Administrador");
                if (removeResult.Succeeded)
                {
                    StatusMessage = $"Admin removido de {user.UserName}.";
                }
                else
                {
                    StatusMessage = $"Error al quitar admin de {user.UserName}.";
                }
            }
            else
            {
                var addResult = await _userManager.AddToRoleAsync(user, "Administrador");
                if (addResult.Succeeded)
                {
                    StatusMessage = $"{user.UserName} ahora es admin.";
                }
                else
                {
                    StatusMessage = $"Error al dar admin a {user.UserName}.";
                }
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                StatusMessage = "Usuario no encontrado.";
                await OnGetAsync();
                return Page();
            }

            // Evitar borrar al propio usuario
            if (UserId == _userManager.GetUserId(User))
            {
                StatusMessage = "No puedes borrarte a ti mismo.";
                await OnGetAsync();
                return Page();
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (deleteResult.Succeeded)
            {
                StatusMessage = $"Usuario {user.UserName} eliminado correctamente.";
            }
            else
            {
                StatusMessage = "Error al eliminar usuario.";
            }

            await OnGetAsync();
            return Page();
        }
    }
}