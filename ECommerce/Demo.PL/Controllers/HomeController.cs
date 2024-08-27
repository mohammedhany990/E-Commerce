using Demo.PL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using AutoMapper;
using Demo.BBL.Interfaces;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Demo.PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var products = _unitOfWork.Repository<Product>()
                .GetAllWithFilter(includeProperty: "ProductImages");

            var mappedProducts = _mapper.Map<IEnumerable<Product>,
                IEnumerable<ProductViewModel>>(products);

            return View(mappedProducts);
        }


        #region Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return BadRequest();
            }

            var product = await _unitOfWork.Repository<Product>()
                .GetWithFilterAsync(i => i.Id == id, includeProperty: "Category,ProductImages");


            if (product is null)
            {
                return NotFound();
            }

            ShoppingCart cart = new ShoppingCart
            {
                Product = product,
                Count = 1,
                ProductId = id.Value
            };


            return View(cart);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 0)
            {
                return RedirectToAction(nameof(Index));
            }
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            shoppingCart.AppUserId = user.Id;

            var shoppingCartDb = await _unitOfWork.Repository<ShoppingCart>()
                .GetWithFilterAsync(i => i.ProductId == shoppingCart.ProductId &&
                                         i.AppUserId == shoppingCart.AppUserId);

            if (shoppingCartDb is not null)
            {
                shoppingCartDb.Count += shoppingCart.Count;
                _unitOfWork.Repository<ShoppingCart>().Update(shoppingCartDb);

                await _unitOfWork.CompleteAsync();
            }
            else
            {
                shoppingCart.Id = 0;
                await _unitOfWork.Repository<ShoppingCart>().AddAsync(shoppingCart);
                await _unitOfWork.CompleteAsync();

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.Repository<ShoppingCart>().
                        GetAllWithFilter(u => u.AppUserId == user.Id).Count());

            }

            TempData["success"] = "Cart Updated Successfully";
            return RedirectToAction(nameof(Index));
        }
        #endregion




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
