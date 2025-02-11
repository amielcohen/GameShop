using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Shop.DataAccess.Repository.IRepository;
using Shop.Models;
using Shop.Models.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace GameShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> CategoryList = _unitOfWork.Category.GetAll();
            ViewBag.Categories = CategoryList;

            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }
        public IActionResult NameSearch(string searchName)
        {
            IEnumerable<Category> CategoryList = _unitOfWork.Category.GetAll();
            ViewBag.Categories = CategoryList;

            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(u => u.GameName.Contains(searchName) || (u.SKU.Equals(searchName)), includeProperties: "Category");
            return View("Index", productList);
        }

        public IActionResult Filter(int priceFrom, int priceTo, int yearFrom, int yearTo, string platform, int category, int PEGI, string saleNpopular)
        {
            IEnumerable<Category> CategoryList = _unitOfWork.Category.GetAll();
            ViewBag.Categories = CategoryList;

            ViewBag.SelectedPlatform = platform;
            ViewBag.priceFrom = priceFrom;
            ViewBag.priceTo = priceTo;
            ViewBag.yearFrom = yearFrom;
            ViewBag.yearTo = yearTo;
            ViewBag.category = category;
            ViewBag.PEGI = PEGI;
            ViewBag.saleNpopular = saleNpopular;

            IEnumerable<Product> productList = _unitOfWork.Product.GetAll();

            
            if (priceFrom != 0)
            {
                productList = productList.Where(u => u.Price >= priceFrom);

            }
            if (priceTo != 0)
            {
                productList = productList.Where(u => u.Price <= priceTo);

            }
             if (yearFrom != 0)
            {
                productList = productList.Where(u => u.ReleaseYear >= yearFrom);

            }
            if (yearTo != 0)
            {
                productList = productList.Where(u => u.ReleaseYear <= yearTo);

            }


            if (category != 0)
            {
                productList = productList.Where(p => p.CategoryId == category);

            }
            if (platform != "All")
            {
                productList = productList.Where(p => p.Platform == platform);

            }

            if (PEGI != 0)
            {
                productList = productList.Where(p => p.PEGI == PEGI);

            }

            if (saleNpopular == "Most Popular")
            {
                productList = productList.Where(p => p.BestSeller == true);

            }

            if (saleNpopular == "On Sale")
            {
                productList = productList.Where(p => p.Discount > 0);

            }
            if (saleNpopular == "On Sale&Most Popular")
            {
                productList = productList.Where(p => p.Discount > 0 && p.BestSeller == true);

            }

            return View("Index", productList);
        }



        public IActionResult Details(int productid)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.id == productid, includeProperties: "Category"),
                Count = 1,
                ProductId = productid
            };

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingcart, string submitButton)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userid = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingcart.ApplicationUserId = userid;
            if (submitButton == "Submit")
            {
                ShoppingCart cartfromdb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userid &&
                u.ProductId == shoppingcart.ProductId);

                if (cartfromdb != null)
                {
                    //shopping cart exists
                    cartfromdb.Count += shoppingcart.Count;
                    _unitOfWork.ShoppingCart.Update(cartfromdb);
                }
                else
                {
                    //add new cart
                    _unitOfWork.ShoppingCart.Add(shoppingcart);
                }
                TempData["success"] = "Cart updated successfully";

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else if(submitButton == "Notify")
            {
                Notify Notifyfromdb = _unitOfWork.Notify.Get(u => u.ApplicationUserId == userid &&
                u.ProductId == shoppingcart.ProductId);

                if( Notifyfromdb == null)
                {
                    Notify notify = new () {
                    ApplicationUserId = userid,
                    ProductId = shoppingcart.ProductId,
                };
                _unitOfWork.Notify.Add(notify);
                _unitOfWork.Save();
                }

                
                TempData["success"] = "you will notify when product is back to stock!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ShoppingCartVM cartsfromdb = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userid,
                   includeProperties: "Product")

                };
     

                List<ShoppingCart> shoppingCarts = cartsfromdb.ShoppingCartList.ToList();
                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();
                _unitOfWork.ShoppingCart.Add(shoppingcart);
                _unitOfWork.Save();

                return RedirectToAction("Summary", "Cart");

            }
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
