using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class CoinPool : ITest
    {
        public string Name => "CoinPool 合约测试";

        public string ID => "CoinPool";

        public delegate Task testAction();
        public Dictionary<string, testAction> infos = null;// = new Dictionary<string, testAction>();
        string[] submenu;
        private string testkey = nns_common.testwif;
        private byte[] pubkey;
        private byte[] prikey;
        private string address = "";
        public static readonly Hash160 coinpool = new Hash160("0x0ea2fa310c7875fdb502f34ec674af560f8e6061");//coinpool 合约地址
        private Hash160 scriptHash;

        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public CoinPool()
        {
            this.initManu();
        }

        private void initAccount()
        {
            this.prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(this.testkey);
            this.pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            this.scriptHash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            this.address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            Console.WriteLine("\n************************************* -- Current Account -- **************************************\n");
            subPrintLine("Address    : " + this.address);
            subPrintLine("Prikey     : " + ThinNeo.Helper.Bytes2HexString(prikey));
            subPrintLine("Pubkey     : " + ThinNeo.Helper.Bytes2HexString(pubkey));
            subPrintLine("ScriptHash : " + this.scriptHash.ToString());
            Console.WriteLine("\n**************************************************************************************************\n");
        }

        private void initManu()
        {
            infos = new Dictionary<string, testAction>();

            infos["setSGASIn"] = test_setSGASIn;
            infos["countSGASOnBlock"] = test_countSGASOnBlock;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }


        public async Task Demo()
        {
            showMenu();

            prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nnc_1.testwif);
            pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            scriptHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            while (true)
            {
                var line = Console.ReadLine().Replace(" ", "").ToLower();
                if (line == "?" || line == "？")
                {
                    showMenu();
                }
                else if (line == "")
                {
                    continue;
                }
                else if (line == "0")
                {
                    return;
                }
                else//get .test's info
                {
                    var id = int.Parse(line) - 1;
                    var key = submenu[id];
                    subPrintLine("[begin]" + key);
                    try
                    {
                        await infos[key]();
                    }
                    catch (Exception err)
                    {
                        subPrintLine(err.Message);
                    }
                    subPrintLine("[end]" + key);
                }
            }
        }

        void showMenu()
        {
            for (var i = 0; i < submenu.Length; i++)
            {
                var key = submenu[i];
                subPrintLine((i + 1).ToString() + ":" + key);
            }
            subPrintLine("0:exit");
            subPrintLine("?:show this");
        }

        async Task test_not_implement_yet()
        {
            subPrintLine("尚未实现");
        }

        #region nep5标准接口


        async Task test_setSGASIn()
        {
            string addressto = ThinNeo.Helper.GetAddressFromScriptHash(coinpool);
            Console.WriteLine("addressto=" + addressto);

            Console.WriteLine("Input amount:");
            string amount = Console.ReadLine();
            byte[] script;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();

                array.AddArrayValue("(addr)" + address);//from
                array.AddArrayValue("(addr)" + addressto);//to
                array.AddArrayValue("(int)" + amount);//value
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("transfer");//参数倒序入
                ThinNeo.Hash160 shash = new ThinNeo.Hash160(SGAS.sgas);
                sb.EmitAppCall(shash);//nep5脚本

                ////这个方法是为了在同一笔交易中转账并充值
                ////当然你也可以分为两笔交易
                ////插入下述两条语句，能得到txid
                sb.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                sb.EmitSysCall("Neo.Transaction.GetHash");
                //把TXID包进Array里
                sb.EmitPushNumber(1);
                sb.Emit(ThinNeo.VM.OpCode.PACK);
                sb.EmitPushString("setSGASIn");
                sb.EmitAppCall(coinpool);
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = await nns_common.api_SendTransaction(prikey, script);
            subPrintLine(result);
        }

        async Task test_countSGASOnBlock()
        {
            subPrintLine("Get SGAS count for " + this.ID + ":");

            Console.WriteLine("Input Height (" + 1439000 + "):");
            string heitht = Console.ReadLine();
            if (heitht.Length < 4)
            {
                heitht = "1439000";
            }
            var result = await nns_common.api_InvokeScript(coinpool, "countSGASOnBlock", "(int)" + heitht);
            Console.Write("Count:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(result.value.subItem[0].AsInteger() + "");
            Console.ForegroundColor = ConsoleColor.White;
        }
        #endregion
    }

}
