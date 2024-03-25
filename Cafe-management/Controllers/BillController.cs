using Cafe_management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Web;
using System.IO;
using System.Net.Http.Headers;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cafe_management.Controllers
{
    [RoutePrefix("api/bill")]
    public class BillController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();
        private string pdfPath = "C:\\Users\\faisa\\OneDrive\\Documents\\";

        [HttpPost, Route("generateReport")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GenerateReport(Bill bill)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);

                var ticks = DateTime.Now.Ticks;
                var guid = Guid.NewGuid().ToString();
                var uniquId = ticks.ToString()+"-"+guid;
                bill.createdBy = claim.Email;
                bill.uuid = uniquId;
                db.Bills.Add(bill);
                db.SaveChanges();
                Get(bill);
                return Request.CreateResponse(HttpStatusCode.OK, new {uuid=bill.uuid});
            }
            catch(Exception ex) 
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private void Get(Bill bill) 
        {
            try
            {
                dynamic productDetails = JsonConvert.DeserializeObject(bill.productDetails);
                var todayDate = "Date:"+DateTime.Today.ToString("MM/dd/yyy");
                PdfWriter writer = new PdfWriter(pdfPath+bill.uuid+".pdf");
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                //Header
                Paragraph header = new Paragraph(" Cafe Management System").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(25);
                document.Add(header);

                //New line
                Paragraph newline = new Paragraph(new Text("\n"));
                document.Add(newline);

                //line seperator
                LineSeparator ls = new LineSeparator(new SolidLine());
                document.Add(ls);
                //Customer Details
                Paragraph customerDetails = new Paragraph("Name: " + bill.name + "\nEmail: " + bill.email + "\nContact Number: " + bill.contactNumber + "\nPayment Method: " + bill.paymentMethod);
                document.Add(customerDetails);

                //table

                Table table = new Table(5,false);
                table.SetWidth(new UnitValue(UnitValue.PERCENT, 100));

                //Header
                Cell headername = new Cell(1,1).SetTextAlignment(TextAlignment.CENTER).SetBold().Add(new Paragraph("Name"));

                Cell headercategory = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).SetBold().Add(new Paragraph("Category"));

                Cell headerquantity = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).SetBold().Add(new Paragraph("Quantity"));

                Cell headerprice = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).SetBold().Add(new Paragraph("Price"));

                Cell headersubtotal = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).SetBold().Add(new Paragraph("Sub Total"));


                table.AddCell(headername);
                table.AddCell(headercategory);
                table.AddCell(headerquantity);
                table.AddCell(headerprice);
                table.AddCell(headersubtotal);

                foreach (JObject product in productDetails)
                {
                    Cell nameCell = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(product["name"].ToString()));
                    Cell categoryCell = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(product["category"].ToString()));
                    Cell quantityCell = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(product["quantity"].ToString()));
                    Cell priceCell = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(product["price"].ToString()));
                    Cell totalCell = new Cell(1, 1).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(product["total"].ToString()));
                    table.AddCell(nameCell);
                    table.AddCell(categoryCell);
                    table.AddCell(quantityCell);
                    table.AddCell(priceCell);
                    table.AddCell(totalCell);
                }
                document.Add(table);
                Paragraph last = new Paragraph("Total: " + bill.totalAmount + "\nThanks for visiting, please come again!");
                document.Add(last);
                document.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

          
            [HttpPost, Route("getpdf")]
            [CustomAuthenticationFilter]
            public HttpResponseMessage getPdf(Bill bill)
            {
                try
                {
                    if (bill.name != null)
                    {
                        Get(bill);
                    }
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    var filepath = pdfPath + bill.uuid.ToString() + ".pdf";
                    byte[] data = File.ReadAllBytes(filepath);
                    response.Content = new ByteArrayContent(data);
                    response.Content.Headers.ContentLength = data.LongLength;
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = bill.uuid.ToString() + ".pdf";
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(bill.uuid.ToString() + ".pdf"));
                    return response;
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        [HttpGet, Route("getbills")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage getBills()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim claim = TokenManager.ValidateToken(token);
                if(claim.Role != "admin")
                {
                    var userResult = db.Bills.Where(x => x.createdBy == claim.Email).AsEnumerable().Reverse();
                    return Request.CreateResponse(HttpStatusCode.OK, userResult);
                }
                var adminResult = db.Bills.AsEnumerable().Reverse();
                return Request.CreateResponse(HttpStatusCode.OK, adminResult);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("deleteBill/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage deteteBill(int id) 
        {
            try
            {
                Bill billObj = db.Bills.Where(x=>x.id == id).FirstOrDefault();
                if(billObj == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Bill not found");
                }
                db.Bills.Remove(billObj);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Bill deleted");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}