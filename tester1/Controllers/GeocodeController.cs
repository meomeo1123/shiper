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
        public async Task<ActionResult> GetCoordinates(string address)
        {
            try
            {
                string apiKey = "AIzaSyDp8VBJ1CIihPbR-iTwR2q7jEkS03BXyOU"; // Thay YOUR_API_KEY bằng API key của bạn
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
            GeocodeController geocodeController = new GeocodeController();
            ActionResult result = await geocodeController.GetCoordinates("HCM Quận 1");

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
                // Xử lý khi kết quả là null
                // Ví dụ: Trả về một view hiển thị thông báo lỗi
                return View("Error");
            }
        }
    }
}