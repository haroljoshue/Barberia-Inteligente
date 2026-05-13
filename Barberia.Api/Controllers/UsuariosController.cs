using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;

namespace BarberiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // POST api/usuarios/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ApplicationUser usuario, string password)
        {
            var result = await _userManager.CreateAsync(usuario, password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { mensaje = "Usuario creado correctamente", usuario.Id });
        }

        // POST api/usuarios/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            if (result.Succeeded)
                return Ok(new { mensaje = "Login exitoso" });

            return Unauthorized(new { mensaje = "Credenciales inválidas" });
        }

        // POST api/usuarios/crear-rol
        [HttpPost("crear-rol")]
        public async Task<IActionResult> CrearRol(string nombreRol)
        {
            if (!await _roleManager.RoleExistsAsync(nombreRol))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(nombreRol));
                if (!result.Succeeded)
                    return BadRequest(result.Errors);
            }
            return Ok(new { mensaje = $"Rol {nombreRol} creado correctamente" });
        }

        // POST api/usuarios/asignar-rol
        [HttpPost("asignar-rol")]
        public async Task<IActionResult> AsignarRol(string userId, string nombreRol)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            var result = await _userManager.AddToRoleAsync(usuario, nombreRol);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            usuario.RolSistema = nombreRol; // tu propiedad extra
            await _userManager.UpdateAsync(usuario);

            return Ok(new { mensaje = $"Rol {nombreRol} asignado a {usuario.UserName}" });
        }

        // GET api/usuarios
        [HttpGet]
        public IActionResult GetUsuarios()
        {
            var usuarios = _userManager.Users.ToList();
            return Ok(usuarios);
        }

        // GET api/usuarios/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });
            return Ok(usuario);
        }

        // PUT api/usuarios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(string id, [FromBody] ApplicationUser usuario)
        {
            var existing = await _userManager.FindByIdAsync(id);
            if (existing == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            existing.UserName = usuario.UserName;
            existing.Email = usuario.Email;
            existing.RolSistema = usuario.RolSistema;
            existing.Activo = usuario.Activo;

            var result = await _userManager.UpdateAsync(existing);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { mensaje = "Usuario actualizado correctamente", existing });
        }

        // DELETE api/usuarios/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            var result = await _userManager.DeleteAsync(usuario);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { mensaje = "Usuario eliminado correctamente" });
        }
    }
}
