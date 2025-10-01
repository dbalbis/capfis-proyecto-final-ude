// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CAPFIS.Models;

namespace CAPFIS.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty(Name = "g-recaptcha-response")]
        public string RecaptchaToken { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Limpiar la cookie externa existente para asegurar un inicio de sesión limpio
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(RecaptchaToken) || !await ValidateReCaptchaAsync(RecaptchaToken))
            {
                ModelState.AddModelError(string.Empty, "No se pudo verificar el captcha.");
                return Page();
            }

            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario ha iniciado sesión.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("La cuenta del usuario ha sido bloqueada.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
                    return Page();
                }
            }

            // Si llegamos hasta aquí, algo falló, volver a mostrar el formulario
            return Page();
        }

        private async Task<bool> ValidateReCaptchaAsync(string token)
        {
            var secret = _configuration["GoogleReCaptcha:SecretKey"];
            using var client = new HttpClient();
            var response = await client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}", null);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(json);

            bool success = result.RootElement.GetProperty("success").GetBoolean();
            double score = result.RootElement.TryGetProperty("score", out var scoreProp) ? scoreProp.GetDouble() : 0.0;

            return success && score > 0.5;
        }
    }
}
