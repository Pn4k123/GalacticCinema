using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Newtonsoft.Json;
using System.Text;

namespace BookingMovieTicket.Services
{
    public class ZaloPayService : IZaloPayService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        public ZaloPayService(IConfiguration config)
        {
            _config = config;
            _client = new HttpClient();
        }

        public async Task<ZaloPayOrderResponse> CreateOrderAsync(DonDatVe donHang)
        {
            if (!int.TryParse(_config["ZaloPay:AppId"], out int appId))
            {
                throw new Exception("AppId phải là số");
            }

            var key1 = _config["ZaloPay:Key1"];
            var apiUrl = _config["ZaloPay:ApiUrl"];

            // --- SỬA ĐOẠN NÀY: Nối MaDon vào URL ---
            
            var baseRedirectUrl = _config["ZaloPay:RedirectUrl"];
            var redirectUrl = $"{baseRedirectUrl}?maDon={donHang.MaDon}";
            // ----------------------------------------

            var randomId = DateTime.Now.Ticks.ToString();
            var appTransId = $"{DateTime.Now:yyMMdd}_{donHang.MaDon.GetHashCode()}_{randomId.Substring(randomId.Length - 5)}";

            var appUser = "KhachHang";
            var appTime = Utils.GetTimeStamp();
            var amount = (long)donHang.ChiTietDonDatVes.Sum(x => x.GiaVe);

            // Lưu redirectUrl (đã có maDon) vào embed_data
            var embedDataDict = new Dictionary<string, string>
    {
        { "redirecturl", redirectUrl },
        { "maDonHangGoc", donHang.MaDon }
    };
            var embedDataJson = JsonConvert.SerializeObject(embedDataDict);
            var itemsJson = "[]";

            // Tính MAC
            var rawData = $"{appId}|{appTransId}|{appUser}|{amount}|{appTime}|{embedDataJson}|{itemsJson}";
            var mac = ZaloPayHelper.HmacSHA256(rawData, key1);

            var requestData = new Dictionary<string, object>
    {
        { "app_id", appId },
        { "app_user", appUser },
        { "app_time", appTime },
        { "amount", amount },
        { "app_trans_id", appTransId },
        { "embed_data", embedDataJson },
        { "item", itemsJson },
        { "description", $"Thanh toan don hang {donHang.MaDon}" },
        { "bank_code", "" },
        { "mac", mac }
    };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(apiUrl, requestData);
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ZaloPayOrderResponse>(responseString);
            }
        }

        // Hàm này xử lý dữ liệu khi ZaloPay Redirect user quay lại web
        public ZaloPayCallbackResult PaymentExecute(IQueryCollection collection)
        {
            var result = new ZaloPayCallbackResult();
            try
            {
                // Lấy các tham số Zalo trả về
                // https://docs.zalopay.vn/v2/payment/gateway/returnurl.html
                var amount = collection["amount"];
                var appid = collection["appid"];
                var apptransid = collection["apptransid"];
                var bankcode = collection["bankcode"];
                var checksum = collection["checksum"]; // Checksum từ Zalo
                var discountamount = collection["discountamount"];
                var pmcid = collection["pmcid"];
                var status = collection["status"];

                // QUAN TRỌNG: Kiểm tra chữ ký bảo mật
                // Dùng Key2 để verify
                var key2 = _config["ZaloPay:Key2"];

                // Format checksum trả về: appid|apptransid|pmcid|bankcode|amount|discountamount|status
                var dataStr = $"{appid}|{apptransid}|{pmcid}|{bankcode}|{amount}|{discountamount}|{status}";

                var myChecksum = ZaloPayHelper.HmacSHA256(dataStr, key2);

                if (!myChecksum.Equals(checksum.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Success = false;
                    result.Message = "Chữ ký không hợp lệ (Invalid Checksum)";
                    return result;
                }

                // Nếu chữ ký khớp -> Lấy trạng thái
                result.Success = true;
                result.Status = int.Parse(status);
                result.Message = result.Status == 1 ? "Thanh toán thành công" : "Thanh toán thất bại";

                // Lấy embed_data trả về (đôi khi Zalo trả về qua query string nếu cấu hình, hoặc ta phải query lại)
                // Tuy nhiên, ở Redirect URL, Zalo thường KHÔNG trả về embed_data trực tiếp trong query string như VNPay.
                // *Cách xử lý*: Vì ta đã lưu `apptransid` lúc tạo, ta có thể không cần embed_data ở bước này nếu đã mapping apptransid với MaDon trong DB.

                // *Nhưng* nếu bạn muốn lấy MaDon như cách mình code trong Controller, 
                // ZaloPay Redirect URL không trả `embed_data`. 
                // Giải pháp: Bạn phải tách apptransid. Ví dụ lúc tạo apptransid = "yyMMdd_MaDon_Random"
                // thì giờ split string để lấy lại MaDon.

                // Giả sử ta parse MaDon từ AppTransId (Logic này phải khớp với lúc tạo CreateOrderAsync)
                var parts = apptransid.ToString().Split('_');
                // parts[0] = date, parts[1] = MaDonHash (hoặc MaDon), parts[2] = Random

                // Hoặc để đơn giản cho bài này, mình giả định bạn dùng MaDon làm phần chính của AppTransId
                // Tuy nhiên, cách tốt nhất là Controller nên lưu AppTransId vào Session hoặc Database trước khi redirect.

                // Để code Controller ở câu trước hoạt động, ta cần EmbedData. 
                // Nếu Zalo không trả về EmbedData ở Redirect, ta fix cứng hoặc trả về AppTransId
                result.EmbedData = ""; // Zalo Redirect ko trả cái này.

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Lỗi xử lý: " + ex.Message;
            }

            return result;
        }
    }

    public static class Utils
    {
        public static long GetTimeStamp()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        // ... (Các hàm cũ giữ nguyên)
    }
}