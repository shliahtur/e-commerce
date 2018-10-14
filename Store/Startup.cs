
using Microsoft.Owin;
using Owin;
using Store.Controllers;

[assembly: OwinStartup(typeof(Store.Startup))]

namespace Store
{
    public partial class Startup
    {

        string connection = AccountController.GetConnectionString("ApplicationDbContext");

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}


