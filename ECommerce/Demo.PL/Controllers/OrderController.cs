using Demo.BBL.Interfaces;
using Demo.BBL.Repositories;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Demo.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;

namespace Demo.PL.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderViewModel orderViewModel { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
             orderViewModel = new OrderViewModel
            {
                OrderHeader =await _unitOfWork.Repository<OrderHeader>().GetWithFilterAsync(u=>u.Id == id, includeProperty:"AppUser"),
                OrderDetails = _unitOfWork.Repository <OrderDetail>().GetAllWithFilter(u => u.OrderHeaderId == id, includeProperty: "Product")
            };
            return View(orderViewModel);
        }

        [HttpPost]
        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        public async Task<IActionResult> UpdateOrderDetails()
        {
            var order =await _unitOfWork.Repository<OrderHeader>()
                .GetWithFilterAsync(u => u.Id == orderViewModel.OrderHeader.Id);

            order.Name = orderViewModel.OrderHeader.Name;
            order.PhoneNumber = orderViewModel.OrderHeader.PhoneNumber;
            order.StreetAddress = orderViewModel.OrderHeader.StreetAddress;
            order.City = orderViewModel.OrderHeader.City;
            order.State = orderViewModel.OrderHeader.State;
            order.PostalCode = orderViewModel.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(orderViewModel.OrderHeader.Carrier))
            {
                order.Carrier = orderViewModel.OrderHeader.Carrier;
            }

            if (!string.IsNullOrEmpty(orderViewModel.OrderHeader.TrackingNumber))
            {
                order.Carrier = orderViewModel.OrderHeader.TrackingNumber;
            }

            _unitOfWork.Repository<OrderHeader>().Update(order);
            await _unitOfWork.CompleteAsync();

            TempData["success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }


        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        public async Task<IActionResult> StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.
                UpdateStatus(orderViewModel.OrderHeader.Id, SD.StatusInProcess);

            await _unitOfWork.CompleteAsync();

            TempData["success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { id = orderViewModel.OrderHeader.Id });

        }


        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            var orderHeader =await _unitOfWork.Repository<OrderHeader>()
                .GetWithFilterAsync(u => u.Id == orderViewModel.OrderHeader.Id);

            orderHeader.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderViewModel.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.Repository<OrderHeader>().Update(orderHeader);
            await _unitOfWork.CompleteAsync();

            TempData["success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { id = orderViewModel.OrderHeader.Id });

        }


        [HttpPost]
        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        public async Task<IActionResult> CancelOrder()
        {

            var orderHeader = await _unitOfWork.Repository<OrderHeader>()
                .GetWithFilterAsync(u => u.Id == orderViewModel.OrderHeader.Id);

            if (orderHeader?.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                var refund = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            await _unitOfWork.CompleteAsync();


            TempData["success"] = "Order Cancelled Successfully.";

            return RedirectToAction(nameof(Details), new { id = orderViewModel.OrderHeader.Id });

        }


        [ActionName("Details")]
        [HttpPost]
        public async Task<IActionResult> Details_PAY_NOW()
        {
            orderViewModel.OrderHeader = await _unitOfWork.OrderHeaderRepository
                .GetWithFilterAsync(u => u.Id == orderViewModel.OrderHeader.Id, includeProperty: "AppUser");
            
            orderViewModel.OrderDetails = _unitOfWork.Repository<OrderDetail>()
                .GetAllWithFilter(u => u.OrderHeaderId == orderViewModel.OrderHeader.Id, includeProperty: "Product");

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"order/PaymentConfirmation?id={orderViewModel.OrderHeader.Id}",
                CancelUrl = domain + $"order/details?id={orderViewModel.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderViewModel.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), 
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
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

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(orderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            await _unitOfWork.CompleteAsync();

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }


        public async Task<IActionResult> PaymentConfirmation(int id)
        {
            var orderHeader =await _unitOfWork.Repository<OrderHeader>().GetWithFilterAsync(u => u.Id == id);
            
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                var session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    await _unitOfWork.CompleteAsync();
                }
            }
            return View(id);
        }


    }
}
