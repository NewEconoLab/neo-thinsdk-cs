using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using smartContractDemo.tests;

namespace smartContractDemo
{
    public class Height : ITest
    {
        public  string api = Config.api; //"https://seed1.spotcoin.com:10332";

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
