using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class SGAS : ITest
    {
        public string Name => "SGAS 合约测试";

        public string ID => "SGAS";

        public delegate Task testAction();
        public Dictionary<string, testAction> infos = null;// = new Dictionary<string, testAction>();
        string[] submenu;
        private string testkey = nns_common.testwif;
        private byte[] pubkey;
        private byte[] prikey;
        private string address = "";
        public static readonly Hash160 sgas = new Hash160("0xbc0fdb1c1b84601a9c66594cb481b684b90e05bb");//sgas 合约地址
        private Hash160 scriptHash;

        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public SGAS()
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

            infos["totalSupply"] = test_totalSupply;
            infos["name"] = test_name;
            infos["symbol"] = test_symbol;
            infos["decimals"] = test_decimals;
            infos["balanceOf"] = test_BalanceOf;
            infos["transfer"] = test_Transfer;
            infos["transfer_app"] = test_not_implement_yet;
            infos["deploy"] = test_not_implement_yet;
            infos["balanceOfDetail"] = test_not_implement_yet;
            infos["use"] = test_not_implement_yet;
            infos["use_app"] = test_not_implement_yet;
            infos["getTXInfo"] = test_not_implement_yet;
            infos["getBonus"] = test_not_implement_yet;
            infos["checkBonus"] = test_CheckBonus;
            infos["newBonus"] = test_NewBonus;
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

        async Task test_totalSupply()
        {
            subPrintLine("Get Total Supply for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(sgas, "totalSupply");
            subPrintLine("Total Supply : " + result.value.subItem[0].AsInteger());
        }

        async Task test_name()
        {
            subPrintLine("Get Name for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(sgas, "name");
            subPrintLine("Name : " + result.value.subItem[0].AsString());
        }

        async Task test_symbol()
        {
            subPrintLine("Get Symbol for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(sgas, "symbol");
            subPrintLine("Symbol : " + result.value.subItem[0].AsString());
        }

        async Task test_decimals()
        {
            subPrintLine("Get decimals for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(sgas, "decimals");
            subPrintLine("decimals : " + result.value.subItem[0].AsInteger());
        }


        async Task test_BalanceOf()
        {
            Console.WriteLine("    Input target address (" + this.address + "):");
            string addr;
            try
            {
                addr = Console.ReadLine();
                if (addr.Length < 34)
                {
                    addr = this.address;
                }
            }
            catch (Exception e)
            {
                addr = this.address;
            }

            byte[] hash = ThinNeo.Helper.GetPublicKeyHashFromAddress(addr);
            string strhash = ThinNeo.Helper.Bytes2HexString(hash);

            var result = await nns_common.api_InvokeScript(sgas, "balanceOf", "(bytes)" + strhash);
            Console.Write("    Balance of ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(addr);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" = "+result.value.subItem[0].AsInteger() + "");
        }

        async Task test_Transfer()
        {
            Console.WriteLine("Input target address:");
            string addressto = Console.ReadLine();
            Console.WriteLine("Input amount:");
            string amount = Console.ReadLine();

            var result = await nns_common.api_SendTransaction(prikey, sgas, "transfer",
              "(addr)" + address,
              "(addr)" + addressto,
              "(int)" + amount
              );
            subPrintLine(result);
        }

        #endregion


        async Task test_NewBonus()
        {
            //var array = new MyJson.JsonNode_Array();
            //array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            //sb.EmitParamJson(array);//参数倒序入
            //sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)newBonus"));//参数倒序入
            //ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            //sb.EmitAppCall(shash);//nep5脚本
        }

        async Task test_GetBonus(ThinNeo.ScriptBuilder sb)
        {
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);

            var result = await nns_common.api_SendTransaction(shash, prikey, "getBonus", "(bytes)" + ThinNeo.Helper.Bytes2HexString(scriptHash));
            subPrintLine(result);
        }

        async Task test_CheckBonus()
        {
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            var result = await nns_common.api_InvokeScript(shash, "checkBonus", "(bytes)" + ThinNeo.Helper.Bytes2HexString(scriptHash));
            //subPrintLine(result);
        }

       

    }

}
