using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nnc_2 : ITest
    {
        public string Name => "NNC 向注册器充值";

        public string ID => "nc 2";

        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nnc_1.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);


            //得到注册器
            var info_reg = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash("sell").ToString());
            var reg_sc = new Hash160(info_reg.value.subItem[0].subItem[1].data);
            Console.WriteLine("reg=" + reg_sc.ToString());

            Console.WriteLine("address=" + address);

            string addressto = ThinNeo.Helper.GetAddressFromScriptHash(reg_sc);
            Console.WriteLine("addressto=" + addressto);

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
                    array.AddArrayValue("(addr)" + addressto);//to
                    array.AddArrayValue("(int)10000000000");//value
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)transfer"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
                    sb.EmitAppCall(shash);//nep5脚本

                    //这个方法是为了在同一笔交易中转账并充值
                    //当然你也可以分为两笔交易
                    //插入下述两条语句，能得到txid
                    sb.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                    sb.EmitSysCall("Neo.Transaction.GetHash");
                    //把TXID包进Array里
                    sb.EmitPushNumber(1);
                    sb.Emit(ThinNeo.VM.OpCode.PACK);
                    sb.EmitPushString("setmoneyin");
                    sb.EmitAppCall(reg_sc);
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
