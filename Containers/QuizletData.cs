using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using HtmlAgilityPack;

namespace QuizMe.Containers
{
    class QuizletData
    {
        private static readonly Regex URLREGEX = new Regex("([a-zA-Z]+(-[a-zA-Z]+)+)", RegexOptions.Compiled);
        public Dictionary<string, string> Data
        { get; set; }
        public string url { get; set; }


        public QuizletData(string url)
        {
            this.url = url;
        }

        public async Task init()
        {
            this.Data = parseHTML(await fetchWebpage(this.url));
        }

        private async Task<string> fetchURL(string url)
        {
            Console.WriteLine("Parsing URL");
            var baseAddress = new Uri(url);
            using (var handler = new HttpClientHandler { UseCookies = true })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; CrOS x86_64 14816.99.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US");
                Trace.WriteLine("Headers configured");
                var message = new HttpRequestMessage();
                Trace.WriteLine("Making GET request");
                var result = await client.SendAsync(message);
                result.EnsureSuccessStatusCode();
                Trace.WriteLine("GET success");
                var resp = await result.Content.ReadAsStringAsync();
                return resp;
            }
        }

        public async Task<String> fetchWebpage(string url)
        {
            string resp = await fetchURL(url);
            return resp;
        }

        private Dictionary<string, string> parseHTML(string data)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            var terms = doc.DocumentNode.Descendants("div")
            .Where(node => node.GetAttributeValue("class", "").Equals("SetPageTerms-term"))
            .ToList();

            Trace.WriteLine($"Loading {terms.Count} terms from the webpage.");

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var element in terms)
            {
                var termlist = element.Descendants("a")
                .Where(node => node.GetAttributeValue("class", "").Equals("SetPageTerm-wordText"))
                .ToList();

                string term = termlist[0].Descendants("span").First().InnerText;

                var deflist = element.Descendants("a")
                .Where(node => node.GetAttributeValue("class", "").Equals("SetPageTerm-definitionText"))
                .ToList();

                string def = deflist[0].Descendants("span").First().InnerText;

                try
                {
                    dict.Add(term, def);
                } catch (ArgumentException e)
                {
                    dict.Add(term + " (dupe)", def);
                }
            }

            return dict;
        }

        /// <summary>
        /// Returns the dictionary in the order of "definition": "term".
        /// </summary>
        /// <returns>Dictionary with the terms and definitions swapped.</returns>
        public Dictionary<string, string> swapTermsAndDefinitions()
        {
            var dict = new Dictionary<string, string>();
            var terms = Data.Keys;
            var def = Data.Values;

            for (int i = 0; i < terms.Count; i++)
            {
                try
                {
                    dict.Add(def.ElementAt(i), terms.ElementAt(i));
                } catch (ArgumentException e)
                {
                    dict.Add(def.ElementAt(i) + " (dupe)", terms.ElementAt(i));
                }
            }

            return dict;
        }

        public string fetchUrlDescription()
        {
            string desc = URLREGEX.Matches(this.url).ElementAt(0).Value;
            return desc;
        }
    }
}