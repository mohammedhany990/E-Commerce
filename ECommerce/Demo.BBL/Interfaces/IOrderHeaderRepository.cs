using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.DAL.Models;

namespace Demo.BBL.Interfaces
{
    public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
    {
        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    }
}
