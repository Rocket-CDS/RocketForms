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
        private SimplisityRazor _model;
        private EmailSenderData _emailData;
        public EmailLimpet(int portalId, SimplisityRazor model, string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetCurrentCulture();
            CultureCode = cultureCode;
            PortalId = portalId;
            SystemData = new SystemLimpet(SystemKey);
            _model = model;
            _emailData = new EmailSenderData();
            PortalData = new PortalLimpet(portalId);

            ErrorMessage = "";
        }
        public EmailLimpet(int portalId, object modelObject, string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetCurrentCulture();
            CultureCode = cultureCode;
            PortalId = portalId;
            SystemData = new SystemLimpet(SystemKey);
            _model = new SimplisityRazor(modelObject);
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
        public bool SendEmail(AppThemeLimpet appTheme, string fromEmail, string toEmail, string replyToEmail, string templateName, string subject = "")
        {
            _emailData.ToEmail = toEmail;
            _emailData.FromEmail = fromEmail;
            _emailData.ReplyToEmail = replyToEmail;
            _emailData.EmailSubject = "Espace4: " + subject;
            _emailData.RazorTemplateName = templateName;
            _emailData.Model = _model;
            _emailData.Model.SetDataObject("Espace4x", PortalData);
            _emailData.CultureCode = CultureCode;
            _emailData.AppTheme = appTheme;

            var emailSender = new EmailSender(_emailData);

            if (PortalData.DebugMode)
            {
                LogUtils.OutputDebugFile(SystemKey + "_" + _emailData.RazorTemplateName + "_Email.html", emailSender.RenderEmailBody(), PortalId);
            }

            if (RocketSystemPortal.EmailActive)
            {
                var emailsent = emailSender.SendEmail();
                ErrorMessage = emailSender.Error;
                return emailsent;
            }
            else
            {
                ErrorMessage = "EmailActive is FALSE";
                return false;
            }
        }
        /// <summary>
        /// Send eamil as a list
        /// If sent, the return "Message" will be empty for the "emailKey",
        /// </summary>
        /// <param name="emailList"></param>
        /// <param name="templateName"></param>
        /// <param name="subject"></param>
        /// <returns> emailKey, Message</returns>
        public Dictionary<string,string> SendEmailList(List<string> emailList, string templateName, string subject = "")
        {
            var rtn = new Dictionary<string, string>();
            foreach (var e in emailList)
            {
                if (GeneralUtils.IsEmail(e))
                {
                    var b = SendEmail(e, templateName, subject);
                    if (b)
                        rtn.Add(e, "");
                    else
                        rtn.Add(e, ErrorMessage);
                }
            }
            return rtn;
        }
        public bool SendEmailCSV(string emailCSV, string templateName, string subject = "")
        {
            var l = emailCSV.Split(',');
            var rtn = new Dictionary<string, string>();
            foreach (var e in l)
            {
                if (GeneralUtils.IsEmail(e))
                {
                    var b = SendEmail(e, templateName, subject);
                    if (b)
                        rtn.Add(e, "");
                    else
                        rtn.Add(e, ErrorMessage);
                }
            }

            var erMsg = "";
            var rtnbool = true;
            foreach (var er in rtn)
            {
                if (er.Value != "") rtnbool = false;
                erMsg += er.Key + ": " + er.Value + " | ";
            }
            if (!rtnbool)ErrorMessage = erMsg;
            return rtnbool;
        }

        public string CultureCode { get; private set; }
        public string SystemKey { get { return "rocketforms"; } }
        public int PortalId { get; set; }
        public PortalLimpet PortalData { get; set; }
        public SystemLimpet SystemData { get; set; }
        public string ErrorMessage { get; set; }

    }
}
