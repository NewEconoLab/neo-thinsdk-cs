using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    class nnc_3 : ITest
    {
        public string Name => "NNC 从注册器取钱";

        public string ID => "nc 3";

        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nnc_1.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);
            Console.WriteLine("address=" + address);

            string addressto = ThinNeo.Helper.GetAddressFromScriptHash(new ThinNeo.Hash160(nnc_1.sc_sell));
            Console.WriteLine("addressFrom=" + addressto);

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
                    array.AddArrayValue("(addr)" + address);//who
                    array.AddArrayValue("(int)10000000000");//value
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)getmoneyback"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_sell);
                    sb.EmitAppCall(shash);//nep5脚本
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
            var url = Helper.MakeRpcUrlPost(Nep55_1.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
        }

    }
}
