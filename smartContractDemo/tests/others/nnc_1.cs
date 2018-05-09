using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nnc_1 : ITest
    {
        public string Name => "NNC 查询余额";

        public string ID => "nc 1";

        public const string sc_nnc = "0xe792196084c99536490e9d275d38b65a3b4793a9";//nnc 合约地址
        //public const string sc_sell = "0x0989dfa7a767857f35711eb6afa0e4091643bbd1";//注册器合约地址

        public const string api = "https://api.nel.group/api/testnet";
        public const string api_local = "http://localhost:20332";
        public  string testwif = Config.test_wif;//"L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";
        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            //得到注册器
            var info_reg = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash("sell").ToString());
            var reg_sc = new Hash160(info_reg.value.subItem[0].subItem[1].data);
            Console.WriteLine("reg=" + reg_sc.ToString());

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
                    sb.EmitAppCall(reg_sc);

                    var data = sb.ToArray();
                    script = ThinNeo.Helper.Bytes2HexString(data);

                }

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
