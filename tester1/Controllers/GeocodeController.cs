using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tester1.Models;

namespace tester1.Controllers
{
    public class GeocodeController : Controller
    {
        // GET: Geocode
        ThucDonDataContext db = new ThucDonDataContext();
        [HttpPost]
        public ActionResult adress(string MaDH)
        {
            var address = db.DonHangs
                            .Where(o => o.MaDH == MaDH)
                            .Select(o => o.DiaChi + ", " + o.Quan)
                            .FirstOrDefault();
            Session["Address"] = address;

            // Trả về một phản hồi HTTP, có thể là PartialView hoặc Redirect
            return RedirectToAction("Map", "Geocode");
        }
        public async Task<ActionResult> GetCoordinates(string address)
        {
            try
            {
                string apiKey = "AIzaSyBXERVKQsDCFy-fojkLIpkLdWvN76AcSjg"; // Thay YOUR_API_KEY bằng API key của bạn
                string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);

                    // Kiểm tra xem có kết quả nào không
                    if (result.status == "OK")
                    {
                        double latitude = result.results[0].geometry.location.lat;
                        double longitude = result.results[0].geometry.location.lng;

                        // Trả về PartialView chứa dữ liệu tọa độ
                        return PartialView("_CoordinatesPartial", new GeocodeResult { Latitude = latitude, Longitude = longitude });
                    }
                    else
                    {
                        return Json(new { Error = "Không tìm thấy địa chỉ." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }



        public async Task<ActionResult> Map()
        {
            string Address = Session["Address"] as string;
            ViewBag.Address = Address;
            GeocodeController geocodeController = new GeocodeController();
            ActionResult result = await geocodeController.GetCoordinates(Address);

            // Kiểm tra xem kết quả có null không
            if (result != null && result is JsonResult && (result as JsonResult).Data != null)
            {
                // Chuyển kết quả thành đối tượng GeocodeResult
                GeocodeResult geocodeResult = (result as JsonResult).Data as GeocodeResult;

                // Truyền tọa độ vào view
                return View(geocodeResult);
            }
            else
            {
                return View();
            }

        }
    }
}