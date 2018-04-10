using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace smartContractDemo
{
    public class Nep55_4 : ITest
    {
        public string Name => "Nep5.5 取回Gas";

        public string ID => "N5 4";
        async public Task Demo()
        {
            var lasthash = Nep55_3.lasttxid;
            if (lasthash == null)
            {
                Console.WriteLine("你还没有正确执行N5 3");
                return;
            }
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Nep55_1.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            var scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

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
            List<Utxo> newlist = new List<Utxo>();
            foreach (var utxo in dir[Nep55_1.id_GAS])
            {
                if (utxo.n == 0 && utxo.txid.Equals(lasthash))
                    newlist.Add(utxo);
            }
            if (newlist.Count == 0)
            {
                Console.WriteLine("找不到要使用的UTXO");
                return;
            }

            {//检查utxo
                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(hex256)" + newlist[0].txid.ToString());
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)getRefundTarget"));//参数倒序入
                    var shash = new ThinNeo.Hash160(Nep55_1.nep55);
                    sb.EmitAppCall(shash);//nep5脚本
                    script = sb.ToArray();
                }
                var urlCheckUTXO = Helper.MakeRpcUrl(Nep55_1.api, "invokescript", new MyJson.JsonNode_ValueString(ThinNeo.Helper.Bytes2HexString(script)));
                string resultCheckUTXO = await Helper.HttpGet(urlCheckUTXO);
                var jsonCU = MyJson.Parse(resultCheckUTXO);
                var stack = jsonCU.AsDict()["result"].AsList()[0].AsDict()["stack"].AsList()[0].AsDict();
                var value = stack["value"].AsString();
                if (value.Length == 0)//未标记的UTXO，不能使用
                {
                    Console.WriteLine("这个utxo没有标记");
                    return;
                }
                var hash = new ThinNeo.Hash160(ThinNeo.Helper.HexString2Bytes(value));
                if (hash.ToString()!= scripthash.ToString())
                {
                    Console.WriteLine("这个utxo不是标记给你用的");
                    return;
                }
            }


            ThinNeo.Transaction tran = Helper.makeTran(newlist, address, new ThinNeo.Hash256(Nep55_1.id_GAS), newlist[0].value);
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;


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
           

            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);

            ThinNeo.Transaction testde = new ThinNeo.Transaction();
            testde.Deserialize(new System.IO.MemoryStream(trandata));

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(Nep55_1.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));


            //bug
            //sendraw api 有bug，所以先加这个
            //url = "http://localhost:20332";


            string poststr = System.Text.Encoding.UTF8.GetString(postdata);
            Console.WriteLine("-----post info begin----");
            Console.WriteLine(poststr);
            Console.WriteLine("-----post info end----");

            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
            var json = MyJson.Parse(result).AsDict();
            if (json.ContainsKey("result"))
            {
                bool bSucc = false;
                if (json["result"].type == MyJson.jsontype.Value_Number)
                {
                    bSucc = json["result"].AsBool();
                    Console.WriteLine("cli=" + json["result"].ToString());
                }
                else
                {
                    var resultv = json["result"].AsList()[0].AsDict();
                    var txid = resultv["txid"].AsString();
                    bSucc = txid.Length > 0;
                    Console.WriteLine("txid=" + txid);
                }
                if (bSucc)
                {
                    Nep55_1.lastNep5Tran = tran.GetHash();
                    Console.WriteLine("besucc txid=" + tran.GetHash().ToString());
                }
            }

        }
    }
}
