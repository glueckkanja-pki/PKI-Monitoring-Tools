using System;
using System.DirectoryServices;

namespace GK.PKIMonitoring.CertWarning
{
    public class AllTemplates
    {
        private struct TemplateStruct
        {
            public string strOID;
            public string strName;
            public string strDisplayName;
        }

        private TemplateStruct[] allSubTemplates;

        public string OID2Name(string sOID)
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

        // search template name to OID
        public bool GetAllTemplates(DirectoryEntry templatesEntry)
        {
            // Variables
            DirectorySearcher searcher = null;
            SearchResultCollection resultCollection = null;
            int i = 0;
            int TemplateCount;
            

            try
            {
                // Look for the Display Name of a template OID in AD
                searcher = new DirectorySearcher(templatesEntry);
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
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                // Clean up
                if (searcher != null)
                {
                    searcher.Dispose();
                }
            }
        }
    }
}
