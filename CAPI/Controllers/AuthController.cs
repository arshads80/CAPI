using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Encr2014;

namespace CAPI.Controllers
{
    public class AuthController : ApiController
    {
        //[BasicAuthentication]
        // GET api/Auth
        public HttpResponseMessage Get()
        {

            // get OriginId
            string originId = "";
            var req = Request;
            var headers = req.Headers;

            if (headers.Contains("OriginId"))
            {
                originId = headers.GetValues("OriginId").First();
                if (originId != "MSQ")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid OriginId");
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing OriginId");
            }


            // get ApiKey
            string apiKey = "";

            if (headers.Contains("ApiKey"))
            {
                apiKey = headers.GetValues("ApiKey").First();
                if (apiKey != "$GFfsjkjekjad{2+-slisd^3")
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid ApiKey");
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing ApiKey");
            }


            string dt;
            dt = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            string encryptedToken = EncryptionModule.msEncryptTool2014("MSQ~~~" + dt, "msqJmbr21#");
            return Request.CreateResponse(HttpStatusCode.OK,encryptedToken);

        }
    }
}
