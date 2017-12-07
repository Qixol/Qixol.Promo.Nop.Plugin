using global::Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Qixol.Promo.Integration.Lib.Basket;
using System.IO;

namespace Qixol.Nop.Promo.Core.Domain.Promo
{
    public class BasketResponseTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                BasketResponse basketResponse = null;
                var valueStr = value as string;
                if (!string.IsNullOrEmpty(valueStr))
                {
                    try
                    {
                        using (var tr = new StringReader(valueStr))
                        {
                            basketResponse = BasketResponse.FromXml(valueStr);
                        }
                    }
                    catch
                    {
                        //xml error
                    }
                }
                return basketResponse;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var basketResponse = value as BasketResponse;
                if (basketResponse != null)
                {
                    return basketResponse.ToXml();
                }

                return "";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
