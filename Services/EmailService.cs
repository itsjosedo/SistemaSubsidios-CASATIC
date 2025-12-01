using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace SistemaSubsidios_CASATIC.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreo(string para, string asunto, string mensaje)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Sistema Subsidios", emailSettings["Email"]));
            mimeMessage.To.Add(new MailboxAddress("", para));
            mimeMessage.Subject = asunto;

            mimeMessage.Body = new TextPart("html")
            {
                Text = mensaje
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailSettings["Host"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailSettings["Email"], emailSettings["AppPassword"]);
            await smtp.SendAsync(mimeMessage);
            await smtp.DisconnectAsync(true);
        }
    }

}
