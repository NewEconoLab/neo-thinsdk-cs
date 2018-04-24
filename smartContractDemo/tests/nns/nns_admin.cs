using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nns_admin : ITest
    {
        public string Name => "NNS测试 超级管理员";

        public string ID => "nns admin";

        string superadminAddress = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
        byte[] superadminprikey;
        #region menuandlog
        public delegate Task testAction();
        public Dictionary<string, testAction> infos = new Dictionary<string, testAction>();
        string[] submenu;
        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_admin()
        {
            infos["set jump _target"] = test_setjumptarget;
            infos["get _target name"] = test_gettargetname;
            //infos["initroot .test"] = test_initroot_test;
            //infos["initroot .sell"] = test_initroot_sell;
            infos["initroot .xxx"] = test_initroot_xxx;
            infos["get root .xxx info"] = test_get_xxx_info;
            //infos["get [xxx].test info"] = test_get_xxx_test_info;
            //infos["request [xxx].test domain"] = test_request_xxx_test_domain;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion
        #region testarea
        string domaincenterhash = "0x2b881a0998cb8e91783b8d671e0f0f42adf4840f";
        async Task test_setjumptarget()
        {
            var target = new ThinNeo.Hash160(domaincenterhash);
            var result = await nns_common.api_SendTransaction(this.superadminprikey, nns_common.sc_nns, "_setTarget", "(hex160)" + target.ToString());
            subPrintLine("result=" + result);
        }

        async Task test_gettargetname()
        {
            var target = new ThinNeo.Hash160(domaincenterhash);
            var result = await nns_common.api_InvokeScript(target,"name");
            subPrintLine("result=" + System.Text.Encoding.UTF8.GetString(result.value.subItem[0].data));
        }
        async Task test_initroot_xxx()
        {
            subPrintLine("input root domain:");
            var root = Console.ReadLine();

            subPrintLine("input register hash:");
            var reg = Console.ReadLine();
            reg = reg.Replace(" ", "");
            var sellregistor = new ThinNeo.Hash160(reg);
            var result = await nns_common.api_SendTransaction(this.superadminprikey, nns_common.sc_nns,
                "initRoot",
                "(str)"+root,//根域名的名字
                "(hex160)" + sellregistor.ToString());
            subPrintLine("result=" + result);

        }

        async Task test_get_xxx_info()
        {
            subPrintLine("input root domain:");
            var root = Console.ReadLine();
            var r = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHash", "(string)"+ root);
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            var mh = nns_common.nameHash(root);
            subPrintLine("calc=" + mh.ToString());
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + mh.ToString());
            subPrintLine("getinfo owner=" + ThinNeo.Helper.GetAddressFromScriptHash(info.value.subItem[0].subItem[0].AsHash160()));
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[3].AsInteger());
            subPrintLine("getinfo parentOwner=" + info.value.subItem[0].subItem[4].AsHash160());
            subPrintLine("getinfo domain=" + info.value.subItem[0].subItem[5].AsString());
            subPrintLine("getinfo parentHash=" + info.value.subItem[0].subItem[6].AsHash256());
            subPrintLine("getinfo root=" + info.value.subItem[0].subItem[7].AsInteger());
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
            subPrintLine("input [" + superadminAddress + "]'s wif first:");
            var wif = Console.ReadLine();
            wif = wif.Replace(" ", "");
            this.superadminprikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            var pubkey=ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.superadminprikey);
            var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            if(addr!=this.superadminAddress)
            {
                subPrintLine("wif错误");
                return;
            }
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
