using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Shop.DataAccess.Data;
using Shop.DataAccess.Repository.IRepository;
using Shop.Models;
using Shop.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Shop.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>,IProductRepository
    {

        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        

        public void Update(Product obj)
        {
            var objFromDb = _db.Products.FirstOrDefault(u => u.id == obj.id);
            if (objFromDb != null)
            {
                objFromDb.Price = obj.Price;
                objFromDb.Category = obj.Category;
                objFromDb.Description = obj.Description;
                objFromDb.ReleaseYear = obj.ReleaseYear;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.GameName = obj.GameName;
                objFromDb.Platform= obj.Platform;
                objFromDb.PEGI = obj.PEGI;
                objFromDb.SoldSoFar = obj.SoldSoFar;
                objFromDb.BestSeller= obj.BestSeller;
                objFromDb.Discount= obj.Discount;
                objFromDb.SKU = $"#0{obj.CategoryId}0{obj.id}";
                if (objFromDb.Stock == 0&& obj.Stock != 0)
                {
                    var NotifysFromdb = _db.Notify.Include(u => u.ApplicationUser).Include(u => u.Product)        
                         .Where(u => u.ProductId == objFromDb.id);
                    if(NotifysFromdb != null && NotifysFromdb.Any())
                    {
                        foreach (var Notify in NotifysFromdb)
                        {
                            string body = Notify.Product.GameName+ " is back in stock <a href='https://localhost:7142/Home/Details?productid=" + Notify.ProductId.ToString() + "'>clicking here to Buy Now!</a>.";
                            var emilsender = new EmailSender();
                            emilsender.SendEmailAsync(Notify.ApplicationUser.Email, "The product is back in stock",
                           body);
                           
                        } 
                        _db.Notify.RemoveRange(NotifysFromdb);
                        _db.SaveChanges();

                    }

                }
                objFromDb.Stock = obj.Stock;

                if (objFromDb.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
