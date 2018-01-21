using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace thinWallet
{
    class WWW : System.Net.WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            req.Timeout = 10000;//10秒超时
            return req;
        }
        public static async Task<string> Get(string url)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var result = await wc.DownloadStringTaskAsync(url);
            return result;
        }
        public static string GetWithDialog(Window owner, string url)
        {
            Dialog_Wait waitdlg = new Dialog_Wait();
            waitdlg.Owner = owner;
            waitdlg.call = async () =>
             {
                 return await Get(url);
             };
            if (waitdlg.ShowDialog() == true)
            {
                return waitdlg.callresult as string;
            }
            return null;
        }
        public static string MakeRpcUrl(string url, string method, params MyJson.IJsonNode[] _params)
        {
            StringBuilder sb = new StringBuilder();
            if (url.Last() != '/')
                url = url + "/";

            sb.Append(url + "?jsonrpc=2.0&id=1&method=" + method + "&params=[");
            for (var i = 0; i < _params.Length; i++)
            {
                _params[i].ConvertToString(sb);
                if (i != _params.Length - 1)
                    sb.Append(",");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
