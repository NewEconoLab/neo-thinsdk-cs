using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smartContractDemo
{
    public class SCDemo3:ITest
    {
        string api = "https://api.nel.group/api/testnet";

        public string Name => "智能合约3连 3/3";

        public string ID => "SC3/3";

        async public Task Demo()
        {
            string nnc = "0x3fccdb91c9bb66ef2446010796feb6ca4ed96b05";
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF("L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW");
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            string toaddr = "APwCdakS1NpJsiq6j9SfvkQFS9ubt347a2";
            string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(api,address);


            string targeraddr = address;  //Transfer it to yourself. 
            ThinNeo.Transaction tran = Helper.makeTran(dir[id_GAS], targeraddr, id_GAS, decimal.Zero);
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
            string txid = ThinNeo.Helper.Bytes2HexString(tran.GetHash().Reverse().ToArray());
            byte[] data = tran.GetRawData();
            string scripthash = ThinNeo.Helper.Bytes2HexString(data);

            string response = await Helper.HttpGet(api + "?method=sendrawtransaction&id=1&params=[\"" + scripthash + "\"]");
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(response);
            Console.WriteLine(resJO["result"].ToString());
        }





    }

}
