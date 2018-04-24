using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nnc : ITest
    {
        public string Name => "NNC NNC合约测试";

        public string ID => "nnc";
        byte[] prikey;
        public string address;
        byte[] scripthash;
        byte[] pubkey;
        Hash160 reg_sc;//注册器合约地址
        public delegate Task testAction();
        public Dictionary<string, testAction> infos = null;// = new Dictionary<string, testAction>();
        string[] submenu;

        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nnc()
        {
            this.initManu();
        }

        private void initManu()
        {
            infos = new Dictionary<string, testAction>();

            infos["totalSupply"] = test_not_implement_yet;
            infos["name"] = test_not_implement_yet;
            infos["symbol"] = test_not_implement_yet;
            infos["decimals"] = test_not_implement_yet;
            infos["balanceOf"] = test_BalanceOf;
            infos["transfer"] = test_Transfer;
            infos["transfer_app"] = test_not_implement_yet;
            infos["deploy"] = test_not_implement_yet;
            infos["balanceOfDetail"] = test_not_implement_yet;
            infos["use"] = test_not_implement_yet;
            infos["use_app"] = test_not_implement_yet;
            infos["getTXInfo"] = test_not_implement_yet;
            infos["getBonus"] = test_not_implement_yet;
            infos["checkBonus"] = test_CheckBonus;
            infos["newBonus"] = test_NewBonus;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }

        public delegate void Method(ThinNeo.ScriptBuilder sb);//第一步：定义委托类型

        public async Task Demo()
        {
            //得到注册器
            var info_reg = await nns_common.api_InvokeScript(nns_common.sc_nns, "getOwnerInfo", "(hex256)" + nns_common.nameHash("sell").ToString());
            this.reg_sc = new Hash160(info_reg.value.subItem[0].subItem[1].data);
            Console.WriteLine("reg=" + reg_sc.ToString());

            showMenu();

            prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(nnc_1.testwif);
            pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

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

        async Task test_BalanceOf()
        {
            Console.WriteLine("Input target address (" + this.address + "):");
            string addr;
            try
            {
                addr = Console.ReadLine();
                if (addr == "\n")
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

            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            var result = await nns_common.api_InvokeScript(shash, "balanceOf", "(bytes)" + strhash);
            //subPrintLine(result);
        }

        async Task test_NewBonus()
        {
            //var array = new MyJson.JsonNode_Array();
            //array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            //sb.EmitParamJson(array);//参数倒序入
            //sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)newBonus"));//参数倒序入
            //ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            //sb.EmitAppCall(shash);//nep5脚本
        }

        async Task test_GetBonus(ThinNeo.ScriptBuilder sb)
        {
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);

            var result = await nns_common.api_SendTransaction(shash, prikey, "getBonus", "(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            subPrintLine(result);
        }

        async Task test_CheckBonus()
        {
            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);
            var result = await nns_common.api_InvokeScript(shash, "checkBonus", "(bytes)" + ThinNeo.Helper.Bytes2HexString(scripthash));
            //subPrintLine(result);
        }

        async Task test_Transfer()
        {
            Console.WriteLine("Input target address:");
            string addressto = Console.ReadLine();
            Console.WriteLine("Input amount:");
            string amount = Console.ReadLine();

            ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc_1.sc_nnc);

            var result = await nns_common.api_SendTransaction(prikey, shash, "transfer",
              "(int)" + amount,
              "(addr)" + addressto,
              "(addr)" + address
              );
            subPrintLine(result);
        }

    }

}
