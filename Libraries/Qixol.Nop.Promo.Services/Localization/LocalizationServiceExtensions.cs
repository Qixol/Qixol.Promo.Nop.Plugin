using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Localization
{
    public static class LocalizationServiceExtensions
    {
        /// <summary>
        ///  Attempt to look up the resourceKey.  If we get back the same text as the resource key, 
        ///  just return the original key.  The reason we do this is because GetResource will convert all to
        ///  lower case...
        /// </summary>
        /// <param name="localizationService"></param>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public static string GetValidatedResource(this ILocalizationService localizationService, string resourceKey)
        {
            string validatedResource = resourceKey;

            if (string.IsNullOrEmpty(resourceKey))
                return validatedResource;

            var workContext = EngineContext.Current.Resolve<IWorkContext>();

            if (workContext.WorkingLanguage != null)
                validatedResource = localizationService.GetResource(resourceKey, workContext.WorkingLanguage.Id, logIfNotFound: false,defaultValue: resourceKey, returnEmptyIfNotFound: false);

            return validatedResource;
        }
    }
}
