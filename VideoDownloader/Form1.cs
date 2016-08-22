﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "http://www.wenguitar.com/gtfree1.html";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //step1 解析出頁面的vimeo iframe
            var restClient = new RestClient(textBox1.Text);
            var request = new RestRequest(Method.GET);
            var response = restClient.Execute(request);

            //step2 從回傳的html解析出iframe網址
            List<string> IframeUrlList = new List<string>();
            Regex qariRegex = new Regex(@"(?<match>//player.vimeo.com/video/\d{8}\?[^""]*)");
            MatchCollection mc = qariRegex.Matches(response.Content.ToString());
            foreach (Match m in mc)
            {
                //將解析出來的網址放入List<T>裡
                string url = "https:" + m.Groups["match"].Value.Replace("amp;", "");
                IframeUrlList.Add(url);
            }

            //step3 向iframe網址發出請求 並回傳html
            string html = string.Empty;
            List<string> videoLinkList = new List<string>();
            Regex reg = new Regex(@"(?<match>https?://[0-9a-zA-Z-]*.vimeocdn.com/[\d*/]*.mp4\?expires=\d*&token=[a-zA-Z0-9]*)");
            IframeUrlList.ForEach(delegate(String url)
            {
                html=SendRequestToVimeo(url);
                MatchCollection match = reg.Matches(html);
                foreach (Match m in match)
                {
                    videoLinkList.Add(m.Groups["match"].Value);
                }
            });

            //step4 開始下載
            videoLinkList.ForEach(delegate(String url)
            {
                startDownload(url);
            });
        }


        /// <summary>
        /// 取得iframe的html 用來解析出mp4網址
        /// </summary>
        /// <param name="url">iframe網址</param>
        /// <returns></returns>
        private string SendRequestToVimeo(string url) {
            HttpWebRequest requestFromVimeo = HttpWebRequest.Create(url) as HttpWebRequest;
            string result = null;
            requestFromVimeo.Method = "Get";
            //取得domain url
            Uri myUri = new Uri(textBox1.Text);
            string host = "http://"+myUri.Host;
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

        #region 檔案下載相關
        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <param name="url">下載地址</param>
        private void startDownload(string url)
        {
            Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(url), @"C:\Users\Administrator\Desktop\test.mp4");
            });
            thread.Start();
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                label1.Text = "下載中.."+Math.Round(percentage,1)+"%";
                progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                label1.Text = "下載完成";
            });
        }
        #endregion
    }
}
