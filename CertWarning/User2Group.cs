
namespace GK.PKIMonitoring.CertWarning
{
    class User2Group
    {
        public string GetMapping(string fileName, string userName)//, string iniFileName)
        {
            string line;
            string group = "";
            string[] strArray;
//            IniParser parser = new IniParser(iniFileName);
            string strReplaceRequesterDomain = Consts.WarningMail_ReplaceRequesterDomain;// parser.GetSetting("EMAIL", "strReplaceRequesterDomain");

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            
            while((line = file.ReadLine()) != null)
            {
                line = line.ToUpper();
                userName = userName.ToUpper();
                strReplaceRequesterDomain = strReplaceRequesterDomain.ToUpper();
                userName = userName.Replace(strReplaceRequesterDomain, "");

                if (line.Contains(",") == true)
                {
                    strArray = line.Split(',');
                    if (strArray[0].Equals(userName) == true)
                    {
                        group = strArray[1];
                    }
                }
            }
            file.Close();
            return group;
        }
    }
}
