﻿using RestSharp;
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
        private static Queue<string> _downloadUrls = new Queue<string>();

        //textBox1.Text的值
        public static string inputUrl = String.Empty;
 

                //根據傳進來的html解析出影片下載連結
        public static Queue<string> GetDownLoadLink(string responseHtml)
        {
            //從回傳的html解析出影片id
            List<string> VideoIdList = new List<string>();
            Regex qariRegex = new Regex(@"gdclick_2dg\(this,\'(?<id>[a-zA-Z0-9-_]*)\',\'html5\'\)");
            MatchCollection mc = qariRegex.Matches(responseHtml);
            foreach (Match m in mc)
            {
                //將解析出來的網址放入List<T>裡
                string id = m.Groups["id"].Value;
                VideoIdList.Add(id);
            }

            foreach (var id in VideoIdList) {
                string Url = String.Format("https://video.google.com/get_player?docid={0}&ps=docs&partnerid=30&cc_load_policy=1",id);
                var restClient = new RestClient(Url);
                var request = new RestRequest(Method.GET);
                var response = restClient.Execute(request);
                //會回傳一個pdf 但還無法解析回傳值
                string a = response.Content.ToString();
                //_downloadUrls.Enqueue(Url);
            }

            return _downloadUrls;
        }
    }
}
