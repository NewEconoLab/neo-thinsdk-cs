using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nep5_test : ITest
    {
        public string Name => "nep5test";

        public string ID => "test";

        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }
        Hash160 sc = new Hash160("0x4ac464f84f50d3f902c2f0ca1658bfaa454ddfbf");

        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            subPrintLine("Get Total Supply for " + this.ID + ":");

            var result = await nns_tools.api_InvokeScript(sc, "totalSupply");
            subPrintLine("Total Supply : " + result.value.subItem[0].AsInteger());

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
                    array.AddArrayValue("(addr)" + address);//from
                    array.AddArrayValue("(str)" + "totalSupply");//to
                    array.AddArrayValue("(int)22");//value
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)transfer"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(sc);
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
            byte[] postdata;//Nep55_1.api
            var url = Helper.MakeRpcUrlPost(nnc_1.api_local, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result2 = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result2);

        }
    }
}
