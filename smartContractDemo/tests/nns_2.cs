using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    class nns_2 : ITest
    {
        public string Name => "getownerinfo";

        public string ID => "ns 2";
        public const string nns_domaincenter = "0x2172f8d5b17c2d45fa3ff58dee8e8a4c3f51ef72";

        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nns_1.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);
            Console.WriteLine("address=" + address);

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Nep55_1.api, address);
            if (dir.ContainsKey(Nep55_1.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return;
            }
            //MakeTran
            ThinNeo.Transaction tran = null;
            {

                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    var rootHash = ThinNeo.Helper.nameHash("test");
                    var nameHash = ThinNeo.Helper.nameHashSub(rootHash,"neodunn");
                    array.AddArrayValue("(str)" + Encoding.UTF8.GetString(nameHash));
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)getOwnerInfo"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(nns_domaincenter);
                    sb.EmitAppCall(shash);//
                    script = sb.ToArray();
                }

                tran = Helper.makeTran(dir[Nep55_1.id_GAS], address, new ThinNeo.Hash256(Nep55_1.id_GAS), 0);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;


            }
            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost("http://localhost:20332", "invokescript", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("invokescript得到的结果是：" + result);
            url = Helper.MakeRpcUrlPost("http://localhost:20332", "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("sendrawtransaction得到的结果是：" + result);
        }

    }
}
