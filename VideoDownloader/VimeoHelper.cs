using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoDownloader
{
    public class VimeoHelper
    {
        //解析出來的下載連結
        private static Queue<string> _downloadUrls = new Queue<string>();

        //textBox1.Text的值
        public static string inputUrl = String.Empty;
 

        //根據傳進來的html解析出影片下載連結
        public static Queue<string> GetVimeoDownLoadLink(string responseHtml)
        {


            //從回傳的html解析出iframe網址
            List<string> IframeUrlList = new List<string>();
            Regex qariRegex = new Regex(@"(?<match>//player.vimeo.com/video/\d*\?[^""]*)");
            MatchCollection mc = qariRegex.Matches(responseHtml);
            foreach (Match m in mc)
            {
                //將解析出來的網址放入List<T>裡
                string url = "https:" + m.Groups["match"].Value.Replace("amp;", "");
                IframeUrlList.Add(url);
            }

            if (IframeUrlList.Count == 0)
            {
                throw new ArgumentException("找不到嵌入的Vimeo影片!!");
            }

            //向iframe網址發出請求 並回傳html
            string html = string.Empty;
            Regex reg = new Regex(@"(?<match>https?://[0-9a-zA-Z-]*.vimeocdn.com/[a-z0-9\d-/]*.mp4[^""]*)");
            IframeUrlList.ForEach(delegate(String url)
            {
                html = SendRequestToVimeo(url, inputUrl);
                MatchCollection match = reg.Matches(html);
                foreach (Match m in match)
                {
                    _downloadUrls.Enqueue(m.Groups["match"].Value);
                }
            });

            return _downloadUrls;
        }

        /// <summary>
        /// 取得iframe的html 用來解析出mp4網址
        /// </summary>
        /// <param name="url">iframe網址</param>
        /// <returns></returns>
        private static string SendRequestToVimeo(string url, string inputUrl)
        {
            HttpWebRequest requestFromVimeo = HttpWebRequest.Create(url) as HttpWebRequest;
            string result = null;
            requestFromVimeo.Method = "Get";
            //取得domain url
            Uri myUri = new Uri(inputUrl);
            string host = "http://" + myUri.Host;
            requestFromVimeo.Referer = host;//必須要加這個 才能避免vimeo forbidden
            requestFromVimeo.UserAgent = " Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            using (WebResponse responseFromVimeo = requestFromVimeo.GetResponse())
            {
                StreamReader sr = new StreamReader(responseFromVimeo.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }
            return result;
        }
    }
}
