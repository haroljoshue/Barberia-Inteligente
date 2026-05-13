using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using System.Linq;
using System.Threading.Tasks;

namespace Barberia.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: api/roles
        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        // GET: api/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRol(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return NotFound(new { mensaje = "Rol no encontrado" });
            return Ok(rol);
        }

        // POST: api/roles/crear
        [HttpPost("crear")]
        public async Task<IActionResult> CrearRol([FromBody] string nombreRol)
        {
            if (string.IsNullOrWhiteSpace(nombreRol))
                return BadRequest("El nombre del rol es obligatorio.");

            if (await _roleManager.RoleExistsAsync(nombreRol))
                return BadRequest("El rol ya existe.");

            var result = await _roleManager.CreateAsync(new IdentityRole(nombreRol));
            if (result.Succeeded)
                return Ok(new { mensaje = $"Rol {nombreRol} creado correctamente" });

            return BadRequest(result.Errors);
        }

        // PUT: api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRol(string id, [FromBody] IdentityRole rol)
        {
            var existing = await _roleManager.FindByIdAsync(id);
            if (existing == null) return NotFound(new { mensaje = "Rol no encontrado" });

            existing.Name = rol.Name;

            var result = await _roleManager.UpdateAsync(existing);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { mensaje = "Rol actualizado correctamente", existing });
        }

        // DELETE: api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return NotFound(new { mensaje = "Rol no encontrado" });

            var result = await _roleManager.DeleteAsync(rol);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { mensaje = "Rol eliminado correctamente" });
        }

        // POST: api/roles/asignar
        [HttpPost("asignar")]
        public async Task<IActionResult> AsignarRol(string userId, string nombreRol)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (!await _roleManager.RoleExistsAsync(nombreRol))
                return BadRequest(new { mensaje = "El rol no existe" });

            var result = await _userManager.AddToRoleAsync(usuario, nombreRol);
            if (result.Succeeded)
                return Ok(new { mensaje = $"Rol {nombreRol} asignado a {usuario.UserName}" });

            return BadRequest(result.Errors);
        }
    }
}
