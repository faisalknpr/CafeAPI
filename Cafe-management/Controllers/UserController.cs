using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Windows.Forms;
using Cafe_management.Models;
using Microsoft.Identity.Client;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;

namespace Cafe_management.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

        [HttpPost, Route("signup")]
        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u=>u.email == user.email).FirstOrDefault();
                if (userObj == null)
                {
                    user.role = "user"; user.status = "false";
                    db.Users.Add(user);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, new {message = "Successfully Registered"});

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "Email already exist"});
                }
            }
            catch(Exception ex)
            {
               return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

        }
        [HttpPost, Route("login")]
        public HttpResponseMessage Login([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u => u.email == user.email && u.password == user.password).FirstOrDefault();
                if (userObj != null)
                {
                    if (userObj.status == "true")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new {token = TokenManager.GenerateToken(userObj.email, userObj.role)});
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "Wait for Admin approval");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Incorrect Username or password");
                }
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, Route("checkToken")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage checkToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { message = "true" });
        }

        [HttpGet, Route("getAllUser")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllUser()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin") 
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "You are not admin" });
                }
                var result = db.Users.Select(u => new { u.id, u.name, u.contactNumber, u.email, u.status, u.role }).Where(x => x.role == "user").ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost, Route("updateuserstatus")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateUserStatus(User user) 
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if (claim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "You are not authorize to make this change" });
                }
                User userObj = db.Users.Find(user.id);
                if (userObj == null)
                {
                    response.Message = "User id does not exist";
                    return Request.CreateResponse(HttpStatusCode.NotFound, response);
                }
                userObj.status = user.status;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, new { message = "User status updated" });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost, Route("changepassword")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage ChangePassword(ChangePassword changePassword)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);

                User userObj = db.Users.Where(u=>u.email == claim.Email && u.password == changePassword.OldPassword).FirstOrDefault();
                if(userObj != null)
                {
                    userObj.password = changePassword.NewPassword;
                    db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Password successfully changed");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Incorrect old password");
                }
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        private string createemailBody(string email, string password) 
        {
            try
            {
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("/Template/forget-password.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{email}", email);
                body = body.Replace("{password}", password);
                body = body.Replace("{frontendUrl}", "http://localhost:4200/");
                return body;
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [HttpPost, Route("forgetpassword")]
        public async Task<HttpResponseMessage> ForgetPassword([FromBody] User user)
        {
            User userObj = db.Users.Where(x => x.email == user.email).FirstOrDefault();
            if(userObj == null) 
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Password sucessfully sent to your email");
            }
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.email));
            message.Subject = "Cafe-management login credentials";
            message.Body = createemailBody(userObj.email, userObj.password);
            message.IsBodyHtml = true;
            using(var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
                await Task.FromResult(0);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Password sucessfully sent to your email");
        }   
    }
}
