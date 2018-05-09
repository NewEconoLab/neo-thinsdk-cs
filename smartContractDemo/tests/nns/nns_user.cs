using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;
using System.Linq;
using smartContractDemo.tests;

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
            infos["get .[yyy] info"] = test_get_yyy_info;
            infos["get [xxx].[yyy] info"] = test_get_xxx_test_info;
            infos["request [xxx].[yyy] domain"] = test_request_xxx_yyy_domain;
            infos["set resolver [xxx].[yyy]"] = test_set_resolver;
            infos["owner change [xxx].[yyy]"] = test_owner_change;
            infos["config resolve 1.[xxx].[yyy]"] = test_config_resolve_1_xxx_yyy;
            infos["resolve text://1.[xxx].[yyy]"] = test_resolve_1_xxx_yyy;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion

        #region testarea
        async Task test_get_yyy_info()
        {
            subPrintLine("get .[yyy] 's info:input yyy:");
            var rootname = Console.ReadLine();

            var r = await nns_common.api_InvokeScript(Config.sc_nns, "nameHash", "(string)"+ rootname);
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            var mh = nns_common.nameHash(rootname);
            subPrintLine("calc=" + mh.ToString());
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + mh.ToString());
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
            subPrintLine("get [xxx].[yyy] 's info:input xxx.yyy:");
            var readline = Console.ReadLine().Split(".");
            var subname = readline[0];
            var rootname = readline[1];

            var r_test = await nns_common.api_InvokeScript(Config.sc_nns, "nameHash", "(string)"+ rootname);
            var hash_test = r_test.value.subItem[0].AsHash256();
            var r_abc_test = await nns_common.api_InvokeScript(Config.sc_nns, "nameHashSub", "(hex256)" + r_test.value.subItem[0].AsHash256().ToString(), "(string)" + subname);
            subPrintLine("得到:" + r_abc_test.value.subItem[0].AsHash256());

            var mh = nns_common.nameHash(rootname);
            var mh_abc = nns_common.nameHashSub(mh, subname);

            subPrintLine("calc=" + mh_abc.ToString());
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + mh_abc.ToString());
            subPrintLine("getinfo owner=" + ThinNeo.Helper.GetAddressFromScriptHash(info.value.subItem[0].subItem[0].AsHash160()));
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[3].AsInteger());
            subPrintLine("getinfo parentOwner=" + info.value.subItem[0].subItem[4].AsHash160());

            subPrintLine("getinfo domain=" + info.value.subItem[0].subItem[5].AsString());
            subPrintLine("getinfo parentHash=" + info.value.subItem[0].subItem[6].AsHash256());
            subPrintLine("getinfo root=" + info.value.subItem[0].subItem[7].AsInteger());
        }

        async Task test_request_xxx_yyy_domain()
        {
            subPrintLine("request [xxx].[yyy] 's domain:input xxx.yyy:");
            var readline = Console.ReadLine().Split(".");
            var subname = readline[0];
            var rootname = readline[1];

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash(rootname).ToString());
            var _result = info.value.subItem[0];
            var test_reg = _result.subItem[1].AsHash160();//根域名注册器必须获取，写死不行

            var sss = nns_common.nameHash(rootname);
            var result = await nns_common.api_SendTransaction(prikey, test_reg, "requestSubDomain",
                "(addr)" + address,
                "(hex256)" + nns_common.nameHash(rootname),
                "(str)" + subname);

            subPrintLine("sendrawtransaction得到的结果是：" + result);
        }

        async Task test_set_resolver()
        {
            subPrintLine("set resolver [xxx].[yyy] input xxx.yyy:");
            var readline = Console.ReadLine().Split(".");
            var subname = readline[0];
            var rootname = readline[1];

   
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            Hash160 hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var resolver = new Hash160("0xabb0f1f3f035dd7ad80ca805fce58d62c517cc6b");
            var testhash = nns_common.nameHash(rootname);
            var subhash = nns_common.nameHashSub(testhash, subname);
            var result = await nns_common.api_SendTransaction(prikey, Config.sc_nns, "owner_SetResolver",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + subhash.ToString(),//参数2 域名fullhash
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
            var testhash = nns_common.nameHash(rootname);
            var subhash = nns_common.nameHashSub(testhash, subname);
            var result = await nns_common.api_SendTransaction(prikey, Config.sc_nns, "owner_SetOwner",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + subhash.ToString(),//参数2 域名fullhash
               "(hex160)" + newowner.ToString()//参数3 新所有者
               );
            subPrintLine("result=" + result);
        }
        async Task test_config_resolve_1_xxx_yyy()
        {
            subPrintLine("config resolve 1.[xxx].[yyy]  input xxx.yyy:");
            var readline = Console.ReadLine().Split(".");
            var subname = readline[0];
            var rootname = readline[1];

            var testhash = nns_common.nameHash(rootname);
            var subhash = nns_common.nameHashSub(testhash, subname);

            var _result = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo",
                "(hex256)" + subhash.ToString());
            var resolver = new Hash160(_result.value.subItem[0].subItem[2].data);
            subPrintLine("resolver=" + resolver.ToString());

            var owner = new Hash160(_result.value.subItem[0].subItem[0].data);
            //string testwif = nnc_1.testwif;
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            Hash160 hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            if (owner.Equals(hash) == false)
            {
                subPrintLine("this is not your domain.");
                return;
            }
            var newowner = ThinNeo.Helper.GetPublicKeyHashFromAddress("ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj");
            var result = await nns_common.api_SendTransaction(prikey, resolver, "setResolveData",
               "(hex160)" + hash.ToString(),//参数1 所有者
               "(hex256)" + subhash.ToString(),//参数2 域名fullhash
               "(string)" + "1",//参数3 要设置的子域名
               "(string)" + "text",//参数4 协议
               "(string)" + "hello world"//解析内容
               );
            subPrintLine("result=" + result);

        }
        async Task test_resolve_1_xxx_yyy()
        {
            subPrintLine("resolve 1.[xxx].[yyy]  input xxx.yyy:");
            var readline = Console.ReadLine().Split(".");
            var subname = readline[0];
            var rootname = readline[1];

            var testhash = nns_common.nameHash(rootname);
            var subhash = nns_common.nameHashSub(testhash, subname);

            var _result = await nns_common.api_InvokeScript(Config.sc_nns, "resolve",
                "(string)"+ rootname,
                "(hex256)" + subhash.ToString(),
                "(string)1"
                );

            subPrintLine("result=" + _result.value.subItem[0].AsString());

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
