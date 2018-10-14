using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Owin.Security.Twitter;
using Store.Models;
using Owin;
using System;
using System.Configuration;

namespace Store
{
    public partial class Startup
    {
       
        public void ConfigureAuth(IAppBuilder app)
        {
         
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
         
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

           
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            #region Microsoft

            var microsoftAuthenticationOptions = new MicrosoftAccountAuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["MsI"],
                ClientSecret = ConfigurationManager.AppSettings["MsS"],
                Provider = new MicrosoftAccountAuthenticationProvider()
                {
                    OnAuthenticated = async context =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("MicrosoftAccessToken", context.AccessToken));
                        foreach (var claim in context.User)
                        {
                            var claimType = string.Format("urn:microsoft:{0}", claim.Key);
                            string claimValue = claim.Value.ToString();
                            if (!context.Identity.HasClaim(claimType, claimValue))
                                context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, "XmlSchemaString", "Microsoft"));
                        }
                    }
                }
            };
            microsoftAuthenticationOptions.Scope.Add("wl.basic");
            microsoftAuthenticationOptions.Scope.Add("wl.emails");
            microsoftAuthenticationOptions.Scope.Add("wl.birthday");
            app.UseMicrosoftAccountAuthentication(microsoftAuthenticationOptions);

            #endregion Microsoft

            #region Twitter
            var twitterAuthenticationOptions = new TwitterAuthenticationOptions()
            {
                ConsumerKey = ConfigurationManager.AppSettings["TwtI"],
                ConsumerSecret = ConfigurationManager.AppSettings["TwtS"],
                BackchannelCertificateValidator = new CertificateSubjectKeyIdentifierValidator(
                new[]
                {
                    "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
                "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
                "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority - G5
                "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA - G4
                "4eb6d578499b1ccf5f581ead56be3d9b6744a5e5", // VeriSign Class 3 Primary CA - G5
                "5168FF90AF0207753CCCD9656462A212B859723B", // DigiCert SHA2 High Assurance Server C‎A 
                "B13EC36903F8BF4701D498261A0802EF63642BC3" // DigiCert High Assurance EV Root CA
                })
            };
            app.UseTwitterAuthentication(twitterAuthenticationOptions);

            #endregion Twitter

            #region Facebook
            var facebookAuthenticationOptions = new FacebookAuthenticationOptions()
            {
                AppId = ConfigurationManager.AppSettings["FaceI"],
                AppSecret = ConfigurationManager.AppSettings["FaceS"],

                Provider = new FacebookAuthenticationProvider()
                {
                    OnAuthenticated = async context =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookAccessToken", context.AccessToken));
                    }
                }
            };
            facebookAuthenticationOptions.Scope.Add("public_profile");
            facebookAuthenticationOptions.Scope.Add("email");
            facebookAuthenticationOptions.Scope.Add("user_birthday");
            app.UseFacebookAuthentication(facebookAuthenticationOptions);


            #endregion Facebook

            #region Google

            var googleAuthenticationOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["GglI"],
                ClientSecret = ConfigurationManager.AppSettings["GglS"],
                Provider = new GoogleOAuth2AuthenticationProvider()
                {
                    OnAuthenticated = async context =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("GoogleAccessToken", context.AccessToken));
                        foreach (var claim in context.User)
                        {
                            var claimType = string.Format("urn:google:{0}", claim.Key);
                            string claimValue = claim.Value.ToString();
                            if (!context.Identity.HasClaim(claimType, claimValue))
                                context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, "XmlSchemaString", "Google"));
                        }
                    }
                }
            };
            googleAuthenticationOptions.Scope.Add("https://www.googleapis.com/auth/plus.login email");
            app.UseGoogleAuthentication(googleAuthenticationOptions);

            #endregion Google


        }
    }
}
