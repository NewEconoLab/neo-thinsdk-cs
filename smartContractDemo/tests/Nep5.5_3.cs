using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace smartContractDemo
{
    public class Nep55_3 : ITest
    {
        public string Name => "Nep5.5 退回Gas";

        public string ID => "N5 3";
        async public Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Nep55_1.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            var nep55_shash = new ThinNeo.Hash160(Nep55_1.nep55);
            string nep55_address = ThinNeo.Helper.GetAddressFromScriptHash(nep55_shash);


            Console.WriteLine("address=" + address);

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Nep55_1.api, nep55_address);
            if (dir.ContainsKey(Nep55_1.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return;
            }
            ThinNeo.Transaction tran = null;
            {
                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)exchangeUTXO"));//参数倒序入
                    var shash = new ThinNeo.Hash160(Nep55_1.nep55);
                    sb.EmitAppCall(shash);//nep5脚本
                    script = sb.ToArray();
                }
                Console.WriteLine("contract address=" + nep55_address);//往合约地址转账

                //生成交易
                tran = Helper.makeTran(dir[Nep55_1.id_GAS], nep55_address, new ThinNeo.Hash256(Nep55_1.id_GAS), (decimal)2.1);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;

                //附加鉴证
                tran.attributes = new ThinNeo.Attribute[1];
                tran.attributes[0] = new ThinNeo.Attribute();
                tran.attributes[0].usage = ThinNeo.TransactionAttributeUsage.Script;
                tran.attributes[0].data = scripthash;
            }

            //sign and broadcast
            {//做智能合约的签名
                byte[] n55contract = null;
                {
                    var urlgetscript = Helper.MakeRpcUrl(Nep55_1.api, "getcontractstate", new MyJson.JsonNode_ValueString(Nep55_1.nep55));
                    var resultgetscript = await Helper.HttpGet(urlgetscript);
                    var _json = MyJson.Parse(resultgetscript).AsDict();
                    var _resultv = _json["result"].AsList()[0].AsDict();
                    n55contract = ThinNeo.Helper.HexString2Bytes(_resultv["script"].AsString());
                }
                byte[] iscript = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    sb.EmitPushNumber(0);
                    sb.EmitPushNumber(0);
                    iscript = sb.ToArray();
                }
                tran.AddWitnessScript(n55contract, iscript);
            }
            {//做提款人的签名
                var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
                tran.AddWitness(signdata, pubkey, address);
            }
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);

            ThinNeo.Transaction testde = new ThinNeo.Transaction();
            testde.Deserialize(new System.IO.MemoryStream(trandata));

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(Nep55_1.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            //url = "http://localhost:20332";
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
            var json = MyJson.Parse(result).AsDict();
            if (json.ContainsKey("result"))
            {
                var resultv = json["result"].AsList()[0].AsDict();
                var txid = resultv["txid"].AsString();
                Console.WriteLine("txid=" + txid);
            }

        }
    }
}
