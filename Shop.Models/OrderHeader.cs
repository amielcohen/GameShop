using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public float OrderTotal { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set;}

        public DateTime PaymentDate { get; set; }
        public DateOnly PaymentDueDate { get; set; }

        public string? sessionId {  get; set; }
        public string? PaymentIntentId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public String StreetAddress { get; set; }
        [Required]
        public String City { get; set; }
        [Required]
        public String State { get; set; }
        [Required]
        public String ZipCode { get; set; }
        [Required]
        public String PhoneNumber { get; set; }

    }
}
