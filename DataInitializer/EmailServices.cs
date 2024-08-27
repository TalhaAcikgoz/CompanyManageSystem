
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;



public class EmailService
{
    private readonly string _apiKey;
    private readonly string _secretKey;
    private readonly string _senderEmail;
    
    public EmailService(IConfiguration configuration)
    {
        _apiKey = "57fdd363e025f1590a4e76b27ed797ae";
        _secretKey = "4545f38e4fcca852846bd57a081634db";
        _senderEmail = "onur.tabuk1@gmail.com";
        Console.WriteLine("EmailService: " + _apiKey + " " + _secretKey + " " + _senderEmail);
        Console.WriteLine("EmailService: " + configuration["MAILJET_API_KEY"] + " " + configuration["MAILJET_SECRET_KEY"] + " " + configuration["MAILJET_SENDER_EMAIL"]);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
{
    MailjetClient client = new MailjetClient(_apiKey, _secretKey);
    Console.WriteLine("EmailService: " + toEmail + " " + subject + " " + body);
    Console.WriteLine("apisend: " + _apiKey + " " + _secretKey + " " + _senderEmail);

    MailjetRequest request = new MailjetRequest
    {
        Resource = SendV31.Resource,
    }
    .Property(Send.Messages, new JArray {
        new JObject {
            { "From", new JObject {
                 { "Email", _senderEmail }, // Gönderici e-posta adresi
                 { "Name", "Your Name" }
             }},
            { "To", new JArray {
                 new JObject {
                     { "Email", toEmail }, // Alıcı e-posta adresi
                     { "Name", "Recipient Name" }
                 }
             }},
            { "Subject", subject },
            { "TextPart", body }
        }
    });

    var response = await client.PostAsync(request);
    var responseContent = response.Content; // Yanıt içeriğini al

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Email sent successfully");
    }
    else
    {
        Console.WriteLine("Email failed with status: " + response.StatusCode);
        Console.WriteLine("Response content: " + responseContent); // Yanıt içeriğini yazdır
    }
    }
}
