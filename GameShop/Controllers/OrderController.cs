using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shop.DataAccess.Repository.IRepository;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;
using Stripe;
using System.Diagnostics;
using System.Security.Claims;

namespace GameShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles=SD.Role_Admin)]
        public IActionResult UpdateOrderDetail(int orderId)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.Name= OrderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber= OrderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress= OrderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City= OrderVM.orderHeader.City;
            orderHeaderFromDb.State= OrderVM.orderHeader.State;
            orderHeaderFromDb.ZipCode= OrderVM.orderHeader.ZipCode;
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Update Successfully!";

            return RedirectToAction(nameof(Details),new {orderId=orderHeaderFromDb.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id,SD.statusProcessing);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Update Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult ShipOrder()
        {
            var orderHeader=_unitOfWork.OrderHeader.Get(u=>u.Id==OrderVM.orderHeader.Id);
            orderHeader.TrackingNumber= OrderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier= OrderVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.statusShipped;
            orderHeader.ShippingDate= DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["success"] = "Order Shipped Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelOrder()
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.orderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(option);
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.statusCancelled,SD.statusRefunded);
            }
            else
            {
             _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.statusCancelled);

            }


            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Customer)]
        public IActionResult DeletePendingOrder()
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.orderHeader.Id);
            
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.statusCancelled);

           


            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Customer)]
        public IActionResult CompletedOrder()
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.orderHeader.Id);

            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.statusCompleted);




            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaders= _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders=objOrderHeaders.Where(u=>u.PaymentStatus==SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.statusProcessing);
                    break;
                case "completed":
                    objOrderHeaders=objOrderHeaders.Where(u=>u.OrderStatus == SD.statusCompleted);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.statusApproved);
                    break;
                case "Shipped":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.statusShipped);
                    break;
                default:
                    break;


            }
       

            return Json(new { data = objOrderHeaders });

        }

        #endregion
    }
}
