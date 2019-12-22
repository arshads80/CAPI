using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CAPI.Models;
using Encr2014;

namespace CAPI.Controllers
{
    public class BookingsController : ApiController
    {
        //[BasicAuthentication]
        // GET api/bookings
        public List<tblMailEntry> Get()
        {
            using (db_MAILNPPEntities dbContext2 = new db_MAILNPPEntities())
            {
                List<tblMailEntry> mailEntries = dbContext2.tblMailEntries.OrderByDescending(i => i.EntryID).Take(10).ToList();
                return mailEntries;
            }
        }

        //[BasicAuthentication]
        // POST api/bookings
        public HttpResponseMessage Post([FromBody] tblMailEntry mailEntry)
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



            // get AuthCode
            string authCode = "";

            if (headers.Contains("AuthCode"))
            {
                authCode = headers.GetValues("AuthCode").First();
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing AuthCode");
            }

            //Check AuthCode
            var decryptedValue = Encr2014.EncryptionModule.msDecryptTool2014(authCode, "msqJmbr21#");

            if (decryptedValue.Length == 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Authcode");
            }
            if (decryptedValue.Substring(0,3) != "MSQ")
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid OriginId");
            }
            else
            {
                if (string.Compare(decryptedValue.Substring(6), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))<0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Authcode Expired");
                }
            }

            using (db_MAILNPPEntities dbContext = new db_MAILNPPEntities())
            {
                tblMailEntry updatedMailEntry = new tblMailEntry();
                if (FillMailEntry(updatedMailEntry, mailEntry))
                    {
                    dbContext.tblMailEntries.Add(updatedMailEntry);
                    dbContext.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, updatedMailEntry.AWBNo);
                    }
                else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error");
                    }
            }

        }

        private bool FillMailEntry(tblMailEntry mailEntry, tblMailEntry originalMailEntry)
        {
            var recordDateTime = DateTime.Now;
            var entryDateTime = recordDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            mailEntry.EntryDateTime = entryDateTime;
            mailEntry.CustID = 25;
            mailEntry.AWBprefix = "MSQ";
            mailEntry.CustRef = originalMailEntry.CustRef;
            mailEntry.AccountNo = originalMailEntry.AccountNo;
            mailEntry.Name1 = originalMailEntry.Name1;
            mailEntry.Name2 = originalMailEntry.Name2;
            mailEntry.CompanyName = originalMailEntry.CompanyName;
            mailEntry.Address1 = originalMailEntry.Address1;
            mailEntry.Address2 = originalMailEntry.Address2;
            mailEntry.CityCode = "";    // originalMailEntry.CityCode;
            mailEntry.City = originalMailEntry.City;
            mailEntry.Country = originalMailEntry.Country;
            mailEntry.Telephone = originalMailEntry.Telephone;
            mailEntry.Mobile = originalMailEntry.Mobile;
            mailEntry.DeliveryArea = "";    // originalMailEntry.DeliveryArea;
            mailEntry.DeliveryStatus = "Booking";
            mailEntry.LastUpdate = mailEntry.EntryDateTime;
            mailEntry.Activities = recordDateTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " - Booking";
            mailEntry.Remark = originalMailEntry.Remark;
            mailEntry.isUpload = 0;
            mailEntry.AuthName = originalMailEntry.AuthName;
            mailEntry.DocType = "CB";
            mailEntry.NoS = 1;
            mailEntry.BatchNo = 250;
            mailEntry.ActivityStatus = "New Booking";
            mailEntry.Shipper = "Mashreq Bank";
            mailEntry.BookingFlag = 1;

            long nextDocSerial = GetNextDocSerial("MSQ", entryDateTime);
            if (nextDocSerial != 0)
            {
                mailEntry.AWBserial = (int)nextDocSerial;
                mailEntry.AWBNo = "MSQ" + DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture) + ("000000" + mailEntry.AWBserial).Substring(("000000" + mailEntry.AWBserial).Length - 6);
                mailEntry.AWBbarcode = "*" + mailEntry.AWBNo + "*";
                return true;
            }
            else
            {
                return false;
            }

        }

        private long GetNextDocSerial(String docCode, String entryDateTime)
        {
            long nextDocSerial;
            using (var dbContext = new db_MAILNPPEntities())
            {
                tblDocSerial docSerialObject = new tblDocSerial();
                docSerialObject.DocCode = docCode;
                long docSerial = dbContext.tblDocSerials.Where(dso => dso.DocCode == docCode).Max(i => i.DocSerial);
                docSerialObject.DocSerial = docSerial + 1;
                docSerialObject.EntryDateTime = entryDateTime;
                try
                {
                    dbContext.tblDocSerials.Add(docSerialObject);
                    dbContext.SaveChanges();
                    nextDocSerial = docSerial + 1;
                }
                catch
                {
                    nextDocSerial = 0;
                }
                return nextDocSerial;
            }
        }
    }
}
