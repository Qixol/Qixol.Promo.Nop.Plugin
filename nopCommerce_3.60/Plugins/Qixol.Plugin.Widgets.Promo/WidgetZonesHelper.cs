using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo
{
    public class WidgetZonesHelper
    {        

        internal static List<WidgetZoneItem> GetWidgetZonesForProductPromos()
        {
            return GetAllWidgets().Where(z => z.ValidForProductPromos).ToList();
        }

        internal static List<WidgetZoneItem> GetWidgetZonesForBanners()
        {
            return GetAllWidgets().Where(z => z.ValidForPromoBanner).ToList();
        }

        internal static List<WidgetZoneItem> GetAllWidgets()
        {

            var widgetZonesList = new List<WidgetZoneItem>();

            widgetZonesList.Add(new WidgetZoneItem() { Name = "account_navigation_after", ValidForPromoBanner = false });        // Used to show coupons issued in the User's account area.
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productbox_addinfo_after", ValidForPromoBanner = false });        // Used when overlaying stickers in the catalogue.

            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_add_info", ValidForProductPromos = true, DefaultForProductPromos = true });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_after_pictures", ValidForProductPromos = false });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_overview_top", ValidForProductPromos = true });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_inside_overview_buttons_before", ValidForProductPromos = false });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_inside_overview_buttons_after", ValidForProductPromos = false });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_overview_bottom", ValidForProductPromos = true });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_before_collateral", ValidForProductPromos = true });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_bottom", ValidForProductPromos = true });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productbreadcrumb_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productbreadcrumb_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_after_breadcrumb" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productdetails_before_pictures" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productsbytag_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productsbytag_before_product_list" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productsbytag_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productsearch_page_basic" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "productsearch_page_advanced" });

            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_after_breadcrumb" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_before_subcategories" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_before_featured_products" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_after_featured_products" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_before_filters" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_before_product_list" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "categorydetails_bottom" });

            widgetZonesList.Add(new WidgetZoneItem() { Name = "manufacturerdetails_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "manufacturerdetails_before_featured_products" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "manufacturerdetails_after_featured_products" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "manufacturerdetails_before_filters" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "manufacturerdetails_before_product_list" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "manufacturerdetails_bottom" });

            widgetZonesList.Add(new WidgetZoneItem() { Name = "header_menu_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "header_menu_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "mob_header_menu_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "mob_header_menu_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "footer" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "header_links_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "header_links_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "home_page_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "home_page_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "account_navigation_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "account_navigation_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "profile_page_info_userdetails" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "profile_page_info_userstats" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "body_start_html_tag_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "content_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "content_after" });

            widgetZonesList.Add(new WidgetZoneItem() { Name = "left_side_column_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "left_side_column_after_category_navigation" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "left_side_column_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "right_side_column_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "right_side_column_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "main_column_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "main_column_after" });

            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_billing_address_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_billing_address_middle" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_billing_address_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_progress_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_progress_after" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_completed_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_completed_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_confirm_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_confirm_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_payment_info_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_payment_info_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_payment_method_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_payment_method_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_shipping_address_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_shipping_address_middle" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_shipping_address_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_shipping_method_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "checkout_shipping_method_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_billing_address_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_billing_address_middle" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_billing_address_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_confirm_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_confirm_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_payment_info_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_payment_info_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_payment_method_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_payment_method_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_shipping_address_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_shipping_address_middle" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_shipping_address_bottom" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_shipping_method_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "op_checkout_shipping_method_bottom" });

            widgetZonesList.Add(new WidgetZoneItem() { Name = "orderdetails_page_top" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "orderdetails_page_overview" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "orderdetails_page_beforeproducts" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "orderdetails_page_afterproducts" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "orderdetails_page_bottom" });

            widgetZonesList.Add(new WidgetZoneItem() { Name = "order_summary_content_before" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "order_summary_cart_footer" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "order_summary_content_deals" });
            widgetZonesList.Add(new WidgetZoneItem() { Name = "order_summary_content_after" });

            return widgetZonesList;
        }

    }

    internal class WidgetZoneItem
    {
        public WidgetZoneItem()
        {
            ValidForPromoBanner = true;     // Default.
        }

        private string _displayName = string.Empty;

        public string Name { get; set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                    return this.Name;
                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }

        public bool ValidForProductPromos { get; set; }

        public bool ValidForPromoBanner { get; set; }

        public bool DefaultForProductPromos { get; set; }
    }
}
