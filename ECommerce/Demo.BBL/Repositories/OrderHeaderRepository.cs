using Demo.BBL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.DAL.Data.Contexts;
using Demo.DAL.Models;

namespace Demo.BBL.Repositories
{
    public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ECommerceDbContext _dbContext;

        public OrderHeaderRepository(ECommerceDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var order = _dbContext.OrderHeaders.FirstOrDefault(u => u.Id == id);

            if (order is not null)
            {
                order.OrderStatus = orderStatus;

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    order.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var order = _dbContext.OrderHeaders.FirstOrDefault(u => u.Id == id);

            if (!string.IsNullOrEmpty(sessionId))
            {
                order.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                order.PaymentIntentId = paymentIntentId;
                order.PaymentDate = DateTime.Now;
            }
        }
    }
}

