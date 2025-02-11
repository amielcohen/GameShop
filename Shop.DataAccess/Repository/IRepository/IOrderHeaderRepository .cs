using Shop.DataAccess.Repository.IRepositor;
using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id,string OrderStatus,string? paymentStatus=null);
        void UpdateStripePaymentId(int id,string sessionId,string PaymentIntId);
    }
}
