using System;
using System.Net.Mail;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;




namespace GK.PKIMonitoring.CertWarning
{
    class SendWarning
    {        
        private String srvEMailName; 
        private String strBody;
        private String strInitialBody;
        private String strSubject;
        private String strTo;
        private String strFrom;
        private String strCert;
        private String strReplaceRequesterDomain;
        private String bodyFileName = ""; 
        private String eMailBodyTemplate = "";
        

        public SendWarning()//String iniFilePath)
        {
            // read parameters
            //IniParser parser = new IniParser(iniFilePath);
            //srvEMailName = parser.GetSetting("EMAIL", "srvEMailName");
            //strInitialBody = parser.GetSetting("EMAIL", "strBody");
            //strSubject = parser.GetSetting("EMAIL", "strSubject");
            //strTo = parser.GetSetting("EMAIL", "strTo");
            //strFrom = parser.GetSetting("EMAIL", "strFrom");
            //strReplaceRequesterDomain = parser.GetSetting("EMAIL", "strReplaceRequesterDomain");
            srvEMailName = Consts.WarningMail_Server;
            strInitialBody = Consts.WarningMail_Body;
            strSubject = Consts.WarningMail_Subject;
            strTo = Consts.WarningMail_To;
            strFrom = Consts.WarningMail_From;
            strReplaceRequesterDomain = Consts.WarningMail_ReplaceRequesterDomain;

            // get body file name
            bodyFileName = Consts.WarningMail_BodyFileName;

            //// get body file name
            //bodyFileName = parser.GetSetting("EMAIL", "BODYFILENAME");
            try
            {
                StreamReader templateFile = new StreamReader(bodyFileName);
                eMailBodyTemplate = templateFile.ReadToEnd();
            }
            catch 
            {
                eMailBodyTemplate = "";
            }
        }

        public void SendWarningEMail(Certificate certData, string sTemplateName, string sCAName)
        {
            StreamWriter certFile = new StreamWriter("Certificate.cer");
            strCert = certData.BinaryCertificate;
            strCert.Replace(" REQUEST", "");
            strCert.Replace("New ", "");
            certFile.Write(strCert);
            certFile.Flush();
            certFile.Dispose();
            string tempSubject = "";
            string tempRequester = "";
            string[] strToArray;
            
            //initialize the file so that it can accept updates
            ZipFile z = ZipFile.Create("Certificate.zip");
            
            //initialize the file so that it can accept updates
            z.BeginUpdate();

            //add your file
            z.Add("Certificate.cer");

            //commit the update once we are done
            z.CommitUpdate();

            //close the file
            z.Close();

            // initialize body
            if (string.IsNullOrEmpty(eMailBodyTemplate))
                strBody = strInitialBody;
            else
                strBody = eMailBodyTemplate;

            // open mail message
            System.Net.Mail.MailMessage mMailMessage = new System.Net.Mail.MailMessage();
            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment("Certificate.zip");
            
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
            try
            {
                tempSubject = "CertID " + certData.RequestId + " - " + strSubject + " Name: " + certData.IssuedCommonName;
                mMailMessage.Subject = tempSubject + "; Coria-ID: " + certData.IssuedState;
            }
            catch
            {
                mMailMessage.Subject = strSubject;
            }

            if (string.IsNullOrEmpty(eMailBodyTemplate))
            {

                // set body
                strBody = strBody + " \nCertID: " + certData.RequestId;
                strBody = strBody + " \nValid from: " + certData.CertificateEffectiveDate;
                strBody = strBody + " \nExpiration: " + certData.ExpirationDate;
                strBody = strBody + " \nCommon Name: " + certData.IssuedCommonName;
                if (certData.IssuedState.Contains("# NULL #"))
                    strBody = strBody + " \nCoria: NULL";
                else
                    strBody = strBody + " \nCoria: " + certData.IssuedState;
                certData.RequesterName = certData.RequesterName.ToUpper();
                tempRequester = certData.RequesterName.Replace(strReplaceRequesterDomain.ToUpper(), "");
                strBody = strBody + " \nRequester: " + tempRequester;
                if (certData.GroupName.Equals("") == false)
                    strBody = strBody + " \nGroup name: " + certData.GroupName;
                strBody = strBody + " \nTemplate: " + sTemplateName.Trim();
                strBody = strBody + " \nIssuing CA: " + sCAName;
                strBody = strBody + " \nSubject: " + certData.SubjectCommonName;
            }
            else
            {
                // set body
                strBody = strBody.Replace("##CertID##"," \nCertID: " + certData.RequestId);
                strBody = strBody.Replace("##Valid from##", " \nValid from: " + certData.CertificateEffectiveDate);
                strBody = strBody.Replace("##Expiration##", " \nExpiration: " + certData.ExpirationDate);
                strBody = strBody.Replace("##Common Name##", " \nCommon Name: " + certData.IssuedCommonName);
                if (certData.IssuedState.Contains("# NULL #"))
                    strBody = strBody.Replace("##Coria##", " \nCoria: NULL");
                else
                    strBody = strBody.Replace("##Coria##", " \nCoria: " + certData.IssuedState);
                certData.RequesterName = certData.RequesterName.ToUpper();
                tempRequester = certData.RequesterName.Replace(strReplaceRequesterDomain.ToUpper(), "");
                strBody = strBody.Replace("##Requester##", " \nRequester: " + tempRequester);
                if (certData.GroupName.Equals("") == false)
                    strBody = strBody.Replace("##Group name##", " \nGroup name: " + certData.GroupName);
                else
                    strBody = strBody.Replace("##Group name##"," \nGroup name: -");
                strBody = strBody.Replace("##Template##", " \nTemplate: " + sTemplateName.Trim());
                strBody = strBody.Replace("##Issuing CA##", " \nIssuing CA: " + sCAName);
                strBody = strBody.Replace("##Subject##", " \nSubject: " + certData.SubjectCommonName);
            }

            // set body to message
            mMailMessage.Body = strBody;
            mMailMessage.IsBodyHtml = true;

            // add attachment
            mMailMessage.Attachments.Add(attachment);

            // send eMail
            SmtpClient mSmtpClient = new SmtpClient();
            mSmtpClient.Host = srvEMailName;
            mSmtpClient.Send(mMailMessage);
            mMailMessage.Dispose();
            attachment.Dispose();

            System.IO.File.Delete("Certificate.zip");
            System.IO.File.Delete("Certificate.cer");
        }
    }
}
