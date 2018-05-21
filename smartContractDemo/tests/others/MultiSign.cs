using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    public class MultiSign : ITest
    {
        public string Name => "多签";

        public string ID => "multisign";

        public delegate Task testAction();
        public Dictionary<string, testAction> infos = null;// = new Dictionary<string, testAction>();
        string[] submenu;

        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public MultiSign()
        {
            this.initManu();
        }


        private void initManu()
        {
            infos = new Dictionary<string, testAction>();

            infos["test_addr"] = test_addr;
            infos["test_multisign"] = test_multisign;
            infos["test_buildMultiSign"] = test_buildMultiSign;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }


        public async Task Demo()
        {
            showMenu();
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


        private async Task test_addr()
        {
            var pubkey = Console.ReadLine();

            var hex = ThinNeo.Helper.HexString2Bytes(pubkey);
            foreach (var item in hex)
            {
                Console.Write($"{item},");
            }
            Console.ReadLine();

        }

        private async Task test_buildMultiSign()
        {

            Console.WriteLine("输入公钥数");
            var pubkeyLen = long.Parse(Console.ReadLine());

            string[] pubkeys = new string[pubkeyLen];

            Console.WriteLine("输入最小签名数");
            var verifyLen = long.Parse(Console.ReadLine());
            for(int i = 0; i < pubkeyLen; i++)
            {
                pubkeys[i] = Console.ReadLine();
            }

            byte[] data;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitPushNumber(verifyLen);
                for(int m = (int)pubkeyLen - 1; m >= 0; m--)
                {
                    sb.EmitPushBytes(ThinNeo.Helper.HexString2Bytes(pubkeys[m]));
                }
                sb.EmitPushNumber(pubkeyLen);
                sb.Emit(ThinNeo.VM.OpCode.CHECKMULTISIG);
                data = sb.ToArray();
                //Console.WriteLine(ThinNeo.Helper.Bytes2HexString(data));
            }

            Config.Log("多签合约哈希: 0x",ConsoleColor.White);
            var hash = ThinNeo.Helper.GetScriptHashFromScript(data);
            Config.LogLn(ThinNeo.Helper.Bytes2HexString(hash), ConsoleColor.Yellow);

            Config.Log("多签合约地址:", ConsoleColor.White);
            Config.LogLn(ThinNeo.Helper.GetAddressFromScriptHash(hash), ConsoleColor.Yellow);

        }

        private async Task test_multisign()
        {
            Console.WriteLine("Input number of Wif");
            var num = int.Parse(Console.ReadLine());

            var prikeys = new List<byte[]>();
            for(int i = 0; i < num; i++)
            {
                Console.WriteLine("Input wif");
                var wif = Console.ReadLine();
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
                prikeys.Add(prikey);
            }
            var result = await api_SendTransaction(prikeys, Config.dapp_multisign, "transfer",
              "(int)1000"
              );
        }



        public static async Task<string> api_SendTransaction(List<byte[]> prikeys, Hash160 schash, string methodname, params string[] subparam)
        {
            byte[] data = null;
            //MakeTran
            ThinNeo.Transaction tran = null;
            {

                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                    for (var i = 0; i < subparam.Length; i++)
                    {
                        array.AddArrayValue(subparam[i]);
                    }
                    sb.EmitParamJson(array);
                    sb.EmitPushString(methodname);
                    sb.EmitAppCall(schash);
                    data = sb.ToArray();
                    //Console.WriteLine(ThinNeo.Helper.Bytes2HexString(data));
                }
            }

            return await api_SendTransaction(prikeys, data);
        }


        /// <summary>
        /// 重载交易构造方法，对于复杂交易传入脚本
        /// </summary>
        /// <param name="prikey">私钥</param>
        /// <param name="script">交易脚本</param>
        /// <returns></returns>
        public static async Task<string> api_SendTransaction(List<byte[]> prikeys, byte[] script)
        {

            List<TransactionInput> list_inputs = new List<TransactionInput>();
            List<TransactionOutput> list_outputs = new List<TransactionOutput>();

            foreach (var prikey in prikeys)
            {
                byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Config.api, address);
                if (dir.ContainsKey(Nep55_1.id_GAS) == false)
                {
                    Console.WriteLine("no gas");
                    return null;
                }

                TransactionInput input = new TransactionInput();
                input.hash = dir[Nep55_1.id_GAS][0].txid;
                input.index = (ushort)dir[Nep55_1.id_GAS][0].n;
                list_inputs.Add(input);


                TransactionOutput outputchange = new TransactionOutput();
                outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);
                outputchange.value = dir[Nep55_1.id_GAS][0].value;
                outputchange.assetId = new Hash256(Nep55_1.id_GAS);
                list_outputs.Add(outputchange);
            }

            //MakeTran
            Transaction tran = new Transaction();
            tran.type = TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            tran.inputs = list_inputs.ToArray();
            tran.outputs = list_outputs.ToArray();

            byte[] data = script;
            tran.type = TransactionType.InvocationTransaction;
            var idata = new InvokeTransData();
            tran.extdata = idata;
            idata.script = data;
            idata.gas = 0;


            foreach(var prikey in prikeys)
            {
                //sign and broadcast
                var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
                byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                tran.AddWitness(signdata, pubkey, address);
            }
            
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(Config.api_local, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            return result;
        }
    }
}
