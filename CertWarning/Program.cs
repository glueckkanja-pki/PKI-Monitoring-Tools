using System;
using System.DirectoryServices;
using log4net;

namespace GK.PKIMonitoring.CertWarning
{
    class Program
    {
        public const int CV_OUT_BASE64 = 0x1;
        public const int CV_OUT_BINARY = 0x2;
        public const int CV_OUT_HEX = 0x4;
        public const int CV_OUT_HEXASCII = 0x5;
        
        public const int PROPTYPE_LONG = 0x1;
        public const int PROPTYPE_DATE = 0x2;
        public const int PROPTYPE_BINARY = 0x3;
        public const int PROPTYPE_STRING = 0x4;
        public const int PROPTYPE_MASK = 0xFF;
        
        public const int CVRC_COLUMN_SCHEMA = 0;
        public const int CVRC_COLUMN_RESULT = 0x1;
        public const int CVRC_COLUMN_VALUE = 0x2;
        public const int CVRC_COLUMN_MASK = 0xfff;

        public const int CVR_SEEK_NONE = 0;
        public const int CVR_SEEK_EQ = 0x1;
        public const int CVR_SEEK_LT = 0x2;
        public const int CVR_SEEK_LE = 0x4;
        public const int CVR_SEEK_GE = 0x8;
        public const int CVR_SEEK_GT = 0x10;
        public const int CVR_SEEK_MASK = 0xff;
        public const int CVR_SEEK_NODELTA = 0x1000;
        
        public const int CVR_SORT_NONE = 0;
        public const int CVR_SORT_ASCEND = 0x1;
        public const int CVR_SORT_DESCEND = 0x2;
//        public const string INIFileName = "certwarning.ini";
        public const string LogFileName = "certwarning.log";
                
        static void Main(string[] args)
        {
            // call log string constructor
//            LogString logString = new LogString(LogFileName);
            ILog log = LogManager.GetLogger("GK.PKIMonitoring.CertWarning");

            try
            {
                // call send warning mail constructor
                SendWarning sendWarning = new SendWarning();

                string templateName;

                int iColumnCount = 0;
                long lastRequestID = 0;
                long largestRequestID = 0;
                long processedRowsCount = 0;
                DateTime dtNow;
                DateTime dtStart;
                DateTime dtEnd;
//                IniParser parser = new IniParser(INIFileName);

                //// get date
                //string dateString = parser.GetSetting("DateTime", "datestring");
                //dateString = dateString.ToLower();

                dtNow = Consts.DateTime_ReferenceDate;

                // get warning threshold
                int warningTimePeriod = Consts.DateTime_WarningTimePeriod;// int.Parse(parser.GetSetting("DateTime", "WarningTimePeriod"));

                // get sliding time size
                int backwardLookupDays = Consts.DateTime_BackwardLookupDays;// int.Parse(parser.GetSetting("DateTime", "backwardLookupDays"));

                //// compute time values
                //if (dateString.Contains("now") == true)
                //{
                //    dtNow = DateTime.Now;
                //}
                //else
                //{
                //    dtNow = DateTime.Parse(dateString);
                //}

                string strDCName = Consts.GlobalSettings_RootDC;// parser.GetSetting("GLOBALSETTINGS", "ROOTDC");
                if (strDCName == null)
                {
                    log.Error("Can't read DC name from ini file.");
                    //PrintString(logString, "Error: Can't read DC name from ini file.");
                    return;
                }
                log.Debug("Root Domain Controller: " + strDCName);
                //PrintString(logString, "Root Domain Controller: " + strDCName);
                DirectoryEntry rootEntry = null;
                DirectoryEntry templatesEntry = null;

                try
                {
                    AllTemplates.GetAllTemplates();
                }
                catch (Exception ex)
                {
                    log.Error("Error enumerating templates", ex);
                    return;
                }

                log.Debug("INI-Date: " + dtNow);
                //PrintString(logString, "INI-Date: " + dtNow);

                //dtNow = DateTime.Now;
                dtEnd = dtNow.AddDays(warningTimePeriod);
                dtStart = dtEnd.AddDays(-backwardLookupDays);

                log.Debug("Start: " + dtStart);
                log.Debug("End: " + dtEnd);
                //PrintString(logString, "Start: " + dtStart);
                //PrintString(logString, "End: " + dtEnd);

                // get ca config string from ini file
                string CAConfigString = Consts.GlobalSettings_CAConfigString;// parser.GetSetting("GlobalSettings", "CAConfigString");
                log.Debug("CAConfigString: " + CAConfigString);
                //PrintString(logString, "CAConfigString: " + CAConfigString);

                // get template names from ini file
                string strConfiguredTemplateNames = Consts.GlobalSettings_TemplateName;// parser.GetSetting("GlobalSettings", "strTemplateName");
                strConfiguredTemplateNames = strConfiguredTemplateNames.ToUpper();
                strConfiguredTemplateNames = " " + strConfiguredTemplateNames + " ";
                strConfiguredTemplateNames = strConfiguredTemplateNames.Replace(",", " , ");
                log.Debug("strTemplatename: " + strConfiguredTemplateNames);
                //PrintString(logString, "strConfiguredTemplateNames: " + strConfiguredTemplateNames);

                // get send mail setting
                bool activateWarningEMail = Consts.WarningMail_Activate;// bool.Parse(parser.GetSetting("GLOBALSETTINGS", "activateWarningEMail"));
                log.Debug("Activate warning eMail: " + activateWarningEMail.ToString());
                //PrintString(logString, "Activate warning eMail: " + activateWarningEMail.ToString());

                // last request ID
                // string strLastRequestID = parser.GetSetting("GlobalSettings", "strLastRequestID");
                lastRequestID = Consts.GlobalSettings_LastRequestId; //long.Parse(strLastRequestID);
                log.Debug("strLastRequestID: " + lastRequestID.ToString());
                //PrintString(logString, "strLastRequestID: " + lastRequestID.ToString());// strLastRequestID);

                // Get user mapping settings
                string MappingFileName = null;
                bool activateUserMapping = Consts.GroupMapping_Activate;// bool.Parse(parser.GetSetting("GROUPMAPPING", "ACTIVATEUSERMAPPING"));
                if (activateUserMapping == true)
                    MappingFileName = Consts.GroupMapping_Filename;// parser.GetSetting("GROUPMAPPING", "FILENAME");

                // initiate group mapping class
                User2Group user2Group = new User2Group();

                // open interface
                CERTADMINLib.CCertView CView = new CERTADMINLib.CCertView();

                // open connection
                CView.OpenConnection(CAConfigString);

                //restrict view
                CView.SetRestriction(CView.GetColumnIndex(CVRC_COLUMN_SCHEMA, "NotAfter"), CVR_SEEK_GT, CVR_SORT_ASCEND, dtStart);

                // column count
                iColumnCount = CView.GetColumnCount(0);
                CView.SetResultColumnCount(iColumnCount);

                // Place each column in the view.
                for (int x = 0; x < iColumnCount; x++)
                {
                    CView.SetResultColumn(x);
                }

                //open the View
                CERTADMINLib.IEnumCERTVIEWROW rowsEnum;
                rowsEnum = CView.OpenView();

                //reset row position
                CERTADMINLib.IEnumCERTVIEWCOLUMN objCol;
                rowsEnum.Reset();

                //Enumerate Rows                                                        
                while (rowsEnum.Next() != -1)
                {
                    Certificate cert = new Certificate();
                    objCol = rowsEnum.EnumCertViewColumn();

                    //Enumerate Columns                             
                    while (objCol.Next() != -1)
                    {
                        //PrintString(logString,  objCol.GetDisplayName());
                        //PrintString(logString, objCol.GetName());
                        switch (objCol.GetDisplayName())
                        {
                            case "Binary Certificate":
                                try
                                {
                                    cert.BinaryCertificate = objCol.GetValue(PROPTYPE_BINARY).ToString();
                                    //PrintString(logString, "Request Disp. Message: " + cert.DispMessage);
                                }
                                catch
                                {
                                    cert.DispMessage = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Request Disposition Message":
                                try
                                {
                                    cert.DispMessage = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "Request Disp. Message: " + cert.DispMessage);
                                }
                                catch
                                {
                                    cert.DispMessage = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Certificate Expiration Date":
                                try
                                {
                                    cert.ExpirationDate = objCol.GetValue(PROPTYPE_DATE).ToString();
                                    //PrintString(logString, "Expiration Date: " + cert.ExpirationDate);
                                }
                                catch
                                {
                                    cert.ExpirationDate = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Issued Distinguished Name":
                                try
                                {
                                    cert.SubjectCommonName = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "DN: " + cert.SubjectCommonName);
                                }
                                catch
                                {
                                    cert.SubjectCommonName = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Certificate Template":
                                try
                                {
                                    cert.CertificateTemplate = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "Certificate Template: " + cert.CertificateTemplate);
                                }
                                catch
                                {
                                    cert.CertificateTemplate = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Effective Revocation Date":
                                try
                                {
                                    cert.EffectiveRevocationDate = objCol.GetValue(PROPTYPE_DATE).ToString();

                                }
                                catch
                                {
                                    cert.EffectiveRevocationDate = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Revocation Reason":
                                try
                                {
                                    cert.RevocationReason = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "Revocation Reason: " + cert.RevocationReason);
                                }
                                catch
                                {
                                    cert.RevocationReason = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Request ID":
                                try
                                {
                                    cert.RequestId = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "RequestID:" + cert.RequestId);
                                }
                                catch
                                {
                                    cert.RequestId = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Requester Name":
                                try
                                {
                                    cert.RequesterName = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "Requester Name: " + cert.RequesterName);
                                }
                                catch
                                {
                                    cert.RequesterName = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Issued State":
                                try
                                {
                                    cert.IssuedState = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "Issued State: " + cert.IssuedState);
                                }
                                catch
                                {
                                    cert.IssuedState = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Issued Common Name":
                                try
                                {
                                    cert.IssuedCommonName = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "Issued Common Name: " + cert.IssuedCommonName);
                                }
                                catch
                                {
                                    cert.IssuedCommonName = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            case "Certificate Effective Date":
                                try
                                {
                                    cert.CertificateEffectiveDate = objCol.GetValue(PROPTYPE_STRING).ToString();
                                    //PrintString(logString, "Effective Date: " + cert.CertificateEffectiveDate);
                                }
                                catch
                                {
                                    cert.CertificateEffectiveDate = "# NULL #";
                                    //Exception Handling
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    // set to empty string
                    templateName = "";

                    // is revoked?
                    if (cert.DispMessage.Contains("Issued"))
                    {
                        if (DateTime.Parse(cert.ExpirationDate) <= dtEnd)
                        {
                            if (lastRequestID < long.Parse(cert.RequestId))
                            {
                                if (cert.CertificateTemplate.Contains("1.3.6.1.4.1"))
                                {
                                    templateName = AllTemplates.OID2Name(cert.CertificateTemplate);
                                }
                                else
                                {
                                    templateName = cert.CertificateTemplate;
                                }
                                templateName = " " + templateName + " ";
                                if (strConfiguredTemplateNames.Contains(templateName.ToUpper()))
                                {
                                    //PrintString(logString, "");
                                    //PrintString(logString, "RequestID:" + cert.RequestId);
                                    //PrintString(logString, "Request Disp. Message: " + cert.DispMessage);
                                    //PrintString(logString, "Expiration Date: " + cert.ExpirationDate);
                                    //PrintString(logString, "Subject: " + cert.SubjectCommonName);
                                    //PrintString(logString, "Certificate Template: " + templateName.Trim());
                                    //PrintString(logString, "Requester Name: " + cert.RequesterName);

                                    //log.Debug("RequestID:" + cert.RequestId);
                                    //log.Debug("Request Disp. Message: " + cert.DispMessage);
                                    //log.Debug("Expiration Date: " + cert.ExpirationDate);
                                    //log.Debug("Subject: " + cert.SubjectCommonName);
                                    //log.Debug("Certificate Template: " + templateName.Trim());
                                    //log.Debug("Requester Name: " + cert.RequesterName);

                                    // user mapping on or off
                                    if (activateUserMapping)
                                    { 
                                        string groupName = user2Group.GetMapping(MappingFileName, cert.RequesterName);//, INIFileName);
                                        if (!groupName.Equals(""))
                                        {
                                            cert.GroupName = groupName;
                                        }
                                    }
                                    
                                    //PrintString(logString, "Issued State: " + cert.IssuedState);
                                    //PrintString(logString, "Issued Common Name: " + cert.IssuedCommonName);
                                    //PrintString(logString, "Effective Date: " + cert.CertificateEffectiveDate);
                                    //PrintString(logString, "Issuing CA: " + CAConfigString);
                                    //log.Debug("Issued State: " + cert.IssuedState);
                                    //log.Debug("Issued Common Name: " + cert.IssuedCommonName);
                                    //log.Debug("Effective Date: " + cert.CertificateEffectiveDate);
                                    //log.Debug("Issuing CA: " + CAConfigString);

                                    log.Warn(cert);

                                    // send warning email if activated
                                    if (activateWarningEMail)
                                        sendWarning.SendWarningEMail(cert, templateName, CAConfigString);
                                    
                                    // save largest request id to ini file
                                    if (long.Parse(cert.RequestId) > largestRequestID)
                                    {
                                        Consts.GlobalSettings_LastRequestId = Convert.ToInt32(cert.RequestId);
                                        //parser.AddSetting("GlobalSettings", "strLastRequestID", cert.RequestId);
                                        //parser.SaveSettings();
                                        largestRequestID = long.Parse(cert.RequestId);
                                    }
                                    processedRowsCount = ++processedRowsCount;
                                }
                            }
                        }
                    }
                }

                log.Info( "- Summary -\n" +
                            "Processed certificate rows: " + processedRowsCount.ToString());
                if (largestRequestID != 0)
                    log.Debug("Last processed request ID saved to ini file: " + largestRequestID.ToString());

                // Clean up
                if (rootEntry != null)
                {
                    rootEntry.Dispose();
                }
                if (templatesEntry != null)
                {
                    templatesEntry.Dispose();
                }
                //Console.ReadKey();
            }
            catch (Exception excep)
            {
                log.Fatal(excep);
                throw;
            }
            finally
            {
                try
                {
                    // call send warning mail constructor
                    SendSummary sendSummary = new SendSummary();
                    sendSummary.SendReportEMail();
                }
                catch (Exception excep)
                {
                    log.Fatal(excep);
                    throw;
                }
            }
        }
    }
}
