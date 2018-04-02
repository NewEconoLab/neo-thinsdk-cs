using System;
using Neo;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace smartContractDemo_win
{
    public class Demo1
    {
        string api = "https://api.nel.group/api/testnet";

        httpHelper http = new httpHelper();

        public void Demo()
        {
            string scriptaddress = "0x2e88caf10afe621e90142357236834e010b16df2";
            string key = "9b87a694f0a282b2b5979e4138944b6805350c6fa3380132b21a2f12f9c2f4b6";
            var revkey = key.HexToBytes().Reverse().ToHexString();
            //api 是 https://api.nel.group/api/testne?jsonrpc=2.0&id=1&method=getstorage&params=[]
            string result = http.HttpGet(api + "?jsonrpc=2.0&id=1&method=getstorage&params=[\"" + scriptaddress + "\"" + "," + "\"" + key + "\"]");
            Console.WriteLine("得到的结果是：" + result);
        }
    }
}
