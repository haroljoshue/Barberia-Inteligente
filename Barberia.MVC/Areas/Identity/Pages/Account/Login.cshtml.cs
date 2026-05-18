using Barberia.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ModelosBarberia;
using ModelosBarberia.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Barberia.MVC.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly LogSistemaService _logSistemaService;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger,
            LogSistemaService logSistemaService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _logSistemaService = logSistemaService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var inicio = DateTime.UtcNow;

                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false
                );

                var latenciaMs = (int)(DateTime.UtcNow - inicio).TotalMilliseconds;

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Autenticacion,
                        mensaje: "Inicio de sesión exitoso.",
                        entidad: "Usuario",
                        entidadId: user?.Id,
                        stackTrace: null,
                        usuarioId: user?.Id,
                        latenciaMs: latenciaMs,
                        exitoso: true
                    );

                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Autenticacion,
                        mensaje: "Inicio de sesión requiere autenticación de dos factores.",
                        entidad: "Usuario",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: null,
                        latenciaMs: latenciaMs,
                        exitoso: false
                    );

                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Seguridad,
                        mensaje: $"Cuenta bloqueada durante intento de inicio de sesión: {Input.Email}.",
                        entidad: "Usuario",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: null,
                        latenciaMs: latenciaMs,
                        exitoso: false
                    );

                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Seguridad,
                    mensaje: $"Intento fallido de inicio de sesión para el correo: {Input.Email}.",
                    entidad: "Usuario",
                    entidadId: null,
                    stackTrace: null,
                    usuarioId: null,
                    latenciaMs: latenciaMs,
                    exitoso: false
                );

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            await _logSistemaService.RegistrarAsync(
                tipo: TipoLogSistema.Validacion,
                mensaje: "Formulario de inicio de sesión inválido.",
                entidad: "Login",
                entidadId: null,
                stackTrace: null,
                usuarioId: null,
                latenciaMs: null,
                exitoso: false
            );

            return Page();
        }
    }
}
