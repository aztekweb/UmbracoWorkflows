using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Data;


namespace AztekWeb
{
    public class SendEncodedEmail : WorkflowType
    {
        [Setting("Attachment", description = "Attach file uploads to email", view = "Checkbox")]
        public string Attachment
        {
            get;
            set;
        }

        [Setting("Email", description = "Enter the receiver email", view = "TextField")]
        public string Email
        {
            get;
            set;
        }

        [Setting("Message", description = "Enter the intro message", view = "TextArea")]
        public string Message
        {
            get;
            set;
        }

        [Setting("SenderEmail", description = "Enter the sender email (if blank it will use the settings from /config/umbracosettings.config)", view = "TextField")]
        public string SenderEmail
        {
            get;
            set;
        }

        [Setting("Subject", description = "Enter the subject", view = "TextField")]
        public string Subject
        {
            get;
            set;
        }

        public SendEncodedEmail()
        {
            base.Id = new Guid("405da629-606a-4b68-80c7-5b117a609373");
            base.Name = "Send Encoded Email";
            base.Description = "Html encodes supplied form data and sends email.  This prevents users from injecting renderable markup or scipt in emails.";
            base.Icon = "icon-autofill";
            base.Group = "Legacy";
        }

        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            WorkflowExecutionStatus workflowExecutionStatu;

            try
            {
                XPathNavigator xPathNavigator = record.ToXml(new XmlDocument()).CreateNavigator();
                XPathExpression xPathExpression = xPathNavigator.Compile("//fields/child::*");
                xPathExpression.AddSort("@pageindex", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
                xPathExpression.AddSort("@fieldsetindex", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
                xPathExpression.AddSort("@sortorder", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
                XPathNodeIterator xPathNodeIterators = xPathNavigator.Select(xPathExpression);
                string str = "<dl>";
                MailMessage mailMessage = new MailMessage();
                try
                {
                    while (xPathNodeIterators.MoveNext())
                    {
                        XPathNavigator xPathNavigator1 = xPathNodeIterators.Current.SelectSingleNode("caption");
                        if (xPathNavigator1 != null)
                        {
                            str = string.Concat(str, string.Format("<dt><strong>{0}</strong><dt><dd>", DictionaryHelper.GetText(xPathNavigator1.Value)));
                        }
                        XPathNodeIterator xPathNodeIterators1 = xPathNodeIterators.Current.Select(".//value");
                        while (xPathNodeIterators1.MoveNext())
                        {
                            string str1 = WebUtility.HtmlEncode((xPathNodeIterators1.Current.Value.Trim()));
                            str = string.Concat(str, DictionaryHelper.GetText(str1).Replace("\n", "<br/>"), "<br/>");
                            if ((this.Attachment != true.ToString() ? false : str1.Contains("/forms/upload")))
                            {
                                string str2 = HttpContext.Current.Server.MapPath(str1);
                                mailMessage.Attachments.Add(new Attachment(str2));
                            }
                        }
                        str = string.Concat(str, "</dd>");
                    }
                    str = string.Concat(str, "</dl>");
                    mailMessage.Body = string.Concat("<p>", this.Message.Replace("\n", "<br/>"), "</p>", str);
                    mailMessage.From = new MailAddress((string.IsNullOrEmpty(this.SenderEmail) ? UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress : this.SenderEmail));
                    mailMessage.Subject = this.Subject;
                    mailMessage.IsBodyHtml = true;
                    string[] strArrays = this.Email.Split(new char[] { ';' });
                    for (int i = 0; i < (int)strArrays.Length; i++)
                    {
                        string str3 = strArrays[i];
                        mailMessage.To.Add(str3.Trim());
                    }
                    SmtpClient smtpClient = new SmtpClient();
                    try
                    {
                        smtpClient.Send(mailMessage);
                    }
                    finally
                    {
                        if (smtpClient != null)
                        {
                            ((IDisposable)smtpClient).Dispose();
                        }
                    }
                    workflowExecutionStatu = WorkflowExecutionStatus.Completed;
                }
                finally
                {
                    if (mailMessage != null)
                    {
                        ((IDisposable)mailMessage).Dispose();
                    }
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                object[] email = new object[] { this.Email, e.Form.Name, e.Form.Id, record.UniqueId };
                LogHelper.Error<SendEncodedEmail>(string.Format("There was a problem sending an email to '{0}' from Workflow for Form '{1}' with id '{2}' for Record with unique id '{3}'", email), exception);
                workflowExecutionStatu = WorkflowExecutionStatus.Failed;
            }
            return workflowExecutionStatu;
        }

        public override List<Exception> ValidateSettings()
        {
            List<Exception> exceptions = new List<Exception>();
            if (string.IsNullOrEmpty(this.Email))
            {
                exceptions.Add(new Exception("'Email' setting has not been set"));
            }
            if (string.IsNullOrEmpty(this.Message))
            {
                exceptions.Add(new Exception("'Message' setting has not been set'"));
            }
            return exceptions;
        }
    }
}