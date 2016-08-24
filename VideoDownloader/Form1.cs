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
        private Queue<string> _downloadUrls = new Queue<string>();
        //委派 決定要呼叫哪個helper的function
        public delegate Queue<string> GetDownLoadLink(string responseHtml);
        GetDownLoadLink downloadVideo;

        public Form1()
        {
            InitializeComponent();
            //下載地址
            //textBox1.Text = "http://www.wenguitar.com/tw-index.php";//蔡文展
            textBox1.Text = "http://2d-gate.org/thread-6613-1-1.html#.V7cLofl96Uk";//二次元之門
            //存檔路徑預設是桌面
            textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //下拉選項的選項
            Dictionary<string, string> test = new Dictionary<string, string>();
            test.Add("1", "嵌入的Vimeo");
            test.Add("2", "二次元之門");
            comboBox1.DataSource = new BindingSource(test, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";
            comboBox1.SelectedIndex = 1;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //step0 重置各參數
                progressBar1.Value = 0;

                button1.Enabled = false;
                button2.Enabled = false;

                if (String.IsNullOrEmpty(textBox1.Text))
                {
                    throw new ArgumentException("請輸入網址!!");
                }
                //取得回傳的html
                var restClient = new RestClient(textBox1.Text);
                var request = new RestRequest(Method.GET);
                var response = restClient.Execute(request);

                //step2 依據影片來源(vimeo/二次元之門) 從回傳的html中取得下載連結
                if (comboBox1.Text.Contains("Vimeo"))
                {
                    VimeoHelper.inputUrl = textBox1.Text;
                    downloadVideo = VimeoHelper.GetVimeoDownLoadLink;
                }
                else if (comboBox1.Text.Contains("二次元之門"))
                {
                    downloadVideo = _2GateHelper.GetDownLoadLink;
                }

                //取得影片下載連結的佇列
                _downloadUrls = downloadVideo(response.Content.ToString());

                if (!_downloadUrls.Any())
                {
                    throw new ArgumentException("找不到影片下載連結!!");
                }
                //step3 開始下載

                startDownload();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }




        #region 檔案下載相關
        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <param name="url">下載地址</param>
        private void startDownload()
        {
            try
            {
                //用佇列和遞迴的方式解決多檔案的問題
                if (_downloadUrls.Any())
                {
                    label5.Text = "剩餘檔案數量:" + _downloadUrls.Count().ToString();
                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    //下載地址和儲存路徑  儲存路徑由使用者選擇的folder和guid和副檔名.mp4
                    var url = _downloadUrls.Dequeue();
                    client.DownloadFileAsync(new Uri(url), textBox2.Text + "\\" + Guid.NewGuid().ToString("N") + @".mp4");
                }
                else
                {
                    label5.Text = "剩餘檔案數量:" + _downloadUrls.Count().ToString();
                    label1.Text = "下載完成";
                    button1.Enabled = true;
                    button2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                 double bytesIn = double.Parse(e.BytesReceived.ToString());
                 double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = Math.Round( bytesIn / totalBytes * 100,1);
                label1.Text = "下載進度:" + percentage + "% ";
                progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle error scenario
                throw e.Error;
            }
            if (e.Cancelled)
            {
                // handle cancelled scenario
            }
            startDownload();
        }
        #endregion

        //瀏覽按鈕
        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "請選擇一個資料夾";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                   textBox2.Text=dlg.SelectedPath;
                }
            }
        }
    }
}
