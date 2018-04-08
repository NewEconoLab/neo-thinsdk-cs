using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace smartContractDemo
{
    public class Nep55_1 : ITest
    {
        public const string api = "https://api.nel.group/api/testnet";
        public const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        public const string nep55 = "0x2648a04c31d0be649065275db7239d8a8fe0f021";
        public const string testwif = "L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";

        public string Name => "Nep5.5 查询余额";

        public string ID => "N5 1";
        async public Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);
            Console.WriteLine("address=" + address);

            string script = null;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)balanceOf"));//参数倒序入
                byte[] shash = ThinNeo.Helper.HexString2Bytes(nep55);
                sb.EmitAppCall(shash.Reverse().ToArray());//nep5脚本




                var data = sb.ToArray();
                script = ThinNeo.Helper.Bytes2HexString(data);

            }

            //var url = Helper.MakeRpcUrl(api, "invokescript", new MyJson.JsonNode_ValueString(script));
            //string result = await Helper.HttpGet(url);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
            var json = MyJson.Parse(result).AsDict();
            if (json.ContainsKey("result"))
            {
                var resultv = json["result"].AsList()[0].AsDict()["stack"].AsList()[0].AsDict();
                var rtype = resultv["type"].AsString();
                var rvalue = resultv["value"].AsString();
                Console.WriteLine("type=" + rtype + "  value=" + rvalue);
                var n = new System.Numerics.BigInteger(ThinNeo.Helper.HexString2Bytes(rvalue));
                Console.WriteLine("value dec=" + n.ToString());

            }

        }
    }
}
