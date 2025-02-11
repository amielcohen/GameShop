using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }

        INotifyRepository Notify {  get; }
        ICreditCardRepository CreditCard {  get; }
        IApplicationUserRepository ApplicationUser { get; }

        void Save();

    }
}
