using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace RetinaNetworking.Server
{
    public class TemplateEmailWithAttachment : SerializedMonoBehaviour
    {
        public string sender = "info@kouo.io";
        private string password = "<3JIie!}v8w\\,";
        [TextArea(1, 3)] public string emailSubject;
        [TextArea(1, 3)] public string previewText;

        public TextAsset body;
        public TextAsset htmlTemplate;
        public ConnectionParams connectionParams;

        public void SendEmail(List<string> attachmentPaths)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(sender);
            mail.To.Add(connectionParams.Username());
            mail.Subject = InsertElement("Name", connectionParams.Name(), emailSubject);

            string htmlBody;
            htmlBody = InsertElement("Body", body.ToString(), htmlTemplate.ToString());
            htmlBody = InsertElement("Name", connectionParams.Name(), htmlBody);
            htmlBody = InsertElement("Preview", previewText, htmlBody);
            htmlBody = InsertElement("Subject", mail.Subject, htmlBody);
            htmlBody = InsertElement("ModalMood", connectionParams.GetModalMood(), htmlBody);

            mail.IsBodyHtml = true;
            mail.Body = htmlBody;

            for (int i = 0; i < attachmentPaths.Count; i++)
            {
                Attachment attachment = new Attachment(attachmentPaths[i]);
                mail.Attachments.Add(attachment);
            }

            SmtpClient server = new SmtpClient("smtp.gmail.com");
            server.Port = 587;
            server.Credentials = new NetworkCredential(sender, password) as ICredentialsByHost;
            server.EnableSsl = true;
            ServicePointManager.ServerCertificateValidationCallback = 
                delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
                { return true; };

            server.Send(mail);

            Wenzil.Console.Console.Log($"SENT MAIL. from: {mail.From} - to: {mail.To} \n subject: {mail.Subject} - attachments: {mail.Attachments}");
        }

        string InsertName(string name, string body)
        {
            string output = body.Replace("[name]", name);
            return output;
        }

        string InsertElement(string key, string element, string body)
        {
            string output = body.Replace($"[{key}]", element);
            return output;
        }
    }
}
