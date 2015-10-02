//using global::Nop.Core.Domain.Customers;
//using global::Nop.Services.Common;
//using System;
//using System.Linq;
//using Qixol.Nop.Promo.Core.Domain.Promo;
//using System.Collections.Generic;

//namespace Qixol.Nop.Promo.Core.Domain.Customers
//{
//    public static class CustomerExtensions
//    {
//        public static IList<string> IssuedCouponCodes(this Customer customer)
//        {
//            string issuedCouponsCsv = customer.GetAttribute<string>(PromoCustomerAttributeNames.PromoIssuedCoupons);

//            if (!string.IsNullOrEmpty(issuedCouponsCsv))
//            {
//                return issuedCouponsCsv.Split(',').ToList();
//            }

//            return new List<string>();
//        }
//    }
//}
