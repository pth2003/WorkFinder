using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Controllers
{
    [Route("admin-tools")]
    public class AdminToolsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminToolsController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("grant-admin/{email}")]
        public async Task<IActionResult> GrantAdminRole(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email không được để trống");
            }

            try
            {
                // Tìm người dùng theo email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy người dùng với email: {email}");
                }

                // Kiểm tra và tạo role Admin nếu chưa tồn tại
                var roleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExists)
                {
                    var role = new IdentityRole<int>("Admin");
                    var createRoleResult = await _roleManager.CreateAsync(role);
                    if (!createRoleResult.Succeeded)
                    {
                        return StatusCode(500, $"Không thể tạo role Admin: {string.Join(", ", createRoleResult.Errors.Select(e => e.Description))}");
                    }
                }

                // Kiểm tra xem người dùng đã có role Admin chưa
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return Ok($"Người dùng {email} đã có quyền Admin.");
                }

                // Gán role Admin cho người dùng
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    return Ok($"Đã cấp quyền Admin cho người dùng {email} thành công.");
                }
                else
                {
                    return StatusCode(500, $"Không thể cấp quyền Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpGet("check-roles/{email}")]
        public async Task<IActionResult> CheckUserRoles(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email không được để trống");
            }

            try
            {
                // Tìm người dùng theo email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy người dùng với email: {email}");
                }

                // Lấy danh sách các roles của người dùng
                var roles = await _userManager.GetRolesAsync(user);

                // Lấy thông tin chi tiết về người dùng
                var userInfo = new
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        [HttpGet("list-roles")]
        public async Task<IActionResult> ListRoles()
        {
            try
            {
                var roles = _roleManager.Roles.Select(r => new
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}