using smartContractDemo.tests;
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
        private byte[] pubkey;
        private byte[] prikey;
        private string address = "";
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
            this.prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
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
            this.submenu = new List<string>(infos.Keys).ToArray();
        }


        public async Task Demo()
        {
            showMenu();

            prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
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

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "totalSupply");
            subPrintLine("Total Supply : " + result.value.subItem[0].AsInteger());
        }

        async Task test_name()
        {
            subPrintLine("Get Name for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "name");
            subPrintLine("Name : " + result.value.subItem[0].AsString());
        }

        async Task test_symbol()
        {
            subPrintLine("Get Symbol for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "symbol");
            subPrintLine("Symbol : " + result.value.subItem[0].AsString());
        }

        async Task test_decimals()
        {
            subPrintLine("Get decimals for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "decimals");
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

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "balanceOf", "(bytes)" + strhash);
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

            var result = await nns_common.api_SendTransaction(prikey, Config.dapp_sgas, "transfer",
              "(addr)" + address,
              "(addr)" + addressto,
              "(int)" + amount
              );
            subPrintLine(result);
        }

        #endregion

    }

}
