using RestSharp;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoDownloader
{
    public class _2GateHelper
    {
        //解析出來的下載連結
        private static Queue<Model.FileInfo> _downloadUrls = new Queue<Model.FileInfo>();

        //textBox1.Text的值
        public static string inputUrl = String.Empty;
 

                //根據傳進來的html解析出影片下載連結
        public static Queue<Model.FileInfo> GetDownLoadLink(string responseHtml)
        {
            //從回傳的html解析出影片id
            List<string> VideoIdList = new List<string>();
            Regex qariRegex = new Regex(@"gdclick_2dg\(this,\'(?<id>[a-zA-Z0-9-_]*)\',\'html5\'\)");
            MatchCollection mc = qariRegex.Matches(responseHtml);
            foreach (Match m in mc)
            {
                //將解析出來的影片ID放入List<T>裡
                string id = m.Groups["id"].Value;
                VideoIdList.Add(id);
            }

            if (VideoIdList.Count == 0) {
                throw new ArgumentException("無法讀取到頁面影片");
            }

            foreach (var id in VideoIdList) {
                //依照影片id取得影片下載網址
                string Url = String.Format("https://docs.google.com/get_video_info?authuser=&docid={0}", id);
                var restClient = new RestClient(Url);
                var request = new RestRequest(Method.GET);
                var response = restClient.Execute(request);
                //需要進行兩次的decode
                string urlDecode = HttpUtility.UrlDecode(HttpUtility.UrlDecode(response.Content.ToString()));
                //將下載連結解析出來 放進佇列
                Regex regex = new Regex(@"&title=(?<title>[^&]*)&.*url=(?<url>https:(.*&)*key=[^&]*)");
                Match linkMatch = regex.Match(urlDecode);
                Model.FileInfo fileInfo = new Model.FileInfo();
                fileInfo.DownLoadLink = linkMatch.Groups["url"].ToString();
                fileInfo.FileName = linkMatch.Groups["title"].ToString().Replace(".mp4","");
                _downloadUrls.Enqueue(fileInfo);
            }
            if (_downloadUrls.Count == 0) {
                throw new ArgumentException("無法取得影片下載連結");
            }

            return _downloadUrls;
        }
    }
}
