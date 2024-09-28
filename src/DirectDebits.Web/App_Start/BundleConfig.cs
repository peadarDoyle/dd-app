using System.Web.Optimization;

namespace DirectDebits
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/content/javascript")
                .Include("~/Scripts/*.js")
                .Include("~/Scripts/BatchRecord/*.js")
                .Include("~/Scripts/Common/*.js")
                .Include("~/Scripts/NewBatch/*.js")
                .Include("~/Scripts/Settings/*.js")
                .Include("~/Scripts/Mode/*.js")
                .Include("~/Scripts/Registration/*.js")
                .Include("~/Scripts/Vendor/*.js")
            );

            bundles.Add(new LessBundle("~/content/css1")
                .Include("~/content/styles/Less/*.less"));

            bundles.Add(new StyleBundle("~/content/css2")
                .Include("~/content/styles/Css/*.css", new CssRewriteUrlTransform()));

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif

        }
    }
}
