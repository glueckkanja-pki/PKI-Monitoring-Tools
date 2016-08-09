using System;
using System.Net;
using System.IO;
using GK.NSSWrapper;
using log4net;

// configure log4net with app.config
[assembly: log4net.Config.XmlConfigurator]

namespace GK.PKIMonitoring.CheckWebCRL
{
    /// <summary>
    /// Downloads a CRL and checks how long it is still valid. If the validity is below a configurable threshold,
    /// a warning is written to stdout, otherwise just an info
    /// </summary>
    class Program
    {
        const int DEFAULT_THRESHOLD_HOURS = 96; // 4 days

        static ILog log;

        static void Main(string[] args)
        {
            log = LogManager.GetLogger("GK.PKIMonitoring.CheckWebCRL");
            log4net.ThreadContext.Properties["args"] = args;

            NSSDatabase.strDbPath = Consts.NssTemppath;
            if (string.IsNullOrWhiteSpace(Consts.NssTemppath))
            {
                log.Fatal("Configuration error: Please configure nss_temppath in the app.config file");
                throw new ArgumentException("Configuration error: Please configure nss_temppath in the app.config file", "nss_temppath");
            }
            log.Debug("NSS TempPath: " + Consts.NssTemppath);

            if (args.Length <= 0 || args[0].StartsWith("-") || args[0].StartsWith("/"))
            {
                printUsage();
                return;
            }

            string sURLCRL = args[0];
            log4net.ThreadContext.Properties["url"] = sURLCRL;
            int threshold_hours = DEFAULT_THRESHOLD_HOURS;
            try
            {
                if (args.Length > 1)
                    threshold_hours = int.Parse(args[1]);
                log4net.ThreadContext.Properties["threshold_hours"] = threshold_hours;
            }
            catch(Exception)
            {
                printUsage();
                return;
            }

            DateTime currentUTC = DateTime.UtcNow;
            string status = string.Empty;
            string shortDescription = string.Empty;
            string longDescription = string.Empty;

            try
            {
                WebRequest httpReq = HttpWebRequest.Create(sURLCRL);

                WebResponse httpResponse = httpReq.GetResponse();
                byte[] baCRL = ReadFully(httpResponse.GetResponseStream());

                X509NSSCRL crl = new X509NSSCRL(baCRL);

                if (crl.dateStartTime > currentUTC || crl.dateEndTime < currentUTC)
                {
                    ThreadContext.Properties["shortMessage"] = "CRL is invalid!";
                    log.Error("CRL from URL " + sURLCRL + " was valid from " + crl.dateStartTime.ToString("u") + " to " + crl.dateEndTime.ToString("u") + ". The CRL is invalid by now!");
                }
                else if (crl.dateEndTime.AddHours(-threshold_hours) < currentUTC)
                {
                    ThreadContext.Properties["shortMessage"] = "CRL expires soon.";
                    log.Warn("CRL from URL " + sURLCRL + " is valid until " + crl.dateEndTime.ToString("u") + 
                        ". The remaining validity period is below the configured threshold of " + threshold_hours + 
                        " hours. The CRL will expire soon and a new CRL should be published before expiration."
                    );
                }
                else
                {
                    ThreadContext.Properties["shortMessage"] = "CRL is still valid.";
                    log.Info("CRL from URL " + sURLCRL + " is valid until " + crl.dateEndTime.ToString("u") + ". The remaining validity period is longer than the threshold of " + threshold_hours + " hours.");
                }
            }
            catch (Exception ex)
            {
                ThreadContext.Properties["shortMessage"] = "Could not access CRL.";
                log.Fatal("Could not download CRL from URL " + sURLCRL + " as an exception of type " + ex.GetType().ToString() + " occurred: " + ex.Message);
            }

#if DEBUG
            Console.ReadKey();
#endif // DEBUG
        }

        private static void printUsage()
        {
            Console.WriteLine();
            Console.WriteLine("CheckWebCRL by Glueck & Kanja Consulting AG 2011");
            Console.WriteLine("Downloads a CRL and checks how long it is still valid. If the validity is below a configurable threshold, a warning is written to stdout, otherwise just an info.");
            Console.WriteLine();
            Console.WriteLine("USAGE: CheckWebCRL.exe URL [ThresholdHours]");
            Console.WriteLine();
            Console.WriteLine("     URL             - Where to download the CRL from");
            Console.WriteLine("     ThresholdHours  - If the CRL is valid less hours than this threshold, the program will warn that the CRL expires soon");
            Console.WriteLine();
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
    }
}
