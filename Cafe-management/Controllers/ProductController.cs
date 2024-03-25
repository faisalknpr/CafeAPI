using Cafe_management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_management.Controllers
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        CafeEntities db = new CafeEntities();
        [HttpPost, Route("addNewProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage addNewProduct(Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                product.status = "true";
                db.Products.Add(product);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Product added successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, Route("getAllProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage getAllProduct()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, (from Product in db.Products join Category in db.Categories on Product.categoryId equals Category.id select new
                {
                    Product.id,
                    Product.name,
                    Product.description,
                    Product.price,
                    Product.status,
                    CategoryId = Category.id,
                    CategoryName = Category.name,
                }).ToList());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("updateProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage updateProduct(Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product pObj = db.Products.Where(x => x.id == product.id).FirstOrDefault();
                if (pObj == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Product dosent exist");
                }
                pObj.id = product.id;
                pObj.name = product.name;
                pObj.description = product.description;
                pObj.price = product.price;
                pObj.status = product.status;
                pObj.categoryId = product.categoryId;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Product updated");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost, Route("deleteproduct/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage deleteProduct(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product pObj = db.Products.Find(id);
                if (pObj == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Product dosent exist");
                }
                db.Products.Remove(pObj);
                db.SaveChanges();
                return Request.CreateErrorResponse(HttpStatusCode.OK, "Product deleted");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost,Route("updateProductStatus")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateProductStatus(Product product) 
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product pObj = db.Products.Find(product.id);
                if (pObj == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Product dosent exist");
                }
                pObj.status = product.status;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Product status updated");
            } catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}

  
