using System.Diagnostics;
using System.Security.Claims;
using Demo.BBL.Interfaces;
using Demo.BBL.Repositories;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Demo.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Demo.PL.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartViewModel shoppingCartVM { get; set; }

        private readonly UserManager<AppUser> _userManager;

        public CartController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        #region Indexs
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

             shoppingCartVM = new ShoppingCartViewModel
            {
                ShoppingCartList = _unitOfWork.Repository<ShoppingCart>()
                 .GetAllWithFilter(u => u.AppUserId == user.Id, includeProperty: "Product"),
                OrderHeader = new()
             };

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = _unitOfWork.Repository<ProductImage>()
                    .GetAllWithFilter(i => i.ProductId == cart.ProductId).ToList();
                cart.Price = GetPrice(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        } 
        #endregion


        #region Plus
        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _unitOfWork.Repository<ShoppingCart>().GetByIdAsync(cartId);
            cart.Count += 1;

            _unitOfWork.Repository<ShoppingCart>().Update(cart);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        } 
        #endregion


        #region Minus
        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _unitOfWork.Repository<ShoppingCart>().GetByIdAsync(cartId);
            if (cart?.Count <= 1)
            {
                _unitOfWork.Repository<ShoppingCart>().Delete(cart);

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.Repository<ShoppingCart>().
                        GetAllWithFilter(u => u.AppUserId == cart.AppUserId).Count() - 1);
            }
            else
            {
                cart.Count -= 1;
                _unitOfWork.Repository<ShoppingCart>().Update(cart);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }
        #endregion


        #region Delete
        public async Task<IActionResult> Delete(int cartId)
        {
            var cart = await _unitOfWork.Repository<ShoppingCart>().GetByIdAsync(cartId);

            if (cart is null)
            {
                return NotFound();
            }

            _unitOfWork.Repository<ShoppingCart>().Delete(cart);

            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.Repository<ShoppingCart>().
                    GetAllWithFilter(u => u.AppUserId == cart.AppUserId).Count() - 1);

            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }
        #endregion


        #region Summary
        public async Task<IActionResult> Summary()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            shoppingCartVM = new ShoppingCartViewModel
            {
                ShoppingCartList = _unitOfWork.Repository<ShoppingCart>()
                    .GetAllWithFilter(u => u.AppUserId == user.Id, includeProperty: "Product"),

                OrderHeader = new()
            };

            if (shoppingCartVM.ShoppingCartList?.Count() <= 0)
            {
                TempData["error"] = "Choose your Books.";
                return RedirectToAction(nameof(Index), "Home");
            }

            shoppingCartVM.OrderHeader.AppUser = (AppUser)user;

            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.AppUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.AppUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.AppUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.AppUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.AppUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.AppUser.PostalCode;


            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }
        #endregion


        #region Summary Post


        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);



            shoppingCartVM.ShoppingCartList = _unitOfWork.Repository<ShoppingCart>()
                .GetAllWithFilter(u => u.AppUserId == user.Id, includeProperty: "Product");

            var appUser = await _unitOfWork.AppUserRepository
                .GetWithFilterAsync(u => u.Id == user.Id, includeProperty: "Company");

            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.AppUserId = user.Id;

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (appUser?.CompanyId.GetValueOrDefault() == 0)
            {
                // not Company
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                // Company
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            await _unitOfWork.OrderHeaderRepository.AddAsync(shoppingCartVM.OrderHeader);
            await _unitOfWork.CompleteAsync();

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                var orderDetails = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                await _unitOfWork.Repository<OrderDetail>().AddAsync(orderDetails);
                await _unitOfWork.CompleteAsync();
            }

            if (appUser?.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7139/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",

                };

                foreach (var item in shoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                var session = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                await _unitOfWork.CompleteAsync();

                Response.Headers.Add("Location", session.Url);

                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartVM.OrderHeader.Id });
        }
        #endregion


        #region Order Confirmation
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var orderHeader = await _unitOfWork.OrderHeaderRepository.GetWithFilterAsync(u => u.Id == id, includeProperty: "AppUser");

            if (orderHeader?.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                var session = service.Get(orderHeader?.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);

                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);

                    await _unitOfWork.CompleteAsync();
                }
            }


            var shoppingCarts = _unitOfWork.Repository<ShoppingCart>()
                .GetAllWithFilter(u => u.AppUserId == orderHeader.AppUserId).ToList();

            _unitOfWork.Repository<ShoppingCart>().DeleteRange(shoppingCarts);

            await _unitOfWork.CompleteAsync();
            HttpContext.Session.Clear();

            return View(id);
        } 
        #endregion


        private double GetPrice(ShoppingCart cart)
        {
            if (cart.Count <= 50)
            {
                return cart.Product.Price;
            }
            else if (cart.Count <= 50)
            {
                return cart.Product.Price50;
            }
            else
            {
                return cart.Product.Price100;
            }
        }
    }
}
 