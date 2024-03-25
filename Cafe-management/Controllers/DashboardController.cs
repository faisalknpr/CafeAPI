using Cafe_management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_management.Controllers
{

    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        CafeEntities db = new CafeEntities();


        [HttpGet, Route("details")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage getDetails()
        {
            try
            {
                var data = new
                {
                    category = db.Categories.Count(),
                    product = db.Products.Count(),
                    bill = db.Bills.Count(),
                    User = db.Users.Count()
                };
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
