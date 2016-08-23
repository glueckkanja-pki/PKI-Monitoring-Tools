using System;
using System.Net.Mail;
using System.Diagnostics;
using log4net;

namespace GK.PKIMonitoring.CertWarning
{
 
    class SendSummary
    {
        private String srvEMailName;
        private String strBody;
        private String strSubject;
        private String strTo;
        private String strFrom;
        private bool activateReportEMail;

        static ILog log = LogManager.GetLogger("GK.PKIMonitoring.CertWarning.Summary");

        // write to eventlog
        private static void WriteEventLog(string sEvent)
        {
            string sSource;
            string sLog;

            sSource = "CertWarning";
            sLog = "Application";

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);
            EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 1);
        }

        public  SendSummary()//String iniFilePath)
        {
            try
            {
                // read parameters
                //IniParser parser = new IniParser(iniFilePath);
                //srvEMailName = parser.GetSetting("SendReport", "SRVEMAILNAMEREPORT");
                //strBody = parser.GetSetting("SendReport", "STRBODYREPORT");
                //strSubject = parser.GetSetting("SendReport", "STRSUBJECTREPORT");
                //strTo = parser.GetSetting("SendReport", "STRTOREPORT");
                //strFrom = parser.GetSetting("SendReport", "STRFROMREPORT");
                //activateReportEMail = bool.Parse(parser.GetSetting("SendReport", "ACTIVATEREPORTEMAIL"));
                srvEMailName = Consts.SendReport_Server;
                strBody = Consts.SendReport_Body;
                strSubject = Consts.SendReport_Subject;
                strTo = Consts.SendReport_To;
                strFrom = Consts.SendReport_From;
                activateReportEMail = Consts.SendReport_Activate;
            }
            catch (Exception excep)
            {
                log.Fatal("Error sending summary mail", excep);
                //Exception Handling
                WriteEventLog(excep.ToString());
                Console.Write(excep.ToString());
            }
        }

        public void SendReportEMail()
        {
            try
            {
                if (activateReportEMail == false) return;

                string[] strToArray;

                // open mail message
                System.Net.Mail.MailMessage mMailMessage = new System.Net.Mail.MailMessage();
                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment("Certwarning.log");

                // Set the sender address of the mail message
                mMailMessage.From = new MailAddress(strFrom);

                // Set the recepient address of the mail message
                if (strTo.Contains(",") == true)
                {
                    strToArray = strTo.Split(',');
                    foreach (string entryTo in strToArray)
                    {
                        mMailMessage.To.Add(new MailAddress(entryTo));
                    }

                }
                else
                {
                    mMailMessage.To.Add(new MailAddress(strTo));
                }

                // set subject
                mMailMessage.Subject = strSubject;

                // set body
                mMailMessage.Body = strBody;

                // add attachment
                mMailMessage.Attachments.Add(attachment);

                // send eMail
                SmtpClient mSmtpClient = new SmtpClient();
                mSmtpClient.Host = srvEMailName;
                mSmtpClient.Send(mMailMessage);
                mMailMessage.Dispose();
                attachment.Dispose();
            }
            catch (Exception excep1)
            {
                log.Fatal("Error sending summary mail", excep1);
                //Exception Handling
                WriteEventLog(excep1.ToString());
                Console.Write(excep1.ToString());
            }
        }
    }
}
