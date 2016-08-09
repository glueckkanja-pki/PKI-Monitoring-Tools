using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace GK.PKIMonitoring.CheckWebCRL
{
    internal static class Consts
    {
        public static string NssTemppath
        {
            get
            {
                return ConfigurationManager.AppSettings["nss_temppath"];
            }
        }
    }
}
