using Nop.Core.Infrastructure;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core
{
    public class TypeConverterRegistrationStartUpTask : IStartupTask
    {
        public void Execute()
        {
            TypeDescriptor.AddAttributes(typeof(BasketResponse), new TypeConverterAttribute(typeof(BasketResponseTypeConverter)));
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
