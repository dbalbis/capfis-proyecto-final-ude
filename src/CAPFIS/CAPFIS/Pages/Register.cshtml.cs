// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using CAPFIS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CAPFIS.Pages
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
    }

    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [Display(Name = "Nombre")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "El apellido es obligatorio.")]
            [Display(Name = "Apellido")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "El país es obligatorio.")]
            [Display(Name = "País")]
            public string Country { get; set; }

            [Required(ErrorMessage = "El género es obligatorio.")]
            [Display(Name = "Género")]
            public string Gender { get; set; }

            [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
            [DataType(DataType.Date)]
            [Display(Name = "Fecha de nacimiento")]
            public DateTime? BirthDate { get; set; }

            [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
            [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
            [Display(Name = "Número de teléfono")]
            public string PhoneNumber { get; set; }

            [MustBeTrue(ErrorMessage = "Debe aceptar los términos y condiciones para registrarse.")]
            [Display(Name = "Acepto los términos y condiciones")]
            public bool AcceptTerms { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.Country = Input.Country;
                user.Gender = Input.Gender;
                user.BirthDate = Input.BirthDate;
                user.PhoneNumber = Input.PhoneNumber;
                user.EmailConfirmed = true;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario creó una nueva cuenta con contraseña.");

                    await _userManager.AddToRoleAsync(user, "Estudiante");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirma tu correo electrónico",
                        $"Por favor confirma tu cuenta haciendo <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clic aquí</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                // Pisamos algunos mensajes de error
                foreach (var error in result.Errors)
                {
                    string mensaje = error.Description;

                    if (mensaje.Contains("Passwords must have at least one non alphanumeric character"))
                        mensaje = "La contraseña debe contener al menos un carácter especial.";
                    else if (mensaje.Contains("Passwords must have at least one digit"))
                        mensaje = "La contraseña debe contener al menos un número.";
                    else if (mensaje.Contains("Passwords must have at least one uppercase"))
                        mensaje = "La contraseña debe contener al menos una letra mayúscula.";
                    else if (mensaje.Contains("Passwords must have at least one lowercase"))
                        mensaje = "La contraseña debe contener al menos una letra minúscula.";
                    else if (mensaje.Contains("Passwords must be at least"))
                        mensaje = "La contraseña no cumple con la longitud mínima requerida.";
                    else if (mensaje.Contains("is already taken"))
                        mensaje = "El correo electrónico ya está en uso.";

                    ModelState.AddModelError(string.Empty, mensaje);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"No se puede crear una instancia de '{nameof(ApplicationUser)}'. " +
                    $"Asegúrate de que '{nameof(ApplicationUser)}' no sea una clase abstracta y tenga un constructor sin parámetros, o alternativamente " +
                    $"sobrescribe la página de registro en /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("La interfaz predeterminada requiere un store de usuarios con soporte de correo electrónico.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}