using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string statusPending = "Pending";
        public const string statusApproved = "Approved";
        public const string statusProcessing = "Processing";
        public const string statusShipped = "Shipped";
        public const string statusCancelled = "Cancelled";
        public const string statusRefunded = "Refunded";
        public const string statusCompleted = "Completed";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";
    }
}
