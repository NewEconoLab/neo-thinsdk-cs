using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo_win
{
    public class httpHelper
    {
        /// <summary>
        /// 同步get请求
        /// </summary>
        /// <param name="url">链接地址</param>    
        /// <param name="formData">写在header中的键值对</param>
        /// <returns></returns>

        public string HttpGet(string url, List<KeyValuePair<string, string>> formData = null)

        {

            HttpClient httpClient = new HttpClient();
            //HttpContent content = new FormUrlEncodedContent(formData);

            //if (formData != null)
            //{
            //    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            //    content.Headers.ContentType.CharSet = "UTF-8";
            //    for (int i = 0; i < formData.Count; i++)
            //    {
            //        content.Headers.Add(formData[i].Key, formData[i].Value);
            //    }
            //}

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            //for (int i = 0; i < formData.Count; i++)
            //{
            //    request.Headers.Add(formData[i].Key, formData[i].Value);
            //}

            var res = httpClient.SendAsync(request);
            res.Wait();
            var resp = res.Result;

            Task<string> temp = resp.Content.ReadAsStringAsync();

            temp.Wait();

            return temp.Result;
        }

        /// <summary>
        /// 同步请求post（键值对形式）
        /// </summary>
        /// <param name="uri">网络基址("http://localhost:59315")</param>
        /// <param name="url">网络的地址("/api/UMeng")</param>
        /// <param name="formData">键值对List<KeyValuePair<string, string>> formData = new List<KeyValuePair<string, string>>();formData.Add(new KeyValuePair<string, string>("userid", "29122"));formData.Add(new KeyValuePair<string, string>("umengids", "29122"));</param>
        /// <param name="charset">编码格式</param>
        /// <param name="mediaType">头媒体类型</param>
        /// <returns></returns>
        public string HttpPost(string uri, string url, List<KeyValuePair<string, string>> formData = null, string charset = "UTF-8", string mediaType = "application/x-www-form-urlencoded")
        {
            string tokenUri = url;
            var client = new HttpClient();
            client.BaseAddress = new Uri(uri);
            HttpContent content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            content.Headers.ContentType.CharSet = charset;

            for (int i = 0; i < formData.Count; i++)
            {
                content.Headers.Add(formData[i].Key, formData[i].Value);
            }

            var res = client.PostAsync(tokenUri, content);
            res.Wait();
            HttpResponseMessage resp = res.Result;

            var res2 = resp.Content.ReadAsStringAsync();
            res2.Wait();

            string token = res2.Result;
            return token;
        }

        /// <summary>
        /// 异步请求get(UTF-8)
        /// </summary>
        /// <param name="url">链接地址</param>    
        /// <param name="formData">写在header中的内容</param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(string url, List<KeyValuePair<string, string>> formData = null)
        {

            HttpClient httpClient = new HttpClient();

            //HttpContent content = new FormUrlEncodedContent(formData);

            //if (formData != null)
            //{
            //    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            //    content.Headers.ContentType.CharSet = "UTF-8";

            //    for (int i = 0; i < formData.Count; i++)
            //    {
            //        content.Headers.Add(formData[i].Key, formData[i].Value);
            //    }
            //}

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            //for (int i = 0; i < formData.Count; i++)
            //{
            //    request.Headers.Add(formData[i].Key, formData[i].Value);
            //}

            var resp = await httpClient.SendAsync(request);
            resp.EnsureSuccessStatusCode();

            string token = await resp.Content.ReadAsStringAsync();

            return token;
        }

        /// <summary>
        /// 异步请求post（键值对形式,可等待的）
        /// </summary>
        /// <param name="uri">网络基址("http://localhost:59315")</param>
        /// <param name="url">网络的地址("/api/UMeng")</param>
        /// <param name="formData">键值对List<KeyValuePair<string, string>> formData = new List<KeyValuePair<string, string>>();formData.Add(new KeyValuePair<string, string>("userid", "29122"));formData.Add(new KeyValuePair<string, string>("umengids", "29122"));</param>
        /// <param name="charset">编码格式</param>
        /// <param name="mediaType">头媒体类型</param>
        /// <returns></returns>
        public async Task<string> HttpPostAsync(string uri, string url, List<KeyValuePair<string, string>> formData = null, string charset = "UTF-8", string mediaType = "application/x-www-form-urlencoded")
        {
            string tokenUri = url;
            var client = new HttpClient();
            client.BaseAddress = new Uri(uri);
            HttpContent content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            content.Headers.ContentType.CharSet = charset;

            for (int i = 0; i < formData.Count; i++)
            {
                content.Headers.Add(formData[i].Key, formData[i].Value);
            }

            HttpResponseMessage resp = await client.PostAsync(tokenUri, content);
            resp.EnsureSuccessStatusCode();
            string token = await resp.Content.ReadAsStringAsync();

            return token;
        }

        //流模式post
        public string Post(string url, string data, Encoding encoding, int type = 3)
        {
            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));
                if (type == 1)
                {
                    req.ContentType = "application/json;charset=utf-8";
                }
                else if (type == 2)
                {
                    req.ContentType = "application/xml;charset=utf-8";
                }
                else
                {
                    req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                }

                req.Method = "POST";
                //req.Accept = "text/xml,text/javascript";
                req.ContinueTimeout = 60000;

                byte[] postData = encoding.GetBytes(data);
                Stream reqStream = req.GetRequestStreamAsync().Result;
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Dispose();

                var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                var result = GetResponseAsString(rsp, encoding);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Dispose();
                if (stream != null) stream.Dispose();
                if (rsp != null) rsp.Dispose();
            }
        }
    }
}
