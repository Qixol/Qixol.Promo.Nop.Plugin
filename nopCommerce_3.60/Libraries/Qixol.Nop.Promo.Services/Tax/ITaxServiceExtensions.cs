using global::Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Tax
{
    public interface ITaxServiceExtensions
    {
        decimal GetCheckoutAttributePrice(global::Nop.Core.Domain.Orders.CheckoutAttributeValue cav, bool includingTax, Customer customer, out decimal taxRate, bool includeDiscounts);
    }
}
