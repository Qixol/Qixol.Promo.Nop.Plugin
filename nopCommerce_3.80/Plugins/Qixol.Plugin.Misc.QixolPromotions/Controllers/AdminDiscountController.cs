using Nop.Admin.Controllers;
using Nop.Admin.Models.Discounts;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public class AdminDiscountController : BaseAdminController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public AdminDiscountController(IDiscountService discountService,
            ILocalizationService localizationService, ICurrencyService currencyService,
            ICategoryService categoryService, IProductService productService,
            IWebHelper webHelper, IDateTimeHelper dateTimeHelper,
            ICustomerActivityService customerActivityService, CurrencySettings currencySettings,
            IPermissionService permissionService)
            /*: base(discountService, localizationService, currencyService, categoryService,
                      productService, webHelper, dateTimeHelper, customerActivityService,
                      currencySettings, permissionService)*/
        {
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._currencyService = currencyService;
            this._categoryService = categoryService;
            this._productService = productService;
            this._webHelper = webHelper;
            this._dateTimeHelper = dateTimeHelper;
            this._customerActivityService = customerActivityService;
            this._currencySettings = currencySettings;
            this._permissionService = permissionService;
        }

        #endregion

        public ActionResult Create()
        {
            throw new NotSupportedException("Create");
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(DiscountModel model, bool continueEditing)
        {
            throw new NotSupportedException("Create");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            throw new NotSupportedException("Delete");
        }

        public ActionResult DeleteDiscountRequirement(int discountRequirementId, int discountId)
        {
            throw new NotSupportedException("Delete");
        }

        public ActionResult Edit(int id)
        {
            throw new NotSupportedException("Edit");
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(DiscountModel model, bool continueEditing)
        {
            throw new NotSupportedException("Edit");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDiscountRequirementConfigurationUrl(string systemName, int discountId, int? discountRequirementId)
        {
            throw new NotSupportedException("GetDiscountRequirementConfigurationUrl");
        }

        public ActionResult GetDiscountRequirementMetaInfo(int discountRequirementId, int discountId)
        {
            throw new NotSupportedException("GetDiscountRequirementMetaInfo");
        }

        public ActionResult Index()
        {
            return RedirectToAction("List", "AdminDiscount");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            return View();
        }
    }
}
