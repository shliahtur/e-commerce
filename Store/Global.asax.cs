using Store.Models;
using System.Web.Mvc;
using System.Web.Routing;
using Store.Infrastructure.Binders;
using System.Web.Optimization;
using System.Threading;

namespace Store
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);  
            ModelBinders.Binders.Add(typeof(Cart), new CartModelBinder());
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }
}
