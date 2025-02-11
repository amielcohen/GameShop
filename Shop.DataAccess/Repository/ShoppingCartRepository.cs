using Shop.DataAccess.Data;
using Shop.DataAccess.Repository.IRepository;
using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Shop.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {

        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);        }
    }
}
