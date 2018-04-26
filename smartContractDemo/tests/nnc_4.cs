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

        //public const string sc_nnc = "0xbab964febd82c9629cc583596975f51811f25f47";//nnc 合约地址

        //public const string api = "https://api.nel.group/api/testnet";
        //public const string testwif = "L4ZntdDCocMJi4ozpTw4uTtxtAFNNCP2mX6m3P9CMJN66Dt2YJqP";//"L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";
        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nns_common.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            //得到注册器
            var info_reg = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash("alibaba").ToString());
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
   
                var who = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
                //  var result = await nns_common.api_SendTransaction(prikey, reg_sc, "addPrice",
                //"(hex160)" + who.ToString(),//参数1 who
                //"(hex256)" + id.ToString(),//参数2 交易id
                //"(int)1000000000"//参数3，加价多少
                //);

                var roothash = nns_common.nameHash("sell");
                var fullhash = nns_common.nameHashSub(roothash, "alibaba");

                //得到注册器
                var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
                var reg = new Hash160(info.value.subItem[0].subItem[1].data);
 
                //得到拍卖ID
                var info3 = await nns_common.api_InvokeScript(reg, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
                var id = info3.value.subItem[0].subItem[0].AsHash256();


                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array2 = new MyJson.JsonNode_Array();
                    array2.AddArrayValue("(hex160)" + who.ToString());
                    array2.AddArrayValue("(hex256)" + id.ToString());
                    array2.AddArrayValue("(int)10" + "00000000");
                    sb.EmitParamJson(array2);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)addPrice"));//参数倒序入
                    ThinNeo.Hash160 shash2 = new ThinNeo.Hash160(reg_sc);
                    sb.EmitAppCall(shash2);//nep5脚本

                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(addr)" + address);//from
                    array.AddArrayValue("(addr)" + addressto);//to
                    array.AddArrayValue("(int)10"+ "00000000");//value
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
