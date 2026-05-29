using System.Net;
using System.Net.Mail;
using System.Net.Mime; // <-- QUAN TRỌNG: Thêm thư viện này
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace BookingMovieTicket.Helper
{
    public class EmailHelper
    {
        private readonly IConfiguration _config;

        public EmailHelper(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateQrCode(string content)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    return Convert.ToBase64String(qrCodeImage);
                }
            }
        }

        public async Task SendTicketEmail(string toEmail, string subject, string bodyHtml, string qrCodeBase64)
        {
            var mailSettings = _config.GetSection("EmailSettings");

            var fromAddress = new MailAddress(mailSettings["Mail"], mailSettings["DisplayName"]);
            var toAddress = new MailAddress(toEmail);
            string fromPassword = mailSettings["Password"];

            var smtp = new SmtpClient
            {
                Host = mailSettings["Host"],
                Port = int.Parse(mailSettings["Port"]),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress))
            {
                message.Subject = subject;

                // 1. Tạo AlternateView để chứa nội dung HTML + Hình ảnh nhúng
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(bodyHtml, null, MediaTypeNames.Text.Html);

                // 2. Xử lý hình ảnh QR Code
                if (!string.IsNullOrEmpty(qrCodeBase64))
                {
                    byte[] imageBytes = Convert.FromBase64String(qrCodeBase64);
                    MemoryStream ms = new MemoryStream(imageBytes);

                    LinkedResource qrResource = new LinkedResource(ms, "image/png");

                    qrResource.ContentId = "qrcode";
                    qrResource.TransferEncoding = TransferEncoding.Base64;

                    // Thêm hình vào View
                    htmlView.LinkedResources.Add(qrResource);
                }

                message.AlternateViews.Add(htmlView);
                message.IsBodyHtml = true;

                await smtp.SendMailAsync(message);
            }
        }
    }
}