using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            infos["initroot .test"] = test_initroot_test;
            //infos["get [xxx].test info"] = test_get_xxx_test_info;
            //infos["request [xxx].test domain"] = test_request_xxx_test_domain;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion
        #region testarea
        async Task test_setjumptarget()
        {
            var target = new ThinNeo.Hash160("0x8b6f1bd09bafb800a6b579e227942cd1f4724010");
            var result = await nns_common.api_SendTransaction(this.superadminprikey, nns_common.sc_nns, "_setTarget", "(hex160)" + target.ToString());
            subPrintLine("result=" + result);
        }
        async Task test_initroot_test()
        {
            var fiforegistor = new ThinNeo.Hash160("0x9a20a91392d90f468fb18dd3070754bec8e573e6");
            var result = await nns_common.api_SendTransaction(this.superadminprikey, nns_common.sc_nns, 
                "initRoot",
                "(str)test",//根域名的名字
                "(hex160)" + fiforegistor.ToString());
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
