using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nns_common : ITest
    {
        public string Name => "所有者分配子域名";

        public string ID => "nns common";



        #region menuandlog
        public delegate Task testAction();
        public Dictionary<string, testAction> infos = new Dictionary<string, testAction>();
        string[] submenu;
        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_common()
        {
            infos["get domain info"] = test_get_domain_info;
            infos["request domain"] = test_request_domain;
            infos["set register"] = test_set_register;
            infos["set resolver"] = test_set_resolver;
            infos["owner change [xxx].[yyy]"] = test_owner_change;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion

        #region testarea
        async Task test_get_domain_info()
        {
            subPrintLine("get domain info:input domain like a.b.c~");
            var domain = Console.ReadLine();

            var r = await nns_tools.api_InvokeScript(Config.sc_nns, "nameHash", "(string)" + domain);
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            string[] strs = domain.Split('.');
            var mh = nns_tools.nameHash(strs[strs.Length - 1]);
            for (var i = strs.Length - 2; i >= 0; i--)
            {
                mh = nns_tools.nameHashSub(mh,strs[i]);
            }
            subPrintLine("calc=" + mh.ToString());
            var info = await nns_tools.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + mh.ToString());
            subPrintLine("getinfo owner=" + ThinNeo.Helper.GetAddressFromScriptHash(info.value.subItem[0].subItem[0].AsHash160()));
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[3].AsInteger());
            subPrintLine("getinfo parentOwner=" +ThinNeo.Helper.GetAddressFromScriptHash(info.value.subItem[0].subItem[4].AsHash160()));
            subPrintLine("getinfo domain=" + info.value.subItem[0].subItem[5].AsString());
            subPrintLine("getinfo parentHash=" + info.value.subItem[0].subItem[6].AsHash256());
            subPrintLine("getinfo root=" + info.value.subItem[0].subItem[7].AsInteger());
        }

        async Task test_request_domain()
        {
            subPrintLine("request domain:input like a.b.c~");
            var strs = Console.ReadLine().Split(".");
            var mh = nns_tools.nameHash(strs[strs.Length - 1]);
            for (var i = strs.Length - 2; i > 0; i--)
            {
                mh = nns_tools.nameHashSub(mh, strs[i]);
            }
            var domain = strs[0];
            subPrintLine("calc=" + mh.ToString());
            subPrintLine("set domain to who");
            var newowner = Console.ReadLine();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            var info = await nns_tools.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + mh);
            var _result = info.value.subItem[0];
            var test_reg = _result.subItem[1].AsHash160();//根域名注册器必须获取，写死不行

            var result = await nns_tools.api_SendTransaction(prikey, test_reg, "SetDomainOwner",
                "(addr)" + newowner,
                "(hex256)" + mh,
                "(str)" + domain);

            subPrintLine("sendrawtransaction得到的结果是：" + result);
        }

        async Task test_set_register()
        {
            subPrintLine("get domain info:input domain like a.b.c~");
            var domain = Console.ReadLine();
            string[] strs = domain.Split('.');
            var mh = nns_tools.nameHash(strs[strs.Length - 1]);
            for (var i = strs.Length - 2; i >= 0; i--)
            {
                mh = nns_tools.nameHashSub(mh, strs[i]);
            }
            subPrintLine("mh=" + mh.ToString());

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            Hash160 hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var register = new Hash160("0x53e26c4b30f2be5bde66ec2c33cadafb93bc9318");
            var result = await nns_tools.api_SendTransaction(prikey, Config.sc_nns, "owner_SetRegister",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + mh.ToString(),//参数2 域名fullhash
               "(hex160)" + register.ToString()//参数3 解析器地址
               );
            subPrintLine("result=" + result);
        }

        async Task test_set_resolver()
        {
            subPrintLine("get domain info:input domain like a.b.c~");
            var domain = Console.ReadLine();
            string[] strs = domain.Split('.');
            var mh = nns_tools.nameHash(strs[strs.Length - 1]);
            for (var i = strs.Length - 2; i >= 0; i--)
            {
                mh = nns_tools.nameHashSub(mh, strs[i]);
            }
            subPrintLine("mh=" + mh.ToString());

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            Hash160 hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var resolver = new Hash160("0x6e2aea28af9c5febea0774759b1b76398e3167f1");
            var result = await nns_tools.api_SendTransaction(prikey, Config.sc_nns, "owner_SetResolver",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + mh.ToString(),//参数2 域名fullhash
               "(hex160)" + resolver.ToString()//参数3 解析器地址
               );
            subPrintLine("result=" + result);
        }

        async Task test_owner_change()
        {
            subPrintLine("owner chaange [xxx].[yyy] input xxx.yyy:");
            var readline = Console.ReadLine().Split(".");
            var subname = readline[0];
            var rootname = readline[1];

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            Hash160 hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var newowner = ThinNeo.Helper.GetPublicKeyHashFromAddress("ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj");
            var testhash = nns_tools.nameHash(rootname);
            var subhash = nns_tools.nameHashSub(testhash, subname);
            var result = await nns_tools.api_SendTransaction(prikey, Config.sc_nns, "owner_SetOwner",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + subhash.ToString(),//参数2 域名fullhash
               "(hex160)" + newowner.ToString()//参数3 新所有者
               );
            subPrintLine("result=" + result);
        }

        #endregion

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
    }
}
