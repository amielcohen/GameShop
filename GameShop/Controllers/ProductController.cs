using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shop.DataAccess.Repository.IRepository;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;
using System.Drawing;

namespace GameShop.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
          
            return View(objProductList);
        }
        public IActionResult Upsert(int? id) //Update+Insert
        {
            ProductVM productVM = new()
            {
                CategoryList =  _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.id.ToString()

                }),

            Product = new Product()
            };
            if(id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product=_unitOfWork.Product.Get(u=>u.id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file!=null)
                {
                    string fileName=Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    string producrPath=Path.Combine(wwwRootPath,@"images\product");

                    if(!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        //delete old image
                        var oldImagePath=Path.Combine(wwwRootPath,obj.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }

                    using(var fileStream=new FileStream(Path.Combine(producrPath, fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl=@"\images\product\"+fileName;
                }

                if(obj.Product.id==0) //add
                {
                    if(file==null) {
                        obj.Product.ImageUrl = @"\images\product\noImageAvailable.jpg";
                    }
                    obj.Product.SKU ="";

                    _unitOfWork.Product.Add(obj.Product);
                   // obj.Product.SKU = $"#0{obj.Product.CategoryId}0{obj.Product.id}";

                }
                else //update
                {
                    _unitOfWork.Product.Update(obj.Product);
                }
            
                _unitOfWork.Save();
                obj.Product.SKU = $"#0{obj.Product.CategoryId}0{obj.Product.id}";
                _unitOfWork.Product.Update(obj.Product);
                _unitOfWork.Save();

                TempData["success"] = "Product Updated Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                obj.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.id.ToString()

                }); 
                return View(obj);

            }
           
        }

        
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    Product productfromDb = _unitOfWork.Product.Get(u => u.id == id);
        //    if (productfromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productfromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult EditPost(int? id)
        //{
        //    Product obj = _unitOfWork.Product.Get(u => u.id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }

        //    _unitOfWork.Product.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product Deleted Successfully";
        //    return RedirectToAction("Index");

        //}


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });

        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var ProductToBeDeleted = _unitOfWork.Product.Get(u => u.id == id);
            if(ProductToBeDeleted == null)
            {
                return Json(new {success = false,message="Error while deleting"});
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, ProductToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(ProductToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Succesful" });

        }
        #endregion
    }
}
