using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nnc : ITest
    {
        public string Name => "nnc 合约测试";

        public string ID => "nnc";

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

        public nnc()
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
            infos["name"] = test_name;
            infos["totalSupply"] = test_totalSupply;
            infos["deploy"] = test_deploy;
            infos["balanceOf"] = test_balanceOf;
            infos["canClaimCount"] = test_canClaimCount;
            infos["getTotalMoney"] = test_getTotalMoney;
            infos["transfer"] = test_transfer;
            infos["claim"] = test_claim;
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

        #region 接口
        async Task test_name()
        {
            var result = await nns_common.api_InvokeScript(Config.dapp_nnc, "name");
            subPrintLine("Name : " + result.value.subItem[0].AsString());
        }

        async Task test_totalSupply()
        {
            var result = await nns_common.api_InvokeScript(Config.dapp_nnc, "totalSupply");
            subPrintLine("Total Supply : " + result.value.subItem[0].AsInteger());
        }

        async Task test_deploy()
        {
            byte[] script;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(int)1");
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("deploy");//参数倒序入
                sb.EmitAppCall(Config.dapp_nnc);//nep5脚本
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = await nns_common.api_SendTransaction(prikey, script);
            subPrintLine(result);
        }

        async Task test_balanceOf()
        {
            subPrintLine("    Input target address :");
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

            var result = await nns_common.api_InvokeScript(Config.dapp_nnc, "balanceOf", "(bytes)" + strhash);
            subPrintLine("Total Supply : " + result.value.subItem[0].AsInteger());
            //var result = await nns_common.api_SendTransaction(prikey,Config.dapp_nnc, "balanceOf", "(bytes)" + strhash);
            //subPrintLine(result); ;
        }


        async Task test_canClaimCount()
        {
            subPrintLine("    Input target address :");
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

            var result = await nns_common.api_InvokeScript(Config.dapp_nnc, "canClaimCount", "(bytes)" + strhash);
            subPrintLine(" = " + result.value.subItem[0].AsInteger() + ""); ;
        }

        async Task test_getTotalMoney()
        {
            subPrintLine("    Input  height :");
            var height = 0;
            try
            {
                height =int.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                height = 0;
            }

            var result = await nns_common.api_InvokeScript(Config.dapp_nnc, "getTotalMoney", "(int)" + height);
            subPrintLine(" = " + result.value.subItem[0].AsInteger() + ""); ;
        }

        async Task test_transfer()
        {
            subPrintLine("Input target address:");
            string addressto = Console.ReadLine();
            addressto = "AeYiwwjiy2nKXoGLDafoTXc1tGvfkTYQcM";
            subPrintLine("Input amount:");
            string amount = Console.ReadLine();

            var result = await nns_common.api_SendTransaction(prikey, Config.dapp_nnc, "transfer",
              "(addr)" + address,
              "(addr)" + addressto,
              "(int)" + amount
              );
            subPrintLine(result);
        }


        async Task test_claim()
        {
            
        }
        #endregion
    }

}
