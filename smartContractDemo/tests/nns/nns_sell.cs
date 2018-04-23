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
        public Dictionary<string, testAction> infos = null ;// = new Dictionary<string, testAction>();
        string[] submenu;
        private string root ="sell";
        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_sell()
        {
            this.initManu();
        }

        private void initManu()
        {
            infos = new Dictionary<string, testAction>();
            infos["get ." + root + " info"] = test_getsellinfo;
            infos["get [xxx]." + root + " info"] = test_get_xxx_sell_info;
            infos["wantbuy [xxx]." + root] = test_wantbuy_xxx_sell;
            infos["addprice 10 for [xxx]." + root] = test_addprice_xxx_sell;
            infos["endselling [xxx]." + root] = test_endselling;//结束拍卖，如果中标我的钱就全没了，没中标退还90%
            infos["get [xxx]." + root + " domain"] = test_getsellingdomaain;//
            infos["switch root name"] = test_switch_root;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion
        #region testarea
        async Task test_getsellinfo()
        {
            var r = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHash", "(string)" + root);
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            var mh = nns_common.nameHash(root);
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

            var r_test = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHash", "(string)" + root);
            var hash_test = r_test.value.subItem[0].AsHash256();
            var r_abc_test = await nns_common.api_InvokeScript(nns_common.sc_nns, "nameHashSub", "(hex256)" + r_test.value.subItem[0].AsHash256().ToString(), "(string)" + subname);
            subPrintLine("得到:" + r_abc_test.value.subItem[0].AsHash256());

            var roothash = nns_common.nameHash(root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            subPrintLine("calc=" + fullhash.ToString());
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + fullhash.ToString());
            subPrintLine("getinfo owner=" + info.value.subItem[0].subItem[0].AsHash160());
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[3].AsInteger());
            subPrintLine("getinfo parentOwner=" + info.value.subItem[0].subItem[4].AsHash160());

            subPrintLine("getinfo domain=" + info.value.subItem[0].subItem[5].AsString());
            subPrintLine("getinfo parentHash=" + info.value.subItem[0].subItem[6].AsHash256());
            subPrintLine("getinfo root=" + info.value.subItem[0].subItem[7].AsInteger());

            //得到注册器
            var info_reg = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info_reg.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());

            var info2 = await nns_common.api_InvokeScript(reg_sc, "getDomainUseState", "(hex256)" + fullhash.ToString());
            subPrintLine("getDomainUseState=" + info2.value.subItem[0].AsInteger());

            //            public class SellingState
            //{
            //    public byte[] id; //拍卖id，就是拍卖生成的txid

            //    public byte[] parenthash;//拍卖内容
            //    public string domain;//拍卖内容
            //    public BigInteger domainTTL;//域名的TTL，用这个信息来判断域名是否发生了变化

            //    public BigInteger startBlockSelling;//开始销售块
            //    //public int StartTime 算出
            //    //step2time //算出
            //    //rantime //算出
            //    //endtime //算出
            //    //最终领取时间 算出，如果超出最终领取时间没有领域名，就不让领了
            //    public BigInteger startBlockRan;//当第一个在rantime~endtime之后出价的人，记录他出价的块
            //    //从这个块开始，往后的每一个块出价都有一定几率直接结束
            //    public BigInteger endBlock;//结束块

            //    public BigInteger maxPrice;//最高出价
            //    public byte[] maxBuyer;//最大出价者
            //    public BigInteger lastBlock;//最后出价块
            //}
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            subPrintLine("getSellingStateByFullhash id=" + info3.value.subItem[0].subItem[0].AsHash256());
            subPrintLine("getSellingStateByFullhash parenthash=" + info3.value.subItem[0].subItem[1].AsHash256());
            subPrintLine("getSellingStateByFullhash domain=" + info3.value.subItem[0].subItem[2].AsString());
            subPrintLine("getSellingStateByFullhash domainTTL=" + info3.value.subItem[0].subItem[3].AsInteger());

            subPrintLine("getSellingStateByFullhash startBlockSelling=" + info3.value.subItem[0].subItem[4].AsInteger());
            subPrintLine("getSellingStateByFullhash startBlockRan=" + info3.value.subItem[0].subItem[5].AsInteger());
            subPrintLine("getSellingStateByFullhash endBlock=" + info3.value.subItem[0].subItem[6].AsInteger());

            subPrintLine("getSellingStateByFullhash maxPrice=" + info3.value.subItem[0].subItem[7].AsInteger());
            subPrintLine("getSellingStateByFullhash maxBuyer=" + info3.value.subItem[0].subItem[8].AsHash160());
            subPrintLine("getSellingStateByFullhash lastBlock=" + info3.value.subItem[0].subItem[9].AsInteger());

            var id = info3.value.subItem[0].subItem[0].AsHash256();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nns_common.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var who = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
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
            subPrintLine("wantbuy [xxx]." + root + ".  input xxx:");
            var subname = Console.ReadLine();


            var roothash = nns_common.nameHash(root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nns_common.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var who = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            var result = await nns_common.api_SendTransaction(prikey, reg_sc, "wantBuy",
                "(hex160)" + who.ToString(),
                "(hex256)" + roothash.ToString(),
                "(string)" + subname
                );
            subPrintLine("result=" + result);
        }
        async Task test_addprice_xxx_sell()
        {
            subPrintLine("addprice 10 for [xxx]." + root + ".  input xxx:");
            var subname = Console.ReadLine();


            var roothash = nns_common.nameHash(root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());
            //得到拍卖ID
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            var id = info3.value.subItem[0].subItem[0].AsHash256();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nns_common.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var who = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            var result = await nns_common.api_SendTransaction(prikey, reg_sc, "addPrice",
          "(hex160)" + who.ToString(),//参数1 who
          "(hex256)" + id.ToString(),//参数2 交易id
          "(int)1000000000"//参数3，加价多少
          );
            subPrintLine("result=" + result);
        }
        async Task test_endselling()
        {
            subPrintLine("endSelling [xxx]." + root + ".  input xxx:");
            var subname = Console.ReadLine();


            var roothash = nns_common.nameHash(root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());
            //得到拍卖ID
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            var id = info3.value.subItem[0].subItem[0].AsHash256();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nns_common.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var who = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            var result = await nns_common.api_SendTransaction(prikey, reg_sc, "endSelling",
          "(hex160)" + who.ToString(),//参数1 who
          "(hex256)" + id.ToString()//参数2 交易id
          );
            subPrintLine("result=" + result);
        }
        async Task test_getsellingdomaain()
        {
            subPrintLine("get [xxx]." + root + " domain.  input xxx:");
            var subname = Console.ReadLine();


            var roothash = nns_common.nameHash(root);
            var fullhash = nns_common.nameHashSub(roothash, subname);

            //得到注册器
            var info = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + roothash.ToString());
            var reg_sc = new Hash160(info.value.subItem[0].subItem[1].data);
            subPrintLine("reg=" + reg_sc.ToString());
            //得到拍卖ID
            var info3 = await nns_common.api_InvokeScript(reg_sc, "getSellingStateByFullhash", "(hex256)" + fullhash.ToString());
            var id = info3.value.subItem[0].subItem[0].AsHash256();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nns_common.testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var who = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            var result = await nns_common.api_SendTransaction(prikey, reg_sc, "getSellingDomain",
          "(hex160)" + who.ToString(),//参数1 who
          "(hex256)" + id.ToString()//参数2 交易id
          );
            subPrintLine("result=" + result);
        }

        async Task test_switch_root()
        {
            subPrintLine("input root name:");
            var root = Console.ReadLine();
            
            this.root = root;
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
