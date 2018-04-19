using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    class nnc : ITest
    {
        public string Name => "NNC NNC合约测试";

        public string ID => "nnc";
        byte[] prikey;
        public string address;
        byte[] scripthash;
        byte[] pubkey;
        public string[] LIST = {
            "exit",
            "totalSupply",
            "name",
            "symbol",
            "decimals",
            "balanceOf",
            "transfer",
            "transfer_app",
            "deploy",
            "balanceOfDetail",
            "use",
            "use_app",
            "getTXInfo",
            "getBonus",
            "checkBonus",
            "newBonus"
        };

        public delegate void Method(ThinNeo.ScriptBuilder sb);//第一步：定义委托类型

        public async Task Demo()
        {

            for (int i = 0; i < LIST.Length; i++)
            {
                Console.WriteLine(i + ":" + LIST[i]);
            }
            Boolean run = true;
            while (run)
            {
                var line = Convert.ToInt16(Console.ReadLine());

                if (line < 0 || line >= LIST.Length)
                {
                    Console.WriteLine("参数错误！");
                    continue;
                }
                else if (LIST[line] == "exit")
                {
                    run = false;
                    Console.WriteLine("已退出 NNC 测试");
                    continue;
                }

                Console.WriteLine("[begin]" + ID + " : " + LIST[line]);

                prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nnc_1.testwif);
                pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

                Console.WriteLine("address=" + address);
                string script = null;
                byte[] postdata;
                try
                {
                    switch (LIST[line])
                    {
                        case "totalSupply":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "name":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "symbol":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "decimals":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "balanceOf":
                            Invoke(new Method(BalanceOf));
                            break;
                        case "transfer":
                            Transac(new Method(Transfer));
                            break;
                        case "transfer_app":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "deploy":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "balanceOfDetail":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "use":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "use_app":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "getTXInfo":
                            Console.WriteLine("尚未实现");
                            continue;
                            break;
                        case "getBonus":
                            Transac(new Method(GetBonus));
                            break;
                        case "checkBonus":
                            Invoke(new Method(CheckBonus));
                            break;
                        case "newBonus":
                            Invoke(new Method(NewBonus));
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine("[end]" + LIST[line]);
            }
        }

        private async void Transac(Method method)
        {
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
                byte[] script = Builder(method);
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

        private async void Invoke(Method method)
        {
            byte[] data = Builder(method);
            string script = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(nnc_1.api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
        }

        private byte[] Builder(Method method)
        {
            byte[] data = null;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                method(sb);
                data = sb.ToArray();

            }
            return data;
        }

        private void BalanceOf(ThinNeo.ScriptBuilder sb)
        {
            var array = new MyJson.JsonNode_Array();
            array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            sb.EmitParamJson(array);//参数倒序入
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)balanceOf"));//参数倒序入
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            sb.EmitAppCall(shash);//nep5脚本

            sb.EmitParamJson(array);//参数倒序入
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)balanceOf"));//参数倒序入
            sb.EmitAppCall(new ThinNeo.Hash160(nnc_1.sc_sell));//nep5脚本
        }

        private void NewBonus(ThinNeo.ScriptBuilder sb)
        {
            var array = new MyJson.JsonNode_Array();
            array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            sb.EmitParamJson(array);//参数倒序入
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)newBonus"));//参数倒序入
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            sb.EmitAppCall(shash);//nep5脚本
        }

        private void GetBonus(ThinNeo.ScriptBuilder sb)
        {
            var array = new MyJson.JsonNode_Array();
            array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            sb.EmitParamJson(array);//参数倒序入
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)getBonus"));//参数倒序入
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            sb.EmitAppCall(shash);//nep5脚本

            sb.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
            sb.EmitSysCall("Neo.Transaction.GetHash");
            //把TXID包进Array里
            sb.EmitPushNumber(1);
            sb.Emit(ThinNeo.VM.OpCode.PACK);
            sb.EmitPushString("setmoneyin");
            sb.EmitAppCall(new ThinNeo.Hash160(nnc_1.sc_sell));
        }

        private void CheckBonus(ThinNeo.ScriptBuilder sb)
        {
            var array = new MyJson.JsonNode_Array();
            array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            sb.EmitParamJson(array);//参数倒序入
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)checkBonus"));//参数倒序入
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            sb.EmitAppCall(shash);//nep5脚本
        }
        private void Transfer(ThinNeo.ScriptBuilder sb)
        {
            string addressto = ThinNeo.Helper.GetAddressFromScriptHash(new ThinNeo.Hash160(nnc_1.sc_sell));

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
            sb.EmitAppCall(new ThinNeo.Hash160(nnc_1.sc_sell));
        }
    }
}
