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
    public class RealSoundHelper
    {
        //儲存登入cookie
        private static CookieContainer _cookieJar = new CookieContainer();


        //登入蔡文展
        public static CookieContainer Login(string account, string pwd)
        {
            RestClient client = new RestClient("http://realsound.tw/");
            client.CookieContainer = new CookieContainer();
            RestRequest login = new RestRequest("active-member/guest", Method.POST);

            if (String.IsNullOrEmpty(account) || String.IsNullOrEmpty(pwd))
                throw new ArgumentException("無法登入嗚流吉他 請在config設置帳號密碼");
            login.AddParameter("login_user_name", account);
            login.AddParameter("login_pwd", pwd);
            login.AddParameter("doLogin", "登入");
            login.AddParameter("_wpnonce", "8386621e7e");//CSRF token 要研究是否會改變

            var response = client.Execute(login);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (client.CookieContainer.Count == 0)
                    throw new ArgumentException("無法登入嗚流吉他 無法取得登入cookie");
                _cookieJar = client.CookieContainer;
            }
            return _cookieJar;

        }

    }
}
