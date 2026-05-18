using System;
using System.Threading.Tasks;
using Barberia.MVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ModelosBarberia;
using ModelosBarberia.Enum;
using System.Security.Claims;

namespace Barberia.MVC.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly LogSistemaService _logSistemaService;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LogoutModel> logger,
            LogSistemaService logSistemaService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _logSistemaService = logSistemaService;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _logSistemaService.RegistrarAsync(
                tipo: TipoLogSistema.Autenticacion,
                mensaje: "Cierre de sesión realizado.",
                entidad: "Usuario",
                entidadId: usuarioId,
                stackTrace: null,
                usuarioId: usuarioId,
                latenciaMs: null,
                exitoso: true
            );

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User logged out.");

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }

            // Redirect para que el navegador haga un nuevo request y se actualice la identidad
            return RedirectToPage();
        }
    }
}
