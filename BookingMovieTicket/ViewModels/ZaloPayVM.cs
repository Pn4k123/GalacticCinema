using Newtonsoft.Json;

namespace BookingMovieTicket.ViewModels
{
    public class ZaloPayOrderResponse
    {
        [JsonProperty("return_code")]
        public int ReturnCode { get; set; }

        [JsonProperty("return_message")]
        public string ReturnMessage { get; set; }

        [JsonProperty("sub_return_code")]
        public int SubReturnCode { get; set; }

        [JsonProperty("sub_return_message")]
        public string SubReturnMessage { get; set; }

        [JsonProperty("order_url")]
        public string OrderUrl { get; set; }
    }

    public class ZaloPayCallbackResult
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public string EmbedData { get; set; }
    }
}
