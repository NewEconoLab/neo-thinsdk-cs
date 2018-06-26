using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace signtool
{
    class HttpHelper:System.Net.WebClient
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
            wc.Encoding = Encoding.UTF8;
            var result = await wc.DownloadStringTaskAsync(url);
            return result;
        }
        public static async Task<string> Post(string url, byte[] data)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers["content-type"] = "text/plain;charset=UTF-8";
            var resultbts = await wc.UploadDataTaskAsync(url, "POST", data);
            return System.Text.Encoding.UTF8.GetString(resultbts);
        }
        public static string MakeRpcUrlPost(string url, string method, out byte[] data, params MyJson.IJsonNode[] _params)
        {
            //if (url.Last() != '/')
            //    url = url + "/";
            var json = new MyJson.JsonNode_Object();
            json["id"] = new MyJson.JsonNode_ValueNumber(1);
            json["jsonrpc"] = new MyJson.JsonNode_ValueString("2.0");
            json["method"] = new MyJson.JsonNode_ValueString(method);
            StringBuilder sb = new StringBuilder();
            var array = new MyJson.JsonNode_Array();
            for (var i = 0; i < _params.Length; i++)
            {

                array.Add(_params[i]);
            }
            json["params"] = array;
            data = System.Text.Encoding.UTF8.GetBytes(json.ToString());
            return url;
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
