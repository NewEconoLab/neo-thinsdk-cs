using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nns_sell : ITest
    {
        public string Name => "NNS测试 拍卖";

        public string ID => "nns sell";
        #region menuandlog
        public delegate Task testAction();
        public Dictionary<string, testAction> infos = new Dictionary<string, testAction>();
        string[] submenu;
        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_sell()
        {
            infos["get .sell info"] = test_getsellinfo;
            infos["get [xxx].sell info"] = test_get_xxx_sell_info;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion
        #region testarea
        async Task test_getsellinfo()
        {
            var r = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHash", "(string)sell");
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            var mh = nns_common.nameHash("sell");
            subPrintLine("calc=" + mh.ToString());
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + mh.ToString());
            subPrintLine("getinfo owner=" + info.value.subItem[0].subItem[0].AsHash160());
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[3].AsInteger());
            subPrintLine("getinfo parentOwner=" + info.value.subItem[0].subItem[4].AsHash160());
            subPrintLine("getinfo domain=" + info.value.subItem[0].subItem[5].AsString());
            subPrintLine("getinfo parentHash=" + info.value.subItem[0].subItem[6].AsHash256());
            subPrintLine("getinfo root=" + info.value.subItem[0].subItem[7].AsInteger());
        }
        async Task test_get_xxx_sell_info()
        {
            subPrintLine("get [xxx].test 's info:input xxx:");
            var subname = Console.ReadLine();

            var r_test = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHash", "(string)sell");
            var hash_test = r_test.value.subItem[0].AsHash256();
            var r_abc_test = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHashSub", "(hex256)" + r_test.value.subItem[0].AsHash256().ToString(), "(string)" + subname);
            subPrintLine("得到:" + r_abc_test.value.subItem[0].AsHash256());

            var mh = nns_common.nameHash("sell");
            var mh_abc = nns_common.nameHashSub(mh, subname);

            subPrintLine("calc=" + mh_abc.ToString());
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + mh_abc.ToString());
            subPrintLine("getinfo owner=" + info.value.subItem[0].subItem[0].AsHash160());
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
