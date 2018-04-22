using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;
using System.Linq;
namespace smartContractDemo
{
    class nns_user : ITest
    {
        public string Name => "NNS测试 用户";

        public string ID => "nns user";


        #region menuandlog
        public delegate Task testAction();
        public Dictionary<string, testAction> infos = new Dictionary<string, testAction>();
        string[] submenu;
        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_user()
        {
            infos["get .test info"] = test_gettestinfo;
            infos["get [xxx].test info"] = test_get_xxx_test_info;
            infos["request [xxx].test domain"] = test_request_xxx_test_domain;
            infos["set resolver [xxx].test"] = test_set_resolver;
            infos["owner chaange [xxx].test"] = test_owner_change;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion

        #region testarea
        async Task test_gettestinfo()
        {
            var r = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHash", "(string)test");
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            var mh = nns_common.nameHash("test");
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
        async Task test_get_xxx_test_info()
        {
            subPrintLine("get [xxx].test 's info:input xxx:");
            var subname = Console.ReadLine();

            var r_test = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHash", "(string)test");
            var hash_test = r_test.value.subItem[0].AsHash256();
            var r_abc_test = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHashSub", "(hex256)" + r_test.value.subItem[0].AsHash256().ToString(), "(string)" + subname);
            subPrintLine("得到:" + r_abc_test.value.subItem[0].AsHash256());

            var mh = nns_common.nameHash("test");
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

        async Task test_request_xxx_test_domain()
        {
            subPrintLine("request [xxx].test 's domain:input xxx:");
            var subname = Console.ReadLine();

            string testwif = nnc_1.testwif;
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash("test").ToString());
            var _result = info.value.subItem[0];
            var test_reg = _result.subItem[1].AsHash160();//根域名注册器必须获取，写死不行


            var result = await nns_common.api_SendTransaction(prikey, test_reg, "requestSubDomain",
                "(addr)" + address,
                "(hex256)" + nns_common.nameHash("test"),
                "(str)" + subname);

            Console.WriteLine("sendrawtransaction得到的结果是：" + result);
        }

        async Task test_set_resolver()
        {
            subPrintLine("set resolver [xxx].test");
            var subname = Console.ReadLine();
            string testwif = nnc_1.testwif;
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            Hash160 hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var resolver = new Hash160("0x375a7e4630c44fb7e18bdce56fd74c597c86a075");
            var testhash = nns_common.nameHash("test");
            var subhash = nns_common.nameHashSub(testhash, subname);
            var result = await nns_common.api_SendTransaction(prikey, nns_common.sc_nns, "owner_SetResolver",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + subhash.ToString(),//参数2 域名fullhash
               "(hex160)" + resolver.ToString()//参数3 解析器地址
               );
            Console.WriteLine("result=" + result);
        }
        async Task test_owner_change()
        {
            subPrintLine("owner chaange [xxx].test");
            var subname = Console.ReadLine();
            string testwif = nnc_1.testwif;
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            Hash160 hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var newowner = ThinNeo.Helper.GetPublicKeyHashFromAddress("ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj");
            var testhash = nns_common.nameHash("test");
            var subhash = nns_common.nameHashSub(testhash, subname);
            var result = await nns_common.api_SendTransaction(prikey, nns_common.sc_nns, "owner_SetOwner",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + subhash.ToString(),//参数2 域名fullhash
               "(hex160)" + newowner.ToString()//参数3 新所有者
               );
            Console.WriteLine("result=" + result);
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
