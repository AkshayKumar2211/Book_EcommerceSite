using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Project.Utility
{
    public class SD
    {
        //Roles
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "EmployeeUser";
        public const string Role_Company = "CompanyUser";
        public const string Role_Individual = "IndividualUser";

        //Session
        public const string Ss_CartSessionCount = "cartCountSesion";


        public static double GetPriceBasedOnQuantity(double Quantity, double Price, double Price50, double Price100)
        {
            if (Quantity < 50)
                return Price;
            else if (Quantity < 100)
                return Price50;
            
            return Price100;
        }

        //OrderStatus
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProgress = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStatusRefunded = "Refunded";

        //Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayPayment = "PaymentStatusDelay";
        public const string PaymentStatusRejected = "Rejected";
    }
}
