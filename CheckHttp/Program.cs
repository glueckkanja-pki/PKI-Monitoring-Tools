using System;
using log4net;
using System.Net;

// configure log4net with app.config
[assembly: log4net.Config.XmlConfigurator]

namespace GK.PKIMonitoring.CheckHttp
{
    class Program
    {
        static ILog log;

        static void Main(string[] args)
        {
            log = LogManager.GetLogger("GK.PKIMonitoring.CheckHttp");
            log4net.ThreadContext.Properties["args"] = args;

            if (args.Length <= 0 || args.Length > 2)
            {
                printUsage();
                return;
            }

            bool fAuthenticate;
            string sURLTarget;
            if (2 == args.Length)
                if (args[0].Substring(1).Equals("authenticate") &&
                    (args[0][0] == '/' || args[0][0] == '-'))
                {
                    fAuthenticate = true;
                    sURLTarget = args[1];
                }
                else
                {
                    printUsage();
                    return;
                }
            else if (args[0].StartsWith("-") || args[0].StartsWith("/"))    // only one argument
                {
                    printUsage();   // something like /? was entered
                    return;
                }
                else
                {
                    fAuthenticate = false;
                    sURLTarget = args[0];
                }

            log4net.ThreadContext.Properties["url"] = sURLTarget; 
            log4net.ThreadContext.Properties["authenticate"] = fAuthenticate;

            try
            {
                try
                {
                    HttpWebRequest httpReq = (HttpWebRequest)HttpWebRequest.Create(sURLTarget);
                    if (fAuthenticate)
                        httpReq.Credentials = CredentialCache.DefaultNetworkCredentials;
                    HttpWebResponse httpResponse = (HttpWebResponse)httpReq.GetResponse();

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        ThreadContext.Properties["shortMessage"] = "Web server is running.";
                        log.Info("Web server at URL " + sURLTarget + " responds with HTTP OK.");
                    }
                    else
                    {
                        ThreadContext.Properties["shortMessage"] = "Web server has problems.";
                        log.Error("Web server at URL " + sURLTarget + " responds with HTTP Status code \"" + httpResponse.StatusCode.ToString() + "\" (" +
                            ((int)httpResponse.StatusCode).ToString() + ")");
                    }
                }
                catch (WebException webEx)
                {
                    HttpWebResponse httpResponse = webEx.Response as HttpWebResponse;

                    if (null == httpResponse)
                    {
                        ThreadContext.Properties["shortMessage"] = "No response from web server.";
                        log.Fatal("Could not access web server at URL " + sURLTarget + " because a WebException occurred: " + webEx.Message);
                    }
                    else
                    {
                        ThreadContext.Properties["shortMessage"] = "Web server has problems.";
                        log.Error("Web server at URL " + sURLTarget + " responds with HTTP Status code \"" + httpResponse.StatusCode.ToString() + "\" (" +
                            ((int)httpResponse.StatusCode).ToString() + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                ThreadContext.Properties["shortMessage"] = "Serious error accessing web server.";
                log.Fatal("Could not access web server at URL " + sURLTarget, ex);
            }

#if DEBUG
            Console.ReadKey();
#endif // DEBUG
        }

        private static void printUsage()
        {
            Console.WriteLine();
            Console.WriteLine("CheckHttp by Glueck & Kanja Consulting AG 2011");
            Console.WriteLine("Accesses a URL and checks whether HTTP status code 200 is returned.");
            Console.WriteLine();
            Console.WriteLine("USAGE: CheckHttp.exe [/authenticate] URL");
            Console.WriteLine();
            Console.WriteLine("     /authenticate   - Use the logged on user credentials to authenticate against the web site");
            Console.WriteLine("     URL             - Which web server and file to access");
            Console.WriteLine();
        }
    }
}
