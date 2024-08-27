using Demo.BBL.Interfaces;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Demo.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Demo.PL.Controllers
{
    [Authorize(Roles = SD.Admin + "," + SD.Employee)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> RoleManagement(string id)
        {
            var users = await _userManager.Users
                .Include(u => u.Company)
                .ToListAsync();

            var user = users.FirstOrDefault(u => u.Id == id);

            var roles = _roleManager.Roles.ToList();
           
            var companies = await _unitOfWork.Repository<Category>().GetAllAsync();

            var roleManagementVM = new RoleManagementViewModel
            {
                AppUser = user,

                RoleList = roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),

                CompanyList = companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            roleManagementVM.AppUser.Role = userRoles.FirstOrDefault();

            return View(roleManagementVM);
        }

        [HttpPost]
        public async Task<IActionResult> RoleManagement(RoleManagementViewModel roleManagementViewModel)
        {
            var users = await _userManager.Users
                .Include(u => u.Company)
                .ToListAsync();

            var user = users.FirstOrDefault(u => u.Id == roleManagementViewModel.AppUser.Id);

            var userRoles = await _userManager.GetRolesAsync(user);
            var oldUserRole = userRoles.FirstOrDefault();

            if (roleManagementViewModel.AppUser.Role != oldUserRole)
            {
                if (roleManagementViewModel.AppUser.Role == SD.Company)
                {
                    user.CompanyId = roleManagementViewModel.AppUser.CompanyId;
                }

                if (oldUserRole == SD.Company)
                {
                    user.CompanyId = null;
                }

                await _unitOfWork.CompleteAsync();

                await _userManager.RemoveFromRoleAsync(user, oldUserRole);

                await _userManager.AddToRoleAsync(user, roleManagementViewModel.AppUser.Role);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user.LockoutEnd is not null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            await _unitOfWork.CompleteAsync();
            return Json(new { success = true, message = "Operation Done Successfully." });
        }


      

    }
}
