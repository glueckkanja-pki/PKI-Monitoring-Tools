using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using log4net;
using System.Collections.Generic;

// configure log4net with app.config
[assembly: log4net.Config.XmlConfigurator]

namespace GK.PKIMonitoring.CheckSSLCert
{
    class Program
    {
        static ILog log;

        const int DEFAULT_THRESHOLD_HOURS = 336; // 2 weeks

        static string sURLTarget;
        static int threshold_hours;
        static bool fSSLCheckFinished = false;
        static bool fVerifyCertificate = false;

        static void Main(string[] args)
        {
            log = LogManager.GetLogger("GK.PKIMonitoring.CheckSSLCert");
            log4net.ThreadContext.Properties["args"] = args;

            if (args.Length <= 0 || args[0].StartsWith("-") || args[0].StartsWith("/"))
            {
                printUsage();
                return;
            }

            sURLTarget = args[0];
            log4net.ThreadContext.Properties["url"] = sURLTarget;

            if (!sURLTarget.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                printUsage();
                Console.WriteLine("URL does not start with \"HTTPS://\". SSL certificates are used only in HTTPS URLs...");
                Console.WriteLine();
                return;
            }

            threshold_hours = DEFAULT_THRESHOLD_HOURS;
            try
            {
                if (args.Length > 1)
                    threshold_hours = int.Parse(args[1]);
                log4net.ThreadContext.Properties["threshold_hours"] = threshold_hours; 
                if (args.Length > 2)
                    if (args[2].Equals("-v", StringComparison.InvariantCultureIgnoreCase) || args[2].Equals("/v", StringComparison.InvariantCultureIgnoreCase))
                        fVerifyCertificate = true;
                    else
                    {
                        printUsage();
                        return;
                    }
            }
            catch(Exception)
            {
                printUsage();
                return;
            }

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(checkCertificate4Validity);
                WebRequest httpReq = HttpWebRequest.Create(sURLTarget);
                WebResponse httpResponse = httpReq.GetResponse();

                if (!fSSLCheckFinished)
                {
                    ThreadContext.Properties["shortMessage"] = "SSL check impossible";
                    log.Error("When accessing web server at URL " + sURLTarget + ", no SSL check could be performed, although no exception or error occurred.");
                }
            }
            catch (Exception ex)
            {
                    // exceptions (like 404) are only a problem if the SSL check does not work
                if (!fSSLCheckFinished)
                {
                    ThreadContext.Properties["shortMessage"] = "Could not access web server";
                    log.Fatal("Could not access web server at URL " + sURLTarget, ex);
                }
            }

#if DEBUG
            Console.ReadKey();
#endif // DEBUG
        }

        private static bool checkCertificate4Validity(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors policyErrors
        )
        {
            HttpWebRequest httpReq = sender as HttpWebRequest;
            string sURLTarget = httpReq.RequestUri.ToString();

            DateTime currentUTC = DateTime.UtcNow;

            try
            {
                X509Certificate2 serverCert = new X509Certificate2(certificate);

                if (serverCert.NotBefore > currentUTC || serverCert.NotAfter < currentUTC)
                {
                    ThreadContext.Properties["shortMessage"] = "SSL certificate is invalid!";
                    log.Error("SSL certificate used by the server at URL " + sURLTarget + " was valid from " + serverCert.NotBefore.ToString("u") + " to " + serverCert.NotAfter.ToString("u") + ". The SSL certificate is invalid by now!");
                }
                else if (fVerifyCertificate && !isCertValid4ServerName(serverCert, httpReq.RequestUri.Host))
                {
                    ThreadContext.Properties["shortMessage"] = "SSL certificate does not match host name!";
                    log.Error("The SSL certificate used by the server at URL " + sURLTarget + " is within its validity range. " +
                        "However, it was issued with the subject \"" + serverCert.Subject + "\" and is not considered a valid SSL certificate for the server's hostname. " +
                        "This program does not check for wildcard certificates and Subject Alternative Names. For these certificates, do not enable verbose certificate checks with -v.");
                }
                else if (fVerifyCertificate && !serverCert.Verify())
                {
                    ThreadContext.Properties["shortMessage"] = "SSL certificate is untrusted!";
                    log.Error("The SSL certificate used by the server at URL " + sURLTarget + " is within its validity range. " +
                        "However, it is not considered trusted by this computer. This may be caused by an inaccessible CRL or by intermediate " +
                        "certificates that have not been installed on this computer.");
                }
                else if (serverCert.NotAfter.AddHours(-threshold_hours) < currentUTC)
                {
                    ThreadContext.Properties["shortMessage"] = "SSL certificate expires soon.";
                    log.Warn("SSL certificate used by the server at URL " + sURLTarget + " is valid until " + serverCert.NotAfter.ToString("u") +
                        ". The remaining validity period is below the configured threshold of " + threshold_hours +
                        " hours. The SSL certificate will expire soon and should be replaced with a new SSL certificate before expiration.");
                }
                else
                {
                    ThreadContext.Properties["shortMessage"] = "SSL certificate is still valid.";
                    log.Info("SSL certificate used by the server at URL " + sURLTarget + " is valid until " + serverCert.NotAfter.ToString("u") +
                        ". The remaining validity period is longer than the threshold of " + threshold_hours + " hours.");
                }
            }
            catch (Exception ex)
            {
                ThreadContext.Properties["shortMessage"] = "Could not check SSL certificate.";
                log.Fatal("Could not check SSL certificate used by the server at URL " + sURLTarget + " as an exception of type " + ex.GetType().ToString() + " occurred: " + ex.Message);
            }

            fSSLCheckFinished = true;
            return true;
        }

        private static bool isAnyInStringEnum(IEnumerable<string> listOfStrings2Search, string searchString)
        {
            foreach (string currentString in listOfStrings2Search)
                if (currentString.Equals(searchString, StringComparison.InvariantCultureIgnoreCase))
                    return true;

            return false;
        }

        private static bool isCertValid4ServerName(X509Certificate2 serverCert, string hostname)
        {
            string[] subjectDnComponents = serverCert.SubjectName.Format(true).Split(new char[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            if (isAnyInStringEnum(subjectDnComponents, "CN=" + hostname))   // Didn't use linq, because of .NET 2.0 compatibility
                return true;

            // TODO: Check wildcard certs and Subject Alternative Names

            return false;
        }

        private static void printUsage()
        {
            Console.WriteLine();
            Console.WriteLine("CheckSSLCert by Glueck & Kanja Consulting AG 2011");
            Console.WriteLine("Accesses a HTTPS URL and checks how long its SSL certificate is still valid. If the validity is below a configurable threshold, a warning is written to stdout, otherwise just an info.");
            Console.WriteLine();
            Console.WriteLine("USAGE: CheckSSLCert.exe URL [ThresholdHours [-v]]");
            Console.WriteLine();
            Console.WriteLine("     URL             - Which web server to access (Must be an HTTPS URL)");
            Console.WriteLine("     ThresholdHours  - If the SSL certificate is valid less hours than this threshold, the program will warn. (default 2 weeks)");
            Console.WriteLine("     -v              - This option enables stricter SSL certificate checking (certificate chain and revocation)");
            Console.WriteLine();
        }
    }
}
