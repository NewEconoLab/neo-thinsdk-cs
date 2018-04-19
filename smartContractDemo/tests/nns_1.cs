using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    class nns_1 : ITest
    {
        public string Name => "先到先得注册器";

        public string ID => "ns 1";
        public const string nns_fifo = "0x9a20a91392d90f468fb18dd3070754bec8e573e6";

        public const string testwif = "L2EHemxzCYKxhH81QVwPDwUT5Bd8yBgbPt7GnUFpGuttiiYroRFi";

        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
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
                    var rootHash = new ThinNeo.Hash256(ThinNeo.Helper.nameHash("test"));

                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(addr)" + address);
                    array.AddArrayValue("(hex256)" + rootHash);
                    array.AddArrayValue("(str)neodunn");
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)requestSubDomain"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(nns_fifo);
                    sb.EmitAppCall(shash);//
                    script = sb.ToArray();
                }

                tran = Helper.makeTran(dir[Nep55_1.id_GAS], null, new ThinNeo.Hash256(Nep55_1.id_GAS), 20);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;
                idata.gas = 20; 


            }
            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost("http://localhost:20332", "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("sendrawtransaction得到的结果是：" + result);
        }

    }
}
