﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Models
{
    public partial class IssuedCouponsModel
    {
        private IList<IssuedCouponModel> _issuedCoupons;

        public IList<IssuedCouponModel> Coupons
        {
            get
            {
                return _issuedCoupons ?? (_issuedCoupons = new List<IssuedCouponModel>());
            }
        }

        public bool IsEditable { get; set; }
        public bool ShowSku { get; set; }
        public bool ShowProductImages { get; set; }
    }

    #region nested classes

    public class IssuedCouponModel
    {
        public bool IsConfirmed { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Status { get; set; }

        public DateTime ValidTo { get; set; }

        public string DisplayText { get; set; }
    }

    #endregion
}