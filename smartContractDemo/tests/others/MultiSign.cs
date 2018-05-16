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

        private async Task test_multisign()
        {
            Console.WriteLine("Input wif");
            var wif0 = "L2ME3NL8XgWLa6XVVzCJyccPw3C7bnqHzWhtfdPaeZzzdX8MJSkj";//Console.ReadLine();
            Console.WriteLine("Input wif");
            var wif1 = "KwuezVnxhfUGiex7HM4ttrKBF4pTQRREkVmL1PW91gBZTRtsrLm9";// Console.ReadLine();

            var prikey0 = ThinNeo.Helper.GetPrivateKeyFromWIF(wif0);
            var prikey1 = ThinNeo.Helper.GetPrivateKeyFromWIF(wif1);

            var result = await MultiSign.api_SendTransaction(prikey0, prikey1, Config.dapp_multisign, "transfer",
              "(int)1000"
              );
        }



        public static async Task<string> api_SendTransaction(byte[] prikey0, byte[] prikey1, Hash160 schash, string methodname, params string[] subparam)
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
                    Console.WriteLine(ThinNeo.Helper.Bytes2HexString(data));
                }
            }

            return await MultiSign.api_SendTransaction(prikey0, prikey1, data);
        }


        /// <summary>
        /// 重载交易构造方法，对于复杂交易传入脚本
        /// </summary>
        /// <param name="prikey">私钥</param>
        /// <param name="script">交易脚本</param>
        /// <returns></returns>
        public static async Task<string> api_SendTransaction(byte[] prikey0, byte[] prikey1, byte[] script)
        {
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey0);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            byte[] pubkey1 = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey1);
            string address1 = ThinNeo.Helper.GetAddressFromPublicKey(pubkey1);


            //获取地址0的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Config.api, address);
            if (dir.ContainsKey(Nep55_1.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return null;
            }

            //获取地址1的资产列表
            Dictionary<string, List<Utxo>> dir1 = await Helper.GetBalanceByAddress(Config.api, address1);
            if (dir1.ContainsKey(Nep55_1.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return null;
            }

            List<TransactionInput> list_inputs = new List<TransactionInput>();
            List<TransactionOutput> list_outputs = new List<TransactionOutput>();

            //MakeTran
            Transaction tran = new Transaction();
            tran.type = TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];

            TransactionInput input = new TransactionInput();
            input.hash = dir[Nep55_1.id_GAS][0].txid;
            input.index = (ushort)dir[Nep55_1.id_GAS][0].n;
            list_inputs.Add(input);

            TransactionInput input1 = new TransactionInput();
            input1.hash = dir1[Nep55_1.id_GAS][0].txid;
            input1.index = (ushort)dir1[Nep55_1.id_GAS][0].n;
            list_inputs.Add(input1);


            tran.inputs = list_inputs.ToArray();



            TransactionOutput outputchange = new TransactionOutput();
            outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);
            outputchange.value = dir[Nep55_1.id_GAS][0].value;
            outputchange.assetId = new Hash256(Nep55_1.id_GAS);
            list_outputs.Add(outputchange);

            TransactionOutput output = new TransactionOutput();
            output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(address1);
            output.value = dir1[Nep55_1.id_GAS][0].value;
            output.assetId = new Hash256(Nep55_1.id_GAS);
            list_outputs.Add(output);


            tran.outputs = list_outputs.ToArray();



            byte[] data = script;
            tran.type = TransactionType.InvocationTransaction;
            var idata = new InvokeTransData();
            tran.extdata = idata;
            idata.script = data;
            idata.gas = 0;

            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey0);

            tran.AddWitness(signdata, pubkey, address);

            
            var signdata1 = ThinNeo.Helper.Sign(tran.GetMessage(), prikey1);

            tran.AddWitness(signdata1, pubkey1, address1);

            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(Config.api_local, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            return result;
        }
    }
}
