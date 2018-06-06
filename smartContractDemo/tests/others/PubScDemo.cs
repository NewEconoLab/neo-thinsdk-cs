using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    //发布智能合约的例子
    class PubScDemo:ITest
    {

        string api = "https://api.nel.group/api/testnet";

        string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        public string Name => "智能合约发布";

        public string ID => "pubsc";

        async public Task Demo()
        {
            Console.WriteLine("请输入你的wif");
            string  wif = Console.ReadLine();
            if (string.IsNullOrEmpty(wif))
            {
                wif = "";  //这里填你用于支付发布合约消耗的私钥
            }
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(api,address);

            //从文件中读取合约脚本
            byte[] script = System.IO.File.ReadAllBytes("Nep5.5gas_Contract.avm"); //这里填你的合约所在地址
            Console.WriteLine("合约脚本:"+ThinNeo.Helper.Bytes2HexString(script));
            Console.WriteLine("合约脚本hash："+ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.GetScriptHashFromScript(script).data.ToArray().Reverse().ToArray()));
            byte[] parameter__list = ThinNeo.Helper.HexString2Bytes("0710");  //这里填合约入参  例：0610代表（string，[]）
            byte[] return_type = ThinNeo.Helper.HexString2Bytes("05");  //这里填合约的出参
            int need_storage = 1;   
            int need_nep4 = 0;
            int need_canCharge = 4;
            string name = "sgas";
            string version = "1.0";
            string auther = "NEL";
            string email = "0";
            string description = "0";
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                var ss = need_storage | need_nep4 | need_canCharge;
                //倒叙插入数据
                sb.EmitPushString(description);
                sb.EmitPushString(email);
                sb.EmitPushString(auther);
                sb.EmitPushString(version);
                sb.EmitPushString(name);
                sb.EmitPushNumber(need_storage|need_nep4| need_canCharge);
                sb.EmitPushBytes(return_type);
                sb.EmitPushBytes(parameter__list);
                sb.EmitPushBytes(script);
                sb.EmitSysCall("Neo.Contract.Create");

                string scriptPublish = ThinNeo.Helper.Bytes2HexString(sb.ToArray());
                //用ivokescript试运行并得到消耗

                byte[] postdata;
                var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(scriptPublish));
                var result = await Helper.HttpPost(url, postdata);
                //string result = http.Post(api, "invokescript", new MyJson.JsonNode_Array() { new MyJson.JsonNode_ValueString(scriptPublish) },Encoding.UTF8);
                var consume =((( MyJson.Parse(result) as MyJson.JsonNode_Object)["result"] as MyJson.JsonNode_Array)[0] as MyJson.JsonNode_Object)["gas_consumed"].ToString();
                decimal gas_consumed = decimal.Parse(consume);
                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.script = sb.ToArray();

                //Console.WriteLine(ThinNeo.Helper.Bytes2HexString(extdata.script));
                extdata.gas = Math.Ceiling(gas_consumed-10);

                //拼装交易体
                ThinNeo.Transaction tran = makeTran(dir,null, new ThinNeo.Hash256(id_GAS), extdata.gas);
                tran.version = 1;
                tran.extdata = extdata;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);

                //Console.WriteLine("scripthash:"+scripthash);

                url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
                result = await Helper.HttpPost(url, postdata);

                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                Console.WriteLine(resJO.ToString());
            }
        }



        //拼交易体
        ThinNeo.Transaction makeTran(Dictionary<string, List<Utxo>> dir_utxos, string targetaddr, ThinNeo.Hash256 assetid, decimal sendcount)
        {
            if (!dir_utxos.ContainsKey(assetid.ToString()))
                throw new Exception("no enough money.");

            List<Utxo> utxos = dir_utxos[assetid.ToString()];
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            var scraddr = "";
            utxos.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });
            decimal count = decimal.Zero;
            List<ThinNeo.TransactionInput> list_inputs = new List<ThinNeo.TransactionInput>();
            for (var i = 0; i < utxos.Count; i++)
            {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                scraddr = utxos[i].addr;
                if (count >= sendcount)
                {
                    break;
                }
            }
            tran.inputs = list_inputs.ToArray();
            if (count >= sendcount)//输入大于等于输出
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();
                //输出
                if (sendcount > decimal.Zero && targetaddr != null)
                {
                    ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                    output.assetId = assetid;
                    output.value = sendcount;
                    output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(targetaddr);
                    list_outputs.Add(output);
                }

                //找零
                var change = count - sendcount;
                if (change > decimal.Zero)
                {
                    ThinNeo.TransactionOutput outputchange = new ThinNeo.TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(scraddr);
                    outputchange.value = change;
                    outputchange.assetId = assetid;
                    list_outputs.Add(outputchange);

                }
                tran.outputs = list_outputs.ToArray();
            }
            else
            {
                throw new Exception("no enough money.");
            }
            return tran;
        }

    }
}
