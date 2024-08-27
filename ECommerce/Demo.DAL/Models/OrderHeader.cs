using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Demo.DAL.Models
{
    public class OrderHeader : BaseEntity
    {
        

        public string AppUserId { get; set; }

        public AppUser AppUser { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime ShippingDate { get; set; }

        public double OrderTotal { get; set; }

        public string? OrderStatus { get; set; }

        public string? PaymentStatus { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Carrier { get; set; }

        public DateTime PaymentDate{ get; set; }

        public DateOnly PaymentDueDate { get; set; }


        public string? SessionId { get; set; }
        
        public string? PaymentIntentId { get; set; }

        public string Name { get; set; }
        
        public string? StreetAddress { get; set; }
        
        public string? City { get; set; }
        
        public string? State { get; set; }
        
        public string? PostalCode { get; set; }
        
        public string? PhoneNumber { get; set; }

    }
}
