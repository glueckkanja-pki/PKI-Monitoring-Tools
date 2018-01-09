using System;
using System.Net.Mail;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;




namespace GK.PKIMonitoring.CertWarning
{
    class SendWarning
    {        
        private String srvEMailName; 
        private String strInitialBody;
        private String strSubject;
        private String strTo;
        private String strFrom;
        private String strReplaceRequesterDomain;
        private String eMailBodyTemplate;
        

        public SendWarning()
        {
            // read parameters
            srvEMailName = Consts.WarningMail_Server;
            strInitialBody = Consts.WarningMail_Body;
            strSubject = Consts.WarningMail_Subject;
            strTo = Consts.WarningMail_To;
            strFrom = Consts.WarningMail_From;
            strReplaceRequesterDomain = Consts.WarningMail_ReplaceRequesterDomain;

            // get body file name
            string bodyFileName = Consts.WarningMail_BodyFileName;
            if (!string.IsNullOrWhiteSpace(bodyFileName) && File.Exists(bodyFileName))
                eMailBodyTemplate = File.ReadAllText(bodyFileName);
        }

        public void SendWarningEMail(Certificate certData, string sTemplateName, string sCAName)
        {
            try
            {
                using (StreamWriter certFile = new StreamWriter("Certificate.cer"))
                {
                    string strCert = certData.BinaryCertificate;
                    strCert.Replace(" REQUEST", "");
                    strCert.Replace("New ", "");
                    certFile.Write(strCert);
                    certFile.Flush();
                }

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

                // open mail message
                using (System.Net.Mail.MailMessage mMailMessage = new System.Net.Mail.MailMessage())
                using (System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment("Certificate.zip"))
                {

                    // Set the sender address of the mail message
                    mMailMessage.From = new MailAddress(strFrom);

                    // Set the recipient address of the mail message
                    foreach (string entryTo in strTo.Split(','))
                        mMailMessage.To.Add(new MailAddress(entryTo));

                    // set subject
                    try
                    {
                        string tempSubject = "CertID " + certData.RequestId + " - " + strSubject + " Name: " + certData.IssuedCommonName;
                        mMailMessage.Subject = tempSubject + "; Coria-ID: " + certData.IssuedState;
                    }
                    catch
                    {
                        mMailMessage.Subject = strSubject;
                    }

                    // initialize body
                    string strBody;
                    if (string.IsNullOrEmpty(eMailBodyTemplate))
                        strBody = strInitialBody + "##CertID####Valid from####Expiration####Common Name####Coria##" +
                            "##Requester####Group Name####Template####Issuing CA####Subject##";
                    else
                        strBody = eMailBodyTemplate;

                    // replace tags in body
                    strBody = strBody.Replace("##CertID##", " \nCertID: " + certData.RequestId);
                    strBody = strBody.Replace("##Valid from##", " \nValid from: " + certData.CertificateEffectiveDate);
                    strBody = strBody.Replace("##Expiration##", " \nExpiration: " + certData.ExpirationDate);
                    strBody = strBody.Replace("##Common Name##", " \nCommon Name: " + certData.IssuedCommonName);
                    if (certData.IssuedState.Contains("# NULL #"))
                        strBody = strBody.Replace("##Coria##", " \nCoria: NULL");
                    else
                        strBody = strBody.Replace("##Coria##", " \nCoria: " + certData.IssuedState);
                    certData.RequesterName = certData.RequesterName.ToUpper();
                    string tempRequester = certData.RequesterName.Replace(strReplaceRequesterDomain.ToUpper(), "");
                    strBody = strBody.Replace("##Requester##", " \nRequester: " + tempRequester);
                    if (string.IsNullOrEmpty(certData.GroupName))
                        strBody = strBody.Replace("##Group name##", " \nGroup name: -");
                    else
                        strBody = strBody.Replace("##Group name##", " \nGroup name: " + certData.GroupName);
                    strBody = strBody.Replace("##Template##", " \nTemplate: " + sTemplateName.Trim());
                    strBody = strBody.Replace("##Issuing CA##", " \nIssuing CA: " + sCAName);
                    strBody = strBody.Replace("##Subject##", " \nSubject: " + certData.SubjectCommonName);

                    // set body to message
                    mMailMessage.Body = strBody;
                    mMailMessage.IsBodyHtml = !string.IsNullOrEmpty(eMailBodyTemplate);

                    // add attachment
                    mMailMessage.Attachments.Add(attachment);

                    // send eMail
                    SmtpClient mSmtpClient = new SmtpClient();
                    mSmtpClient.Host = srvEMailName;
                    mSmtpClient.Send(mMailMessage);
                }
            }
            finally
            {
                if (File.Exists("Certificate.zip"))
                    File.Delete("Certificate.zip");
                if (File.Exists("Certificate.cer"))
                    File.Delete("Certificate.cer");
            }
        }
    }
}
