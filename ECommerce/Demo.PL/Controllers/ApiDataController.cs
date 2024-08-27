using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;
using Demo.BBL.Interfaces;
using Demo.BBL.Repositories;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Demo.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiDataController: ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApiDataController(IUnitOfWork unitOfWork, 
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _unitOfWork.Repository<Product>().GetAllAsync();

                if (products == null || !products.Any())
                    return NotFound("No products found.");

                return Ok(new { data = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetAllCompanies")]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                var companies = await _unitOfWork.Repository<Company>().GetAllAsync();

                if (companies == null || !companies.Any())
                    return NotFound("No companies found.");

                return Ok(new { data = companies });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAllOrders(string? status)
        {
            try
            {
                IEnumerable<OrderHeader> orders;

                if (User.IsInRole(SD.Admin) || User.IsInRole(SD.Employee))
                {
                    orders = _unitOfWork.Repository<OrderHeader>().GetAllWithFilter(includeProperty: "AppUser");
                }
                else
                {
                    var email = User.FindFirstValue(ClaimTypes.Email);
                    var user = await _userManager.FindByEmailAsync(email);

                    orders = _unitOfWork.Repository<OrderHeader>()
                        .GetAllWithFilter(u=>u.AppUserId == user.Id,includeProperty: "AppUser");

                }
                if (orders is null  || !orders.Any())
                    return NotFound("No AppUser found.");

                switch (status)
                {
                    case "pending":
                        orders = orders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                        break;
                    case "inprocess":
                        orders = orders.Where(u => u.PaymentStatus == SD.StatusInProcess);
                        break;
                    case "completed":
                        orders = orders.Where(u => u.PaymentStatus == SD.StatusShipped);
                        break;
                    case "approved":
                        orders = orders.Where(u => u.PaymentStatus == SD.StatusApproved);
                        break;
                    default:
                        break;

                }


                return Ok(new { data = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Include(u=>u.Company)
                    .ToListAsync();
                
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    user.Role = roles.FirstOrDefault();

                    user.Company ??= new Company()
                    {
                        Name = ""
                    };
                }

                return Ok(new { data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
