using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Promo
{
    public static class SettingsExtensions
    {

        public static ImportServiceManager GetImportService(this PromoSettings settings)
        {
            switch (settings.ServiceEndpointSelection)
            {
                case SettingsEndpointAddress.LIVE_SERVICES:
                    return new ImportServiceManager(ServiceTarget.LiveServices);

                case SettingsEndpointAddress.EVALUATION_SERVICES:
                    return new ImportServiceManager(ServiceTarget.EvaluationServices);

                case SettingsEndpointAddress.CUSTOM_SERVICES:
                    return new ImportServiceManager(settings.PromoImportEndpointAddress);
                default:
                    break;
            }

            return null;
        }

        public static BasketServiceManager GetBasketService(this PromoSettings settings)
        {
            switch (settings.ServiceEndpointSelection)
            {
                case SettingsEndpointAddress.LIVE_SERVICES:
                    return new BasketServiceManager(ServiceTarget.LiveServices);

                case SettingsEndpointAddress.EVALUATION_SERVICES:
                    return new BasketServiceManager(ServiceTarget.EvaluationServices);

                case SettingsEndpointAddress.CUSTOM_SERVICES:
                    return new BasketServiceManager(settings.BasketRequestEndpointAddress);
                default:
                    break;
            }

            return null;
        }

        public static ExportServiceManager GetExportService(this PromoSettings settings)
        {
            switch (settings.ServiceEndpointSelection)
            {
                case SettingsEndpointAddress.LIVE_SERVICES:
                    return new ExportServiceManager(ServiceTarget.LiveServices);

                case SettingsEndpointAddress.EVALUATION_SERVICES:
                    return new ExportServiceManager(ServiceTarget.EvaluationServices);

                case SettingsEndpointAddress.CUSTOM_SERVICES:
                    return new ExportServiceManager(settings.PromoExportEndpointAddress);
                default:
                    break;
            }

            return null;
        }
    }

    public class SettingsEndpointAddress
    {
        public const int EVALUATION_SERVICES = 0;
        public const int LIVE_SERVICES = 1;
        public const int CUSTOM_SERVICES = 2;
    }
}
