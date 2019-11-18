using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.IO;

namespace HAPBug
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Needs json containing list of urls.");
                Environment.Exit(1);
            }

            var urlfile = args[0];
            if (!File.Exists(urlfile))
            {
                Console.WriteLine($"{urlfile} not found.");
                Environment.Exit(2);
            }

            var timeoutMins = args.Length > 1 ? int.Parse(args[1]) : 1;

            dynamic urllist = JsonConvert.DeserializeObject(File.ReadAllText(urlfile));
            foreach (var url in urllist)
            {
                Test(url.Value, timeoutMins);
            }
        }

        private static void Test(string url, int timeoutMins)
        {
            Console.Write($"{url} ... ");
            var web = new HtmlWeb
            {
                CaptureRedirect = true,
                UseCookies = false,
                UsingCache = false,
                BrowserTimeout = TimeSpan.FromMinutes(timeoutMins),
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.97 Safari/537.36"
            };
            HtmlDocument doc;
            try
            {
                doc = web.LoadFromBrowser(url);
                Console.WriteLine($"{web.StatusCode} {doc.DocumentNode.OuterHtml.Length} bytes.");
            }
            catch (HtmlWebException HWE)
            {
                Console.Write($"LoadFromBrowser {HWE.Message}; ");
                if (HWE.InnerException != null)
                    Console.Write($"LoadFromBrowser {HWE.InnerException.Message}.");
                Console.WriteLine();
                File.AppendAllText("HAPBUG.TXT", $"{url}\t{HWE.Message}\r\n");
            }
            catch (Exception E)
            {
                Console.Write($"LoadFromBrowser {E.Message}; ");
                if (E.InnerException != null)
                    Console.Write($"LoadFromBrowser {E.InnerException.Message}.");
                Console.WriteLine();
                File.AppendAllText("HAPBUG.TXT", $"{url}\t{E.Message}\r\n");

            }
        }
    }
}
