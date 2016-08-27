using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoDownloader
{
    public class WenGuitarHelper
    {
        //儲存登入cookie
        private static CookieContainer _cookieJar = new CookieContainer();


        //登入蔡文展
        public static CookieContainer Login(string account, string pwd)
        {
            RestClient client = new RestClient("http://www.wenguitar.com/");
            client.CookieContainer = new CookieContainer();
            RestRequest login = new RestRequest("/login.php", Method.POST);

            if (String.IsNullOrEmpty(account) || String.IsNullOrEmpty(pwd))
                throw new ArgumentException("無法登入蔡文展吉他 請在config設置帳號密碼");
            login.AddParameter("inputEmail", account);
            login.AddParameter("inputPassword", pwd);

            var response = client.Execute(login);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (client.CookieContainer.Count == 0)
                    throw new ArgumentException("無法登入蔡文展吉他 無法取得登入cookie");
                _cookieJar = client.CookieContainer;
            }
            return _cookieJar;

        }

    }
}
