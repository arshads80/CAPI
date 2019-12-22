using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CAPI.Controllers
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization != null)
            {
                try
                {
                    var authToken = actionContext.Request.Headers
                    .Authorization.Parameter;

                    // decoding authToken we get decode value in 'Username:Password' format  
                    var decodeauthToken = System.Text.Encoding.UTF8.GetString(
                        Convert.FromBase64String(authToken));

                    // spliting decodeauthToken using ':'   
                    var arrUserNameandPassword = decodeauthToken.Split(':');

                    // at 0th postion of array we get username and at 1st we get password  
                    if (IsAuthorizedUser(arrUserNameandPassword[0], arrUserNameandPassword[1]))
                    {
                        // setting current principle  
                        Thread.CurrentPrincipal = new GenericPrincipal(
                        new GenericIdentity(arrUserNameandPassword[0]), null);
                    }
                    else
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                    }
                }
                catch (Exception ex)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
                    
                }
                
            }
            else
            {
                actionContext.Response = actionContext.Request
                 .CreateResponse(HttpStatusCode.Unauthorized);
            }
        }

        private bool IsAuthorizedUser(string Username, string Password)
        {
            return Username == "msqbooking1" && Password == "ghfgfgf78678fgeuyuuietrtt@yrv&55bnbn";
        }
    }
}