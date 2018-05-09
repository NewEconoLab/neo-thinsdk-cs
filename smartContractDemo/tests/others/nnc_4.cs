using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nnc_4 : ITest
    {
        public string Name => "充值+拍卖";

        public string ID => "nc 4";

        private byte[] pubkey;
        private byte[] prikey;
        private string address = "";
        private Hash160 scriptHash;
        //public const string sc_nnc = "0xbab964febd82c9629cc583596975f51811f25f47";//nnc 合约地址

        //public const string api = "https://api.nel.group/api/testnet";
        //public const string testwif = "L4ZntdDCocMJi4ozpTw4uTtxtAFNNCP2mX6m3P9CMJN66Dt2YJqP";//"L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";
        public async Task Demo()
        {
            this.prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            this.pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            this.address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            this.scriptHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            var roothash = nns_common.nameHash("sell");
            var fullhash = nns_common.nameHashSub(roothash, "helloworld");

            //得到注册器
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
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

                var who = this.scriptHash;

                //得到拍卖ID
                var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
                var id = info3.value.subItem[0].subItem[0].AsHash256();


                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {

                    var array2 = new MyJson.JsonNode_Array();
                    array2.AddArrayValue("(hex160)" + who.ToString());
                    array2.AddArrayValue("(hex256)" + id.ToString());
                    array2.AddArrayValue("(int)10" + "00000000");
                    sb.EmitParamJson(array2);//参数倒序入
                    sb.EmitPushString("addPrice");//参数倒序入
                    sb.EmitAppCall(new ThinNeo.Hash160(reg_sc));//nep5脚本


                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(addr)" + address);//from
                    array.AddArrayValue("(addr)" + addressto);//to
                    array.AddArrayValue("(int)10" + "00000000");//value
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitPushString("transfer");//参数倒序入
                    sb.EmitAppCall(new ThinNeo.Hash160(nnc_1.sc_nnc));//nep5脚本

                    //这个方法是为了在同一笔交易中转账并充值
                    //当然你也可以分为两笔交易
                    //插入下述两条语句，能得到txid
                    sb.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                    sb.EmitSysCall("Neo.Transaction.GetHash");
                    //把TXID包进Array里
                    sb.EmitPushNumber(1);
                    sb.Emit(ThinNeo.VM.OpCode.PACK);
                    sb.EmitPushString("setmoneyin");
                    sb.EmitAppCall(new ThinNeo.Hash160(reg_sc));

                    script = sb.ToArray();
                }

                tran = Helper.makeTran(dir[Nep55_1.id_GAS], null, new ThinNeo.Hash256(Nep55_1.id_GAS), 0);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;
                idata.gas = 0;
            }
            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(nnc_1.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine(result);
        }
    }
}
