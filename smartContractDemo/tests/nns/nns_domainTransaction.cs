using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nns_domainTransaction : ITest
    {
        public string Name => "域名售卖";

        public string ID => "dt";

        private byte[] pubkey;
        private byte[] prikey;
        private string address = "";
        private Hash160 scriptHash;

        private delegate Task testAction();
        private Dictionary<string, testAction> infos = null;
        string[] submenu;


        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_domainTransaction()
        {
            InitMenu();
        }

        private void InitMenu()
        {
            infos = new Dictionary<string, testAction>();
            infos["balanceOf"] = balanceOf;
            infos["getDomainSellingInfo"] = getDomainSellingInfo;
            infos["putaway"] = putaway;
            infos["cancel"] = cancel;
            infos["buy"] = buy;
            infos["setmoneyin"] = setmoneyin;
            infos["getmoneyback"] = getmoneyback;
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


        async Task balanceOf()
        {
            var result = await nns_tools.api_InvokeScript(Config.domainTransactionhash, "balanceOf","(addr)"+address);
            subPrintLine("balanceOf : " + result.value.subItem[0].AsInteger());
        }

        async Task getDomainSellingInfo()
        {
            subPrintLine("    Input full domain just like test.neo :");
            string fulldomain;
            string[] domains;
            try
            {
                fulldomain = Console.ReadLine();
                domains = fulldomain.Split('.');
                if (domains.Length <= 1)
                {
                    subPrintLine("    Input incorret fulldomain");
                    return;
                }
            }
            catch (Exception e)
            {
                subPrintLine("    Input incorret fulldomain");
                return;
            }
            Hash256 rootHash = nns_tools.nameHash(domains[domains.Length - 1]);
            Hash256 fullHash = rootHash;
            for (var i = domains.Length - 2; i == 0; i--)
            {
                fullHash = nns_tools.nameHashSub(rootHash, domains[i]);
                if (i != 0)
                    rootHash = fullHash;
            }
            subPrintLine("calc=" + fullHash.ToString());
            //先查询这个域名的所有者是不是自己
            var info = await nns_tools.api_InvokeScript(Config.domainTransactionhash, "getDomainSellingInfo", "(hex256)" + fullHash.ToString());
            subPrintLine("getDomainSellingInfo hash=" + info.value.subItem[0].subItem[0].AsHash256());
            subPrintLine("getDomainSellingInfo owner=" + ThinNeo.Helper.GetAddressFromScriptHash(info.value.subItem[0].subItem[1].AsHash160()));
            subPrintLine("getDomainSellingInfo price=" + info.value.subItem[0].subItem[2].AsInteger());
        }

        async Task putaway()
        {
            subPrintLine("    Input full domain just like test.neo :");
            string fulldomain;
            string[] domains;
            try
            {
                fulldomain = Console.ReadLine();
                domains = fulldomain.Split('.');
                if (domains.Length <= 1)
                {
                    subPrintLine("    Input incorret fulldomain");
                    return;
                }
            }
            catch (Exception e)
            {
                subPrintLine("    Input incorret fulldomain");
                return;
            }
            Hash256 rootHash = nns_tools.nameHash(domains[domains.Length-1]);
            Hash256 fullHash = rootHash;
            for (var i = domains.Length - 2; i == 0; i--)
            {
                fullHash = nns_tools.nameHashSub(rootHash, domains[i]);
                if (i != 0)
                    rootHash = fullHash;
            }
            subPrintLine("calc=" + fullHash.ToString());
            //先查询这个域名的所有者是不是自己
            var info = await nns_tools.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + fullHash.ToString());
            var sh = info.value.subItem[0].subItem[0].AsHash160();
            if (sh == null)
            {
                subPrintLine(" Domain not exist ");
                return;
            }
            var owner = ThinNeo.Helper.GetAddressFromScriptHash(sh);
            subPrintLine("getinfo owner=" + owner);
            if (owner != address)
            {
                subPrintLine(" Domain is not yours ");
                return;
            }
            subPrintLine("    Input sell price");
            string price;
            try
            {
                price = Console.ReadLine();
                int.Parse(price);
            }
            catch (Exception e)
            {
                subPrintLine("    Input incorret price");
                return;
            }
            var result = await nns_tools.api_SendTransaction(prikey, Config.domainTransactionhash, "putaway","(hex256)" + fullHash.ToString(), "(int)" + price+"00000000");
            subPrintLine("result=" + result);
        }
        async Task cancel()
        {
            subPrintLine("    Input full domain just like test.neo :");
            string fulldomain;
            string[] domains;
            try
            {
                fulldomain = Console.ReadLine();
                domains = fulldomain.Split('.');
                if (domains.Length <= 1)
                {
                    subPrintLine("    Input incorret fulldomain");
                    return;
                }
            }
            catch (Exception e)
            {
                subPrintLine("    Input incorret fulldomain");
                return;
            }
            Hash256 rootHash = nns_tools.nameHash(domains[domains.Length - 1]);
            Hash256 fullHash = rootHash;
            for (var i = domains.Length - 2; i == 0; i--)
            {
                fullHash = nns_tools.nameHashSub(rootHash, domains[i]);
                if (i != 0)
                    rootHash = fullHash;
            }
            subPrintLine("calc=" + fullHash.ToString());
            //先查询这个域名的所有者是不是自己
            var info = await nns_tools.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + fullHash.ToString());
            var sh = info.value.subItem[0].subItem[0].AsHash160();
            if (sh == null)
            {
                subPrintLine(" Domain not exist ");
                return;
            }
            var owner = ThinNeo.Helper.GetAddressFromScriptHash(sh);
            subPrintLine("getinfo owner=" + owner);
            if (owner != ThinNeo.Helper.GetAddressFromScriptHash(Config.domainTransactionhash))
            {
                subPrintLine(" Domain is not dt ");
                return;
            }

            var result = await nns_tools.api_SendTransaction(prikey, Config.domainTransactionhash, "cancel", "(hex256)" + fullHash.ToString());
            subPrintLine("result=" + result);
        }
        async Task buy()
        {
            subPrintLine("    Input full domain just like test.neo :");
            string fulldomain;
            string[] domains;
            try
            {
                fulldomain = Console.ReadLine();
                domains = fulldomain.Split('.');
                if (domains.Length <= 1)
                {
                    subPrintLine("    Input incorret fulldomain");
                    return;
                }
            }
            catch (Exception e)
            {
                subPrintLine("    Input incorret fulldomain");
                return;
            }
            Hash256 rootHash = nns_tools.nameHash(domains[domains.Length - 1]);
            Hash256 fullHash = rootHash;
            for (var i = domains.Length - 2; i == 0; i--)
            {
                fullHash = nns_tools.nameHashSub(rootHash, domains[i]);
                if (i != 0)
                    rootHash = fullHash;
            }
            subPrintLine("calc=" + fullHash.ToString());

            var info = await nns_tools.api_InvokeScript(Config.domainTransactionhash, "getDomainSellingInfo", "(hex256)" + fullHash.ToString());
            var price = info.value.subItem[0].subItem[2].AsInteger();

            var result = await nns_tools.api_SendTransaction(prikey, Config.domainTransactionhash, "buy","(addr)"+address, "(hex256)" + fullHash.ToString());
            subPrintLine("result=" + result);

        }


        async Task setmoneyin()
        {
            string addressto = ThinNeo.Helper.GetAddressFromScriptHash(Config.domainTransactionhash);
            byte[] script;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();

                array.AddArrayValue("(addr)" + address);//from
                array.AddArrayValue("(addr)" + addressto);//to
                array.AddArrayValue("(int)" + 20+"00000000");//value
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("transfer");//参数倒序入
                sb.EmitAppCall(Config.dapp_sgas);//nep5脚本

                sb.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                sb.EmitSysCall("Neo.Transaction.GetHash");
                //把TXID包进Array里
                sb.EmitPushNumber(1);
                sb.Emit(ThinNeo.VM.OpCode.PACK);
                sb.EmitPushString("setmoneyin");
                sb.EmitAppCall(Config.domainTransactionhash);
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = await nns_tools.api_SendTransaction(prikey, script);
            subPrintLine(result);
        }
        async Task getmoneyback()
        {
            var result = await nns_tools.api_SendTransaction(prikey, Config.domainTransactionhash, "getmoneyback", "(string)" + address);
            subPrintLine("result=" + result);
        }
    }
}
