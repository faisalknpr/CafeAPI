using Cafe_management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_management.Controllers
{
    [RoutePrefix("api/categories")]
    public class CategoryController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

        [HttpPost, Route("addNewCategory")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage addNewCategory([FromBody] Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                db.Categories.Add(category);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Category added");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpGet,Route("getAllCategory")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage getAllCategory()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Categories.ToList());
            }
            catch (Exception ex) 
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost, Route("updateCategory")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage updateCategory([FromBody] Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Category catObj = db.Categories.Find(category.id);
                if(catObj == null) 
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Category not found");
                }
                catObj.name = category.name;
                db.Entry(catObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Category update sucessfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex);
            }
        }

    }
}
