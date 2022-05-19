using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketForms.Components
{
    public class EmailLimpet
    {
        private EmailSenderData _emailData;
        public EmailLimpet(int portalId, string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetCurrentCulture();
            CultureCode = cultureCode;
            PortalId = portalId;
            SystemData = new SystemLimpet(SystemKey);
            _emailData = new EmailSenderData();
            PortalData = new PortalLimpet(portalId);

            ErrorMessage = "";
        }
        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="templateName"></param>
        /// <param name="subject"></param>
        /// <returns>true if email sent, fail is failed.  [If emails are off, then no email is sent. In that case, if in debug mode we still return true, in not in debug module we return false.]</returns>
        public bool SendEmail(string emailBody, string fromEmail, string toEmail, string replyToEmail, string subject = "")
        {
            _emailData.ToEmail = toEmail;
            _emailData.FromEmail = fromEmail;
            _emailData.ReplyToEmail = replyToEmail;
            _emailData.EmailSubject = subject;
            _emailData.CultureCode = CultureCode;
            _emailData.EmailBody = emailBody;

            var emailSender = new EmailSender(_emailData);
            var emailsent = emailSender.SendEmail();
            ErrorMessage = emailSender.Error;
            return emailsent;
        }

        public string CultureCode { get; private set; }
        public string SystemKey { get { return "rocketforms"; } }
        public int PortalId { get; set; }
        public PortalLimpet PortalData { get; set; }
        public SystemLimpet SystemData { get; set; }
        public string ErrorMessage { get; set; }

    }
}
