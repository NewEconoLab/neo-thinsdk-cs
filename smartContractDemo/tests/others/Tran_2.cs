using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    //发布智能合约的例子
    class Tran_2 : ITest
    {

        string api = "https://api.nel.group/api/testnet";

        string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        public string Name => "2个地址给2个地址转账";

        public string ID => "tran 2";

        async public Task Demo()
        {
            string wif1 = "KwwJMvfFPcRx2HSgQRPviLv4wPrxRaLk7kfQntkH8kCXzTgAts8t";
            string wif2 = "L2EHemxzCYKxhH81QVwPDwUT5Bd8yBgbPt7GnUFpGuttiiYroRFi";
            string targetAddr1 = "AHDV7M54NHukq8f76QQtBTbrCqKJrBH9UF";
            string targetAddr2 = "AdsNmzKPPG7HfmQpacZ4ixbv9XJHJs2ACz";

            byte[] prikey1 = ThinNeo.Helper.GetPrivateKeyFromWIF(wif1);
            byte[] pubkey1 = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey1);
            string address1 = ThinNeo.Helper.GetAddressFromPublicKey(pubkey1);

            byte[] prikey2 = ThinNeo.Helper.GetPrivateKeyFromWIF(wif2);
            byte[] pubkey2 = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey2);
            string address2 = ThinNeo.Helper.GetAddressFromPublicKey(pubkey2);

            Dictionary<string, List<Utxo>> dir1 = await Helper.GetBalanceByAddress(api, address1);
            Dictionary<string, List<Utxo>> dir2 = await Helper.GetBalanceByAddress(api, address2);


            //拼装交易体
            string[] targetAddrs = new string[2] {targetAddr1, targetAddr2 };
            ThinNeo.Transaction tran = makeTran(dir1, dir2, targetAddrs, new ThinNeo.Hash256(id_GAS), 2);
            tran.version = 0;
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            byte[] msg = tran.GetMessage();
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey1);
            byte[] signdata2 = ThinNeo.Helper.Sign(msg, prikey2);
            tran.AddWitness(signdata, pubkey1, address1);
            tran.AddWitness(signdata2, pubkey2, address2);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
            var result = await Helper.HttpPost(url, postdata);
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
            Console.WriteLine(resJO.ToString());
        }

        //拼交易体
        ThinNeo.Transaction makeTran(Dictionary<string, List<Utxo>> dir_utxos1, Dictionary<string, List<Utxo>> dir_utxos2,string[] targetaddrs, ThinNeo.Hash256 assetid, decimal sendcount)
        {
            if (!dir_utxos1.ContainsKey(assetid.ToString())|| !dir_utxos2.ContainsKey(assetid.ToString()))
                throw new Exception("no enough money.");

            List<Utxo> utxos1 = dir_utxos1[assetid.ToString()];
            List<Utxo> utxos2 = dir_utxos2[assetid.ToString()];

            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            utxos1.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });
            utxos2.Sort((a, b) =>
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
            for (var i = 0; i < utxos1.Count; i++)
            {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = utxos1[i].txid;
                input.index = (ushort)utxos1[i].n;
                list_inputs.Add(input);
                count += utxos1[i].value;
                scraddr = utxos1[i].addr;
                if (count >= (sendcount/2))
                {
                    break;
                }
            }
            decimal count2 = decimal.Zero;
            string scraddr2 = "";
            for (var i = 0; i < utxos2.Count; i++)
            {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = utxos2[i].txid;
                input.index = (ushort)utxos2[i].n;
                list_inputs.Add(input);
                count2 += utxos2[i].value;
                scraddr2 = utxos2[i].addr;
                if (count2 >= (sendcount / 2))
                {
                    break;
                }
            }
            tran.inputs = list_inputs.ToArray();

            if (count+count2 >= sendcount)//输入大于等于输出
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();
                //输出
                if (sendcount > decimal.Zero && targetaddrs.Length > 0)
                {
                    foreach (string targetaddr in targetaddrs)
                    {
                        ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                        output.assetId = assetid;
                        output.value = sendcount/2;
                        output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(targetaddr);
                        list_outputs.Add(output);
                    }
                }

                //找零
                var change1 = count - sendcount/2;
                var change2 = count2 - sendcount/2;
                if (change1 > decimal.Zero)
                {
                    ThinNeo.TransactionOutput outputchange = new ThinNeo.TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(scraddr);
                    outputchange.value = change1;
                    outputchange.assetId = assetid;
                    list_outputs.Add(outputchange);
                }
                if (change2 > decimal.Zero)
                {
                    ThinNeo.TransactionOutput outputchange = new ThinNeo.TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(scraddr2);
                    outputchange.value = change2;
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
