using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //step1 解析出頁面的vimeo iframe
            var restClient = new RestClient("http://www.wenguitar.com/gtfree1.html");
            var request = new RestRequest(Method.GET);
            var response = restClient.Execute(request);

            //step2 從回傳的html解析出iframe網址

            //step3 向iframe網址發出請求
            var restClient2 = new RestClient("https://player.vimeo.com/video/99632493?title=0&byline=0&portrait=0");
            //step4 從iframe網址回傳的response取得mp4的位址

            //step5 開始下載
            //startDownload();
        }




        #region 檔案下載相關
        private void startDownload()
        {
            Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri("https://14-lvl3-pdl.vimeocdn.com/01/4926/3/99632493/267008272.mp4?expires=1471849315&token=0f1f5dd0f5901cc337d25"), @"C:\Users\Administrator\Desktop\test.mp4");
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
