using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shop.DataAccess.Repository.IRepository;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userid = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }
            ShoppingCartVM.OrderHeader.OrderTotal=(float)Math.Round(ShoppingCartVM.OrderHeader.OrderTotal, 2);
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {

            
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userid = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid,
                    includeProperties: "Product"),
                    OrderHeader = new()
                };

                ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userid);
                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.name;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.ZipCode = ShoppingCartVM.OrderHeader.ApplicationUser.ZipCode;

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
                }
                ShoppingCartVM.OrderHeader.OrderTotal = (float)Math.Round(ShoppingCartVM.OrderHeader.OrderTotal, 2);

            return View(ShoppingCartVM);
            
           
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userid = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userid;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userid);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }
            ShoppingCartVM.OrderHeader.OrderTotal = (float)Math.Round(ShoppingCartVM.OrderHeader.OrderTotal, 2);

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.statusPending;

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Product.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            //STRIPE 
            var domain = "https://localhost:7142/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain+ $"cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain+"cart/index",
                LineItems = new List<SessionLineItemOptions>(),
               
                Mode = "payment",
            };

            foreach(var item in ShoppingCartVM.ShoppingCartList) { 
            
                var sessionLineItem = new SessionLineItemOptions { 
                
                    PriceData = new SessionLineItemPriceDataOptions {
                    
                        UnitAmount = (long)(item.Product.Price*100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.GameName
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session =service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            //return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u=>u.Id == id,includeProperties:"ApplicationUser");
            var service=new SessionService();
            Session session=service.Get(orderHeader.sessionId);

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.
               GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();



            foreach (var product in shoppingCarts)
            {
                Product productFromDb = _unitOfWork.Product.Get(u => u.id == product.ProductId);
                productFromDb.SoldSoFar += product.Count;
                if(productFromDb.SoldSoFar >= 10) ///set what consider to be best seller
                {
                    productFromDb.BestSeller=true;
                }
                productFromDb.Stock -= product.Count;
                if (productFromDb.Stock < 0)
                {
                    return View("OutOfStock", productFromDb.GameName);
                }
                _unitOfWork.Product.Update(productFromDb);
            }
            _unitOfWork.Save();

            if (session.PaymentStatus.ToLower()=="paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.statusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }

           
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
           

            return View(id);
        }
        

        public IActionResult plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count == 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        public IActionResult remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}
