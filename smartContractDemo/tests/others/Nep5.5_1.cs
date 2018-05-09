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
        //public const string nep55 = "0x2648a04c31d0be649065275db7239d8a8fe0f021";
        //public const string nep55 = "0xc57052599db703fb28fbf0a8a45b5afb36720866";//gas 55 第三版
        //public const string nep55 = "0xdcd55d42a8311f8bccd7badc0d26c221933fc522";//gas 55 第四版
        //public const string nep55 = "0x76894aa9f2a6469d0f8852d1d21ffe5ea247f514";//gas 55 第五版
        //public const string nep55 = "0x5a7483c89243fc366f7236d0a2a97d1d31c62ca3";//gas 55 不计数了，排除法
        //public const string nep55 = "0x11ff9455b2283beea6165f08aa7f90dfd0ca369f";//gas 55 第七版
        //public const string nep55 = "0x35fb9eb15370b54d6c14abf3272c2a0ffc5c79a0";//gas 55 第八版
        //public const string nep55 = "0x219fb2735bcf4f77a7240c446261402ab594c3e9";//gas 55 第九版
        //public const string nep55 = "0xc6a306ac02c31e5764cde34dc63c74b4a988c2a0";//gas 55 第十版
        //public const string nep55 = "0x5926cd4991b2a946b87c3b02da8b89d86ea7023b";//gas 55 第十一版
        public const string nep55 = "0xbc0fdb1c1b84601a9c66594cb481b684b90e05bb";//gas 55 第12版

        public const string testwif = "L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";

        public static ThinNeo.Hash256 lastNep5Tran;
        public string Name => "Nep5.5 查询余额";

        public string ID => "N5 1";
        async public Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);
            Console.WriteLine("address=" + address);

            {//查balance
                string script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)balanceOf"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(nep55);
                    sb.EmitAppCall(shash);//nep5脚本
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

            if(lastNep5Tran!=null)
            {
                string script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(hex256)" + lastNep5Tran.ToString());
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)getTXInfo"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(nep55);
                    sb.EmitAppCall(shash);//nep5脚本
                    var data = sb.ToArray();
                    script = ThinNeo.Helper.Bytes2HexString(data);

                }
                byte[] postdata;
                var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
                var result = await Helper.HttpPost(url, postdata);
                Console.WriteLine("得到的结果是：" + result);

            }
        }
    }
}
