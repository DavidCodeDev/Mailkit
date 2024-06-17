using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MailKit.Net.Smtp;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using System.Runtime.Intrinsics.X86;
using Org.BouncyCastle.Crypto;
using System.CodeDom.Compiler;
using System.Numerics;
using System.Security.Policy;
using System.Windows.Media.Media3D;
using System.Text.RegularExpressions;

namespace ControlMat
{
    public class EmailManager
    {
        public static string gmailAppPassword = "abcde";
        public static string gmailAccount = "correo@gmail.com";


        public static void ReadInbox()
        {
            imapClient = new ImapClient("imap.gmail.com", gmailAccount, gmailAppPassword, AuthMethods.Login, 993, true);
            AE.Net.Mail.Imap.Mailbox[] mailBoxes = imapClient.ListMailboxes(string.Empty, "*");
            imapClient.SelectMailbox("[Gmail]/Enviados");
            var cont = imapClient.GetMessageCount();
            var email = imapClient.GetMessage(cont - 1);
            var asunto = email.Subject;
            var texto = email.Body;
        }

        public static void Send() 
        {
            // create email message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("tu alias", "correo@gmail.com"));

            email.To.Add(MailboxAddress.Parse("ayuntamiento@algodonales.es"));
            email.Subject = "Test Email Subject";
            email.Body = new TextPart(TextFormat.Plain) { Text = "Example Plain Text Message Body" };

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(gmailAccount, gmailAppPassword);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public static async Task SendAsync()
        {
            await Task.Run(() => Send());
        }
       
       
        public static IEnumerable<string> GetUnreadMails(PlateManager plateManager)
        {
            var messages = new List<string>();
            try
            {
                using (var client = new ImapClient())
                {
                    client.Connect("imap.gmail.com", 993, true);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(gmailAccount, gmailAppPassword);

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    //inbox.Open(FolderAccess.ReadOnly);
                    inbox.Open(FolderAccess.ReadWrite);
                    var results = inbox.Search(SearchOptions.All, SearchQuery.Not(SearchQuery.Seen));
                    foreach (var uniqueId in results.UniqueIds)
                    {
                        var message = inbox.GetMessage(uniqueId);
                        string from = ExtractEmail(message.From[0].ToString());
                                                                                              
                        string subject = message.Subject;
                        messages.Add(message.Subject);

                        //Mark message as read
                        //inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
                    }
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return messages;
        }
        /// <summary>
        /// Devuelve el email que se encuentra entre los caracteres "<" y ">"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ExtractEmail(string input)
        {
            string pattern = @"<([^>]*)>";

            Match match = Regex.Match(input, pattern);

            if (match.Success)
                return match.Groups[1].Value;
            else
                return string.Empty;
        }

        
    }
}
