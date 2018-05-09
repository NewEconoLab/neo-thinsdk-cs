using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Numerics;
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
        public Dictionary<string, testAction> infos = null;// = new Dictionary<string, testAction>();
        string[] submenu;
        private string root = "sell";
        private byte[] pubkey;
        private byte[] prikey;
        private string address = "";
        private Hash160 scriptHash;
        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_sell()
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
            infos["get ." + this.root + " info"] = test_getsellinfo;
            infos["get [xxx]." + this.root + " info"] = test_get_xxx_sell_info;
            infos["wantbuy [xxx]." + this.root] = test_wantbuy_xxx_sell;
            infos["addprice 10 for [xxx]." + this.root] = test_addprice_xxx_sell;
            infos["endselling [xxx]." + this.root] = test_endselling;//结束拍卖，如果中标我的钱就全没了，没中标退还90%
            infos["get [xxx]." + this.root + " domain"] = test_getsellingdomaain;//
            infos["switch root name"] = test_switch_root;
            infos["change wif key"] = test_change_key;
            infos["get address balanceof sell"] = test_getbalanceof;
            infos["recharge reg"] = test_rechargeReg;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }

        #endregion
        #region testarea
        async Task test_getsellinfo()
        {
            var r = await nns_common.api_InvokeScript(Config.sc_nns, "nameHash", "(string)" + this.root);
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            var mh = nns_common.nameHash(this.root);
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
        async Task test_get_xxx_sell_info()
        {
            subPrintLine("get [xxx]."+this.root+" 's info:input xxx:");
            var subname = Console.ReadLine();

            var r_test = await nns_common.api_InvokeScript(Config.sc_nns, "nameHash", "(string)" + this.root);
            var hash_test = r_test.value.subItem[0].AsHash256();
            var r_abc_test = await nns_common.api_InvokeScript(Config.sc_nns, "nameHashSub", "(hex256)" + r_test.value.subItem[0].AsHash256().ToString(), "(string)" + subname);
            subPrintLine("得到:" + r_abc_test.value.subItem[0].AsHash256());

            var roothash = nns_common.nameHash(this.root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            subPrintLine("calc=" + fullhash.ToString());
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + fullhash.ToString());
            var sh = info.value.subItem[0].subItem[0].AsHash160();
            if (sh != null)
            {
                var owner = ThinNeo.Helper.GetAddressFromScriptHash(sh);
                subPrintLine("getinfo owner=" + owner);
            }
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[3].AsInteger());
            subPrintLine("getinfo parentOwner=" + info.value.subItem[0].subItem[4].AsHash160());

            subPrintLine("getinfo domain=" + info.value.subItem[0].subItem[5].AsString());
            subPrintLine("getinfo parentHash=" + info.value.subItem[0].subItem[6].AsHash256());
            subPrintLine("getinfo root=" + info.value.subItem[0].subItem[7].AsInteger());

            //得到注册器
            var info_reg = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info_reg.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());

            var info2 = await nns_common.api_InvokeScript(reg_sc, "getDomainUseState", "(hex256)" + fullhash.ToString());
            subPrintLine("getDomainUseState=" + info2.value.subItem[0].AsInteger());

  
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            subPrintLine("getSellingStateByFullhash id=" + info3.value.subItem[0].subItem[0].AsHash256());
            subPrintLine("getSellingStateByFullhash parenthash=" + info3.value.subItem[0].subItem[1].AsHash256());
            subPrintLine("getSellingStateByFullhash domain=" + info3.value.subItem[0].subItem[2].AsString());
            subPrintLine("getSellingStateByFullhash domainTTL=" + info3.value.subItem[0].subItem[3].AsInteger());

            subPrintLine("getSellingStateByFullhash startBlockSelling=" + info3.value.subItem[0].subItem[4].AsInteger());
            subPrintLine("getSellingStateByFullhash startBlockRan=" + info3.value.subItem[0].subItem[5].AsInteger());
            subPrintLine("getSellingStateByFullhash endBlock=" + info3.value.subItem[0].subItem[6].AsInteger());

            subPrintLine("getSellingStateByFullhash maxPrice=" + info3.value.subItem[0].subItem[7].AsInteger());
            //var addr_hash = ;
            //if(addr_hash !=null)
            //{
            //var addr = ThinNeo.Helper.GetAddressFromScriptHash(addr_hash.AsHash160());
            subPrintLine("getSellingStateByFullhash maxBuyer=" + info3.value.subItem[0].subItem[8].AsHash160());
            //}

            subPrintLine("getSellingStateByFullhash lastBlock=" + info3.value.subItem[0].subItem[9].AsInteger());

            var id = info3.value.subItem[0].subItem[0].AsHash256();


            var who = this.scriptHash;
            var info4 = await nns_common.api_InvokeScript(reg_sc, "balanceOf",
               "(hex160)" + who.ToString());
            subPrintLine("balanceOf=" + info4.value.subItem[0].AsInteger());

            var info5 = await nns_common.api_InvokeScript(reg_sc, "balanceOfSelling",
                "(hex160)" + who.ToString(),
                "(hex256)" + id.ToString());
            subPrintLine("balanceOfSelling=" + info5.value.subItem[0].AsInteger());

        }
        async Task test_wantbuy_xxx_sell()
        {
            subPrintLine("wantbuy [xxx]." + this.root + ".  input xxx:");
            var subname = Console.ReadLine();


            var roothash = nns_common.nameHash(this.root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());


            var who = this.scriptHash;
            var result = await nns_common.api_SendTransaction(prikey, reg_sc, "wantBuy",
                "(hex160)" + who.ToString(),
                "(hex256)" + roothash.ToString(),
                "(string)" + subname
                );
            subPrintLine("result=" + result);
        }
        async Task test_addprice_xxx_sell()
        {
            subPrintLine("addprice 10 for [xxx]." + this.root + ".  input xxx:");
            var subname = Console.ReadLine();

            var roothash = nns_common.nameHash(this.root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());

            //得到拍卖ID
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            var id = info3.value.subItem[0].subItem[0].AsHash256();
            var who = this.scriptHash;

            var result = await nns_common.api_SendTransaction(this.prikey, reg_sc, "addPrice",
          "(hex160)" + who.ToString(),//参数1 who
          "(hex256)" + id.ToString(),//参数2 交易id
          "(int)1000000000"//参数3，加价多少
          );
            subPrintLine("result=" + result);
        }
        async Task test_endselling()
        {
            subPrintLine("endSelling [xxx]." + this.root + ".  input xxx:");
            var subname = Console.ReadLine();


            var roothash = nns_common.nameHash(this.root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());

            //得到拍卖ID
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            var id = info3.value.subItem[0].subItem[0].AsHash256();


            var who = this.scriptHash;
            var result = await nns_common.api_SendTransaction(prikey, reg_sc, "endSelling",
          "(hex160)" + who.ToString(),//参数1 who
          "(hex256)" + id.ToString()//参数2 交易id
          );
            subPrintLine("result=" + result);
        }
        async Task test_getsellingdomaain()
        {
            subPrintLine("get [xxx]." + this.root + " domain.  input xxx:");
            var subname = Console.ReadLine();

            var roothash = nns_common.nameHash(this.root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());
            //得到拍卖ID
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            var id = info3.value.subItem[0].subItem[0].AsHash256();


            var who = this.scriptHash;
            var result = await nns_common.api_SendTransaction(prikey, reg_sc, "getSellingDomain",
          "(hex160)" + who.ToString(),//参数1 who
          "(hex256)" + id.ToString()//参数2 交易id
          );
            subPrintLine("result=" + result);
        }

        async Task test_getbalanceof()
        {

            var who = this.scriptHash;

            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash(root).ToString());
            var _result = info.value.subItem[0];
            var sell_reg = _result.subItem[1].AsHash160();

            var r_abc_test = await nns_common.api_InvokeScript(sell_reg, "balanceOf", "(hex160)" + who.ToString());
            subPrintLine(root+"注册器里的余额:" + r_abc_test.value.subItem[0].AsInteger());
        }
        /// <summary>
        /// 充值注册器
        /// </summary>
        /// <returns></returns>
        async Task test_rechargeReg()
        {
            var info = await nns_common.api_InvokeScript(Config.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash(root).ToString());
            var _result = info.value.subItem[0];
            var sell_reg = _result.subItem[1].AsHash160();

            string addressto = ThinNeo.Helper.GetAddressFromScriptHash(sell_reg);
            Console.WriteLine("addressto=" + addressto);

            Console.WriteLine("Input amount:");
            string amount = Console.ReadLine();

            byte[] script;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();

                array.AddArrayValue("(addr)" + address);//from
                array.AddArrayValue("(addr)" + addressto);//to
                array.AddArrayValue("(int)" + amount);//value
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("transfer");//参数倒序入
                sb.EmitAppCall(Config.dapp_sgas);//nep5脚本

                ////这个方法是为了在同一笔交易中转账并充值
                ////当然你也可以分为两笔交易
                ////插入下述两条语句，能得到txid
                sb.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                sb.EmitSysCall("Neo.Transaction.GetHash");
                //把TXID包进Array里
                sb.EmitPushNumber(1);
                sb.Emit(ThinNeo.VM.OpCode.PACK);
                sb.EmitPushString("setmoneyin");
                sb.EmitAppCall(Config.dapp_coinpool);
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = await nns_common.api_SendTransaction(prikey, script);
            subPrintLine(result);

        }
        async Task test_switch_root()
        {
            subPrintLine("input root name:");
            var root = Console.ReadLine();

            this.root = root;
            this.initManu();
            this.showMenu();
        }

        async Task test_change_key()
        {
            subPrintLine("input wif key:");
            var wif = Console.ReadLine();

            Config.changeWif(wif);
            this.initAccount();
            this.initManu();
            this.showMenu();
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
            this.initAccount();
            showMenu();

            while (true)
            {

                var line = Console.ReadLine().Replace(" ", "").ToLower();
                if (line == "?" || line == "？" || line == "ls")
                {
                    showMenu();
                }
                else if (line == "")
                {
                    continue;
                }
                else if (line == "0" || line == "cd ..")
                {
                    return;
                }
                else//get .test's info
                {
                    string key = "";
                    try
                    {
                        var id = int.Parse(line) - 1;
                        key = submenu[id];
                    }
                    catch (Exception err)
                    {
                        subPrintLine("Unknown option");
                        continue;
                    }

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
