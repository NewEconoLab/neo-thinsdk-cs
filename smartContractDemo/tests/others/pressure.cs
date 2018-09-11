using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    class pressure : ITest
    {

        string api = "https://api.nel.group/api/testnet";

        string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        public string Name => "轮询";

        public string ID => "pressure";

        async public Task Demo()
        {
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(api, "getblock", out postdata, new MyJson.JsonNode_ValueNumber(1000));
            for (var i = 0; i < 1000; i++)
            {
                Console.WriteLine("i:"+i);
                var result = await Helper.HttpPost(url, postdata);
                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                Console.WriteLine(resJO.ToString());
            }
        }

        //拼交易体
        ThinNeo.Transaction makeTran(Dictionary<string, List<Utxo>> dir_utxos, string[] targetaddrs, ThinNeo.Hash256 assetid, decimal sendcount)
        {
            if (!dir_utxos.ContainsKey(assetid.ToString()))
                throw new Exception("no enough money.");

            List<Utxo> utxos = dir_utxos[assetid.ToString()];

            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
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
            string scraddr = "";
            List<ThinNeo.TransactionInput> list_inputs = new List<ThinNeo.TransactionInput>();
            for (var i = 0; i < utxos.Count; i++)
            {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                scraddr = utxos[i].addr;
                if (count >= (sendcount))
                {
                    break;
                }
            }

            tran.inputs = list_inputs.ToArray();

            if (count >= sendcount)//输入大于等于输出
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();
                //输出
                if (sendcount > decimal.Zero && targetaddrs.Length > 0)
                {
                    foreach (string targetaddr in targetaddrs)
                    {
                        ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                        output.assetId = assetid;
                        output.value = sendcount;
                        output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(targetaddr);
                        list_outputs.Add(output);
                    }
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
