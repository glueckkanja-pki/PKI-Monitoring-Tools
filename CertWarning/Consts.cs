using System;
using System.Configuration;

// configure log4net with app.config
[assembly: log4net.Config.XmlConfigurator]

namespace GK.PKIMonitoring.CertWarning
{
    public static class Consts
    {
        private static string GlobalSettings_LastRequestIdFile
        {
            get
            {
                return ConfigurationManager.AppSettings["GlobalSettings.LastRequestIdFile"];
            }
        }

        private static int _GlobalSettings_LastRequestId = int.MinValue;
        public static int GlobalSettings_LastRequestId
        {
            get
            {
                if (int.MinValue == _GlobalSettings_LastRequestId)
                    if (System.IO.File.Exists(GlobalSettings_LastRequestIdFile))
                        _GlobalSettings_LastRequestId = Convert.ToInt32(System.IO.File.ReadAllText(GlobalSettings_LastRequestIdFile));
                    else
                        _GlobalSettings_LastRequestId = 0;

                return _GlobalSettings_LastRequestId;
            }
            set
            {
                _GlobalSettings_LastRequestId = value;
                System.IO.File.WriteAllText(GlobalSettings_LastRequestIdFile, value.ToString());
            }
        }

        public static string GlobalSettings_TemplateName
        {
            get
            {
                return ConfigurationManager.AppSettings["GlobalSettings.TemplateName"];
            }
        }

        public static string GlobalSettings_RootDC
        {
            get
            {
                return ConfigurationManager.AppSettings["GlobalSettings.RootDC"];
            }
        }

        public static string GlobalSettings_CAConfigString
        {
            get
            {
                return ConfigurationManager.AppSettings["GlobalSettings.CAConfigString"];
            }
        }

        public static bool SendReport_Activate
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["SendReport.Activate"]);
            }
        }

        public static string SendReport_Server
        {
            get
            {
                return ConfigurationManager.AppSettings["SendReport.Server"];
            }
        }

        public static string SendReport_From
        {
            get
            {
                return ConfigurationManager.AppSettings["SendReport.From"];
            }
        }

        public static string SendReport_To
        {
            get
            {
                return ConfigurationManager.AppSettings["SendReport.To"];
            }
        }

        public static string SendReport_Subject
        {
            get
            {
                return ConfigurationManager.AppSettings["SendReport.Subject"];
            }
        }

        public static string SendReport_Body
        {
            get
            {
                return ConfigurationManager.AppSettings["SendReport.Body"];
            }
        }

        public static DateTime DateTime_ReferenceDate
        {
            get
            {
                string sDate = ConfigurationManager.AppSettings["DateTime.ReferenceDate"];
                if (sDate.ToLower() == "now")
                    return DateTime.UtcNow;
                else
                    return DateTime.Parse(sDate);
            }
        }

        public static int DateTime_WarningTimePeriod
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["DateTime.WarningTimePeriod"]);
            }
        }

        public static int DateTime_BackwardLookupDays
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["DateTime.BackwardLookupDays"]);
            }
        }

        public static bool WarningMail_Activate
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["WarningMail.Activate"]);
            }
        }

        public static string WarningMail_Server
        {
            get
            {
                return ConfigurationManager.AppSettings["WarningMail.Server"];
            }
        }

        public static string WarningMail_From
        {
            get
            {
                return ConfigurationManager.AppSettings["WarningMail.From"];
            }
        }

        public static string WarningMail_To
        {
            get
            {
                return ConfigurationManager.AppSettings["WarningMail.To"];
            }
        }

        public static string WarningMail_Subject
        {
            get
            {
                return ConfigurationManager.AppSettings["WarningMail.Subject"];
            }
        }

        public static string WarningMail_Body
        {
            get
            {
                return ConfigurationManager.AppSettings["WarningMail.Body"];
            }
        }

        public static string WarningMail_BodyFileName
        {
            get
            {
                return ConfigurationManager.AppSettings["WarningMail.BodyFileName"];
            }
        }

        public static string WarningMail_ReplaceRequesterDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["WarningMail.ReplaceRequesterDomain"];
            }
        }

        public static bool GroupMapping_Activate
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["GroupMapping.Activate"]);
            }
        }

        public static string GroupMapping_Filename
        {
            get
            {
                return ConfigurationManager.AppSettings["GroupMapping.Filename"];
            }
        }
    }
}
