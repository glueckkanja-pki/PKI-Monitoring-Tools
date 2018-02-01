using System;
using System.DirectoryServices;

namespace GK.PKIMonitoring.CertWarning
{
    public static class AllTemplates
    {
        public struct TemplateStruct
        {
            public string strOID;
            public string strName;
            public string strDisplayName;
        }

        public static TemplateStruct[] allSubTemplates { get; private set; }

        public static string OID2Name(string sOID)
        {
            string sName;

            for (int i = 0; i < allSubTemplates.Length; i++)
            {
                if (allSubTemplates[i].strOID.Equals(sOID))
                {
                    sName = allSubTemplates[i].strName;
                    return sName;
                }
            }
            return "Error no Template found.";
        }

        public static void GetAllTemplates()
        {
            GetAllTemplates(string.Empty); 
        }

        /// <summary>
        /// search template names and corresponding OIDs
        /// </summary>
        public static void GetAllTemplates(string strDCName)
        {
            string ldapURIPrefix = "LDAP://" + (string.IsNullOrEmpty(strDCName) ? string.Empty : strDCName + "/");

            using (DirectoryEntry rootEntry = new DirectoryEntry(ldapURIPrefix + "rootDSE"))
            using (DirectoryEntry templatesEntry = new DirectoryEntry(ldapURIPrefix + "cn = certificate templates,cn=public key services,cn=services,cn=configuration," + (string)rootEntry.Properties["defaultNamingContext"][0]))
                GetAllTemplates(templatesEntry);
        }

        private static void GetAllTemplates(DirectoryEntry templatesEntry)
        {
            // Variables
            SearchResultCollection resultCollection = null;
            int i = 0;
            int TemplateCount;
            
            // Look for the Display Name of a template OID in AD
            using (DirectorySearcher searcher = new DirectorySearcher(templatesEntry))
            {
                searcher.Filter = "(&(msPKI-Cert-Template-OID=*))";
                resultCollection = searcher.FindAll();

                // create array of template struct
                TemplateCount = resultCollection.Count;
                allSubTemplates = new TemplateStruct[TemplateCount];

                // fill up the array
                foreach (SearchResult result in resultCollection)
                {
                    allSubTemplates[i].strOID = (string)result.Properties["msPKI-Cert-Template-OID"][0];
                    allSubTemplates[i].strName = (string)result.Properties["Name"][0];
                    allSubTemplates[i].strDisplayName = (string)result.Properties["displayName"][0];
                    i = ++i;
                }
            }
        }
    }
}
