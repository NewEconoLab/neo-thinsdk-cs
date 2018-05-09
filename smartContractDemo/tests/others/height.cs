using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace smartContractDemo
{
    public class Height : ITest
    {
        public const string api = "https://api.nel.group/api/testnet";

        public string Name => "show Height";

        public string ID => "height";

        async public Task Demo()
        {

            var url = Helper.MakeRpcUrl(api, "getblockcount");
            string result = await Helper.HttpGet(url);
            var json = MyJson.Parse(result).AsDict();
            if (json.ContainsKey("result"))
            {
                var resultv = json["result"].AsList()[0].AsDict();
                var count = resultv["blockcount"].AsString();
                Console.WriteLine("blockccount=" + count);
            }

        }
    }
}
