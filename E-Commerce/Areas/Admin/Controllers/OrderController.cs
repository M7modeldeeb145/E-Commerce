using DeeboStore.DataAccess.Repository;
using DeeboStore.DataAccess.Repository.IRepository;
using DeeboStore.Models;
using DeeboStore.Models.ViewModels;
using DeeboStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private UserManager<IdentityUser> _userManager;
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index(int orderId)
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Prdocut")
            };
            return View(orderVM);
        }
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        [HttpPost]
        public IActionResult UpdateOrderDetail()
        {
            var orderheaderfromdb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderheaderfromdb.Name = orderVM.OrderHeader.Name;
            orderheaderfromdb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderheaderfromdb.Street = orderVM.OrderHeader.Street;
            orderheaderfromdb.City = orderVM.OrderHeader.City;
            orderheaderfromdb.Governate = orderVM.OrderHeader.Governate;
            orderheaderfromdb.PostalCode = orderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderheaderfromdb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrickingNumber))
            {
                orderheaderfromdb.Carrier = orderVM.OrderHeader.TrickingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderheaderfromdb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", new {orderId = orderheaderfromdb.Id});
        }
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[HttpPost]
		public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
			TempData["Success"] = "Order Details Updated Successfully.";
			return RedirectToAction("Details", new { orderId = orderVM.OrderHeader.Id });

		}
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[HttpPost]
		public IActionResult ShipOrder()
        {
			var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
			orderHeader.TrickingNumber = orderVM.OrderHeader.TrickingNumber;
			orderHeader.Carrier = orderVM.OrderHeader.Carrier;
			orderHeader.OrderStatus = SD.StatusShipped;
			orderHeader.ShippingDate = DateTime.Now;
			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayed)
			{
				orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
			}
			_unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusShipped);
            _unitOfWork.Save();
			TempData["Success"] = "Order Shipped Successfully.";
			return RedirectToAction("Details", new { orderId = orderVM.OrderHeader.Id });

		}
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[HttpPost]
		public IActionResult CancelOrder()
        {
			var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
			if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeader.PaymentIntentId
				};

				var service = new RefundService();
				Refund refund = service.Create(options);

				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
			}
			_unitOfWork.Save();
			TempData["Success"] = "Order Cancelled Successfully.";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}
		[ActionName("Details")]
		[HttpPost]
		public IActionResult Details_PAY_NOW()
		{
			orderVM.OrderHeader = _unitOfWork.OrderHeader
				.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			orderVM.OrderDetail = _unitOfWork.OrderDetail
				.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Product");
			//stripe logic
			var domain = Request.Scheme + "://" + Request.Host.Value + "/";
			var options = new SessionCreateOptions
			{
				SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
				CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
			};
			foreach (var item in orderVM.OrderDetail)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
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
			Session session = service.Create(options);
			_unitOfWork.OrderHeader.UpdateStripePaymentID(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
			_unitOfWork.Save();
			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}

		public IActionResult PaymentConfirmation(int orderHeaderId)
		{

			OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayed)
			{
				//this is an order by company
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);

				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
			}
			return View(orderHeaderId);
		}
		#region API Calls

		[HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> Orderheaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)) 
            {
                Orderheaders= _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                Orderheaders = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == userId, includeProperties:"ApplicationUser");
            }
            switch (status)
            {
                case "pending":
                    Orderheaders = Orderheaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayed);
                    break;
                case "inprocess":
                    Orderheaders = Orderheaders.Where(u => u.PaymentStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    Orderheaders = Orderheaders.Where(u => u.PaymentStatus == SD.StatusShipped);
                    break;
                case "approved":
                    Orderheaders = Orderheaders.Where(u => u.PaymentStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = Orderheaders });
        }

        #endregion
    }
}
