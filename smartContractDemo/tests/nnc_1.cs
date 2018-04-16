using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    class nnc_1 : ITest
    {
        public string Name => "NNC 查询余额";

        public string ID => "nc 1";

        public const string sc_nnc = "0xfe3c9a03242e90423093e1733b7640c847883205";//nnc 合约地址
        public const string sc_sell = "0x47ff3a6c5da432e0e884bb065b65b6cbfe020c10";//注册器合约地址

        public const string api = "https://api.nel.group/api/testnet";
        public const string testwif = "L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";
        public async Task Demo()
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
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(sc_nnc);
                    sb.EmitAppCall(shash);//nep5脚本


                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)balanceOf"));//参数倒序入
                    sb.EmitAppCall(new ThinNeo.Hash160(sc_sell));//nep5脚本

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
                    if (rtype == "Integer")
                    {
                        decimal num = decimal.Parse(rvalue) / (decimal)100000000;
                        Console.WriteLine("value dec=" + num.ToString());
                    }
                    else
                    {
                        var n = new System.Numerics.BigInteger(ThinNeo.Helper.HexString2Bytes(rvalue));
                        decimal num = (decimal)n / (decimal)100000000;
                        Console.WriteLine("value dec=" + num.ToString());
                    }
                }
            }
        }

    }
}
