using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ModelosBarberia;
using Barberia.MVC.Services;
using ModelosBarberia.Enum;

namespace Barberia.MVC.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly LogSistemaService _logSistemaService;
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            LogSistemaService logSistemaService,
            IHttpClientFactory httpClientFactory) // <-- inyectado
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _logSistemaService = logSistemaService;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(100)]
            [Display(Name = "Nombre completo")]
            public string NombreCompleto { get; set; }

            [Required]
            [Display(Name = "Rol")]
            public string RolSistema { get; set; }
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
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    NombreCompleto = Input.NombreCompleto,
                    RolSistema = Input.RolSistema,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Autenticacion,
                        mensaje: "Usuario registrado correctamente.",
                        entidad: "Usuario",
                        entidadId: user.Id,
                        stackTrace: null,
                        usuarioId: user.Id,
                        latenciaMs: null,
                        exitoso: true
                    );

                    // asignar rol en Identity
                    if (!string.IsNullOrEmpty(Input.RolSistema))
                    {
                        await _userManager.AddToRoleAsync(user, Input.RolSistema);
                        if (!string.IsNullOrEmpty(Input.RolSistema))
                        {
                            await _userManager.AddToRoleAsync(user, Input.RolSistema);

                            // Si el rol es Barbero, crear registro en la API
                            if (Input.RolSistema == "Barbero")
                            {
                                var client = _httpClientFactory.CreateClient("BarberiaApi");

                                var nuevoBarbero = new Barbero
                                {
                                    UserId = user.Id,
                                    Nombre = Input.NombreCompleto,
                                    Email = Input.Email,
                                    Disponible = true,
                                    FechaRegistro = DateTime.UtcNow
                                };

                                await client.PostAsJsonAsync("api/barberos", nuevoBarbero);
                            }
                        }


                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Validacion,
                    mensaje: $"Error al registrar usuario: {Input.Email}.",
                    entidad: "Usuario",
                    entidadId: null,
                    stackTrace: string.Join(" | ", result.Errors.Select(e => e.Description)),
                    usuarioId: null,
                    latenciaMs: null,
                    exitoso: false
                );

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
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
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not abstract and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
