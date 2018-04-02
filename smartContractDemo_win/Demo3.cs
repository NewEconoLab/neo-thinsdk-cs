using System;
using System.Collections.Generic;
using System.Linq;

namespace smartContractDemo_win
{
    public class Demo3
    {
        string api = "https://api.nel.group/api/testnet";

        httpHelper http = new httpHelper();

        public void Demo()
        {
            string nnc = "0x3fccdb91c9bb66ef2446010796feb6ca4ed96b05";
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF("L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW");
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            string toaddr = "APwCdakS1NpJsiq6j9SfvkQFS9ubt347a2";
            string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = GetBalanceByUtxo(address);


            string targeraddr = address;  //Transfer it to yourself. 
            ThinNeo.Transaction tran = makeTran(dir[id_GAS], targeraddr, id_GAS, decimal.Zero);
            tran.type = ThinNeo.TransactionType.InvocationTransaction;

            ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();

            byte[] scriptaddress = ThinNeo.Helper.HexString2Bytes(nnc.Replace("0x", "")).Reverse().ToArray();
            //Parameter inversion 
            MyJson.JsonNode_Array JAParams = new MyJson.JsonNode_Array();
            JAParams.Add(new MyJson.JsonNode_ValueString("(address)" + address));
            JAParams.Add(new MyJson.JsonNode_ValueString("(address)" + toaddr));
            JAParams.Add(new MyJson.JsonNode_ValueString("(integer)" + 1));
            sb.EmitParamJson(JAParams);//Parameter list 
            sb.EmitPushString("transfer");//Method
            sb.EmitAppCall(scriptaddress);  //Asset contract 

            ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
            extdata.script = sb.ToArray();
            extdata.gas = 1;
            tran.extdata = extdata;

            byte[] msg = tran.GetMessage();
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
            tran.AddWitness(signdata, pubkey, address);
            string txid =ThinNeo.Helper.Bytes2HexString(tran.GetHash().Reverse().ToArray());
            byte[] data = tran.GetRawData();
            string scripthash =ThinNeo.Helper.Bytes2HexString(data);

            string response = http.HttpGet(api + "?method=sendrawtransaction&id=1&params=[\"" + scripthash + "\"]");
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(response);
            Console.WriteLine(resJO["result"].ToString());
        }



        //获取地址的utxo来得出地址的资产  
        Dictionary<string, List<Utxo>> GetBalanceByUtxo(string _addr)
        {
            MyJson.JsonNode_Object response = (MyJson.JsonNode_Object)MyJson.Parse(http.HttpGet(api + "?method=getutxo&id=1&params=['" + _addr + "']"));
            MyJson.JsonNode_Array resJA = (MyJson.JsonNode_Array)response["result"];
            Dictionary<string, List<Utxo>> _dir = new Dictionary<string, List<Utxo>>();
            foreach (MyJson.JsonNode_Object j in resJA)
            {
                Utxo utxo = new Utxo(j["addr"].ToString(), j["txid"].ToString(), j["asset"].ToString(),decimal.Parse(j["value"].ToString()),int.Parse(j["n"].ToString()));
                if (_dir.ContainsKey(j["asset"].ToString()))
                {
                    _dir[j["asset"].ToString()].Add(utxo);
                }
                else
                {
                    List<Utxo> l = new List<Utxo>();
                    l.Add(utxo);
                    _dir[j["asset"].ToString()] = l;
                }

            }
            return _dir;
        }


        ThinNeo.Transaction makeTran(List<Utxo> utxos, string targetaddr, string assetid, decimal sendcount)
        {
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
                input.hash =ThinNeo.Helper.HexString2Bytes(utxos[i].txid.Replace("0x", "")).Reverse().ToArray();
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
                if (sendcount > decimal.Zero)
                {
                    ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                    output.assetId = ThinNeo.Helper.HexString2Bytes(assetid.Replace("0x", "")).Reverse().ToArray();
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
                    outputchange.assetId = ThinNeo.Helper.HexString2Bytes(assetid.Replace("0x", "")).Reverse().ToArray();
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




    public class Utxo
    {
        public string addr;
        public string txid;
        public string asset;
        public decimal value;
        public int n;
        public Utxo(string _addr, string _txid, string _asset, decimal _value, int _n)
        {
            this.addr = _addr;
            this.txid = _txid;
            this.asset = _asset;
            this.value = _value;
            this.n = _n;
        }
    }
}
