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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {

        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);        }

        public void UpdateStatus(int id, string OrderStatus, string? paymentStatus = null)
        {
            var orderFromDb=_db.OrderHeaders.FirstOrDefault(u=>u.Id == id);
            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus = OrderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string PaymentIntId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.sessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(PaymentIntId))
            {
                orderFromDb.PaymentIntentId = PaymentIntId;
                orderFromDb.PaymentDate= DateTime.Now;
            }

        }
    }
}
