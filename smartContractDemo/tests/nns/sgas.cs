using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class SGAS : ITest
    {
        public string Name => "SGAS 合约测试";

        public string ID => "SGAS";

        public delegate Task testAction();
        public Dictionary<string, testAction> infos = null;// = new Dictionary<string, testAction>();
        string[] submenu;
        private byte[] pubkey;
        private byte[] prikey;
        private string address = "";
        private Hash160 scriptHash;

        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public SGAS()
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

            infos["totalSupply"] = test_totalSupply;
            infos["name"] = test_name;
            infos["symbol"] = test_symbol;
            infos["decimals"] = test_decimals;
            infos["balanceOf"] = test_BalanceOf;
            infos["transfer"] = test_Transfer;
            infos["transfer_app"] = test_not_implement_yet;
            infos["getTXInfo"] = test_not_implement_yet;
            infos["mintTokens"] = test_mintTokens;
            infos["refund"] = test_refund;
            infos["getRefundTarget"] = test_not_implement_yet;
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

        async Task test_not_implement_yet()
        {
            subPrintLine("尚未实现");
        }

        #region nep5标准接口

        async Task test_totalSupply()
        {
            subPrintLine("Get Total Supply for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "totalSupply");
            subPrintLine("Total Supply : " + result.value.subItem[0].AsInteger());
        }

        async Task test_name()
        {
            subPrintLine("Get Name for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "name");
            subPrintLine("Name : " + result.value.subItem[0].AsString());
        }

        async Task test_symbol()
        {
            subPrintLine("Get Symbol for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "symbol");
            subPrintLine("Symbol : " + result.value.subItem[0].AsString());
        }

        async Task test_decimals()
        {
            subPrintLine("Get decimals for " + this.ID + ":");

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "decimals");
            subPrintLine("decimals : " + result.value.subItem[0].AsInteger());
        }


        async Task test_BalanceOf()
        {
            Console.WriteLine("    Input target address (" + this.address + "):");
            string addr;
            try
            {
                addr = Console.ReadLine();
                if (addr.Length < 34)
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

            var result = await nns_common.api_InvokeScript(Config.dapp_sgas, "balanceOf", "(bytes)" + strhash);
            Console.Write("    Balance of ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(addr);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" = "+result.value.subItem[0].AsInteger() + "");
        }

        async Task test_Transfer()
        {
            Console.WriteLine("Input target address:");
            string addressto = Console.ReadLine();
            Console.WriteLine("Input amount:");
            string amount = Console.ReadLine();

            var result = await nns_common.api_SendTransaction(prikey, Config.dapp_sgas, "transfer",
              "(addr)" + address,
              "(addr)" + addressto,
              "(int)" + amount
              );
            subPrintLine(result);
        }


        async Task test_mintTokens()
        {
            decimal amount = 0;
            while (true)
            {
                Console.WriteLine("Input amount:");
                string str_amount = Console.ReadLine();
                try
                {
                    amount = decimal.Parse(str_amount);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("input number");
                }
            }

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Config.api, address);
            if (dir.ContainsKey(Config.id_GAS )== false)
            {
                Console.WriteLine("no gas");
                return;
            }
            ThinNeo.Transaction tran = null;
            {
                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)mintTokens"));//参数倒序入
                    ThinNeo.Hash160 shash = Config.dapp_sgas;
                    sb.EmitAppCall(shash);//nep5脚本
                    script = sb.ToArray();
                }
                var sgasScripthash = Config.dapp_sgas;
                var targetaddr = ThinNeo.Helper.GetAddressFromScriptHash(sgasScripthash);
                Console.WriteLine("contract address=" + targetaddr);//往合约地址转账

                //生成交易
                tran = Helper.makeTran(dir[Config.id_GAS], targetaddr, new ThinNeo.Hash256(Config.id_GAS), amount);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;

                //sign and broadcast
                var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
                tran.AddWitness(signdata, pubkey, address);
                var trandata = tran.GetRawData();
                var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
                byte[] postdata;
                var url = Helper.MakeRpcUrlPost(Config.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
                var result = await Helper.HttpPost(url, postdata);
                Console.WriteLine("得到的结果是：" + result);
                var json = MyJson.Parse(result).AsDict();
                if (json.ContainsKey("result"))
                {
                    var resultv = json["result"].AsList()[0].AsDict();
                    var txid = resultv["txid"].AsString();
                    Console.WriteLine("txid=" + txid);
                }
            }
        }


        async Task test_refund()
        {
            decimal amount = 0;
            while (true)
            {
                Console.WriteLine("Input amount:");
                string str_amount = Console.ReadLine();
                try
                {
                    amount = decimal.Parse(str_amount);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("input number");
                }
            }

            string sgas_address = ThinNeo.Helper.GetAddressFromScriptHash(Config.dapp_sgas);

            //获取sgas合约地址的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Config.api, sgas_address);
            if (dir.ContainsKey(Config.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return;
            }
            List<Utxo> newlist = new List<Utxo>(dir[Config.id_GAS]);
            //检查sgas地址拥有的gas的utxo是否有被标记过
            for (var i = newlist.Count - 1; i >= 0; i--)
            {
                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(hex256)" + newlist[i].txid.ToString());
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)getRefundTarget"));//参数倒序入
                    var shash = Config.dapp_sgas;
                    sb.EmitAppCall(shash);//nep5脚本
                    script = sb.ToArray();
                }
                if (newlist[i].n > 0)
                    continue;

                var urlCheckUTXO = Helper.MakeRpcUrl(Config.api, "invokescript", new MyJson.JsonNode_ValueString(ThinNeo.Helper.Bytes2HexString(script)));
                string resultCheckUTXO = await Helper.HttpGet(urlCheckUTXO);
                var jsonCU = MyJson.Parse(resultCheckUTXO);
                var stack = jsonCU.AsDict()["result"].AsList()[0].AsDict()["stack"].AsList()[0].AsDict();
                var value = stack["value"].AsString();
                if (value.Length > 0)//已经标记的UTXO，不能使用
                {
                    newlist.RemoveAt(i);
                }
            }


            ThinNeo.Transaction tran = null;
            {
                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(bytes)" + ThinNeo.Helper.Bytes2HexString(scriptHash));
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)refund"));//参数倒序入
                    var shash = Config.dapp_sgas;
                    sb.EmitAppCall(shash);//nep5脚本
                    script = sb.ToArray();
                }

                //sgas 自己给自己转账   用来生成一个utxo  合约会把这个utxo标记给发起的地址使用
                tran = Helper.makeTran(newlist, sgas_address, new ThinNeo.Hash256(Config.id_GAS), amount);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;

                //附加鉴证
                tran.attributes = new ThinNeo.Attribute[1];
                tran.attributes[0] = new ThinNeo.Attribute();
                tran.attributes[0].usage = ThinNeo.TransactionAttributeUsage.Script;
                tran.attributes[0].data = scriptHash;
            }

            //sign and broadcast
            {//做智能合约的签名
                byte[] n55contract = null;
                {
                    var urlgetscript = Helper.MakeRpcUrl(Config.api, "getcontractstate", new MyJson.JsonNode_ValueString(Config.dapp_sgas.ToString()));
                    var resultgetscript = await Helper.HttpGet(urlgetscript);
                    var _json = MyJson.Parse(resultgetscript).AsDict();
                    var _resultv = _json["result"].AsList()[0].AsDict();
                    n55contract = ThinNeo.Helper.HexString2Bytes(_resultv["script"].AsString());
                }
                byte[] iscript = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    sb.EmitPushString("whatever");
                    sb.EmitPushNumber(250);
                    iscript = sb.ToArray();
                }
                tran.AddWitnessScript(n55contract, iscript);
            }
            {//做提款人的签名
                var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
                tran.AddWitness(signdata, pubkey, address);
            }
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(Config.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));

            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
            var json = MyJson.Parse(result).AsDict();
            if (json.ContainsKey("result"))
            {
                bool bSucc = false;
                if (json["result"].type == MyJson.jsontype.Value_Number)
                {
                    bSucc = json["result"].AsBool();
                    Console.WriteLine("cli=" + json["result"].ToString());
                }
                else
                {
                    var resultv = json["result"].AsList()[0].AsDict();
                    var txid = resultv["txid"].AsString();
                    bSucc = txid.Length > 0;
                }
                if (bSucc)
                {
                    Hash256 txid = tran.GetHash();
                    url = Helper.MakeRpcUrlPost(Config.api, "getrawtransaction", out postdata, new MyJson.JsonNode_ValueString(txid.ToString()));
                    while (true)
                    {
                        subPrintLine("正在等待交易验证，请稍后。。。。");
                        result = await Helper.HttpPost(url, postdata);
                        json = MyJson.Parse(result).AsDict();
                        if (json.ContainsKey("result"))
                        {
                            //tx的第一个utxo就是给自己的
                            Utxo utxo = new Utxo(address, txid,Config.id_GAS,amount,0);
                            //把这个txid里的utxo[0]的value转给自己
                            TranGas(new List<Utxo>() { utxo },amount);
                            break;
                        }
                        await Task.Delay(5000);
                    }
                }
                else
                {
                }
            }
        }


        private async void TranGas(List<Utxo> list,decimal value)
        {
            var tran = Helper.makeTran(list, address, new ThinNeo.Hash256(Config.id_GAS), value);
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;
            //sign and broadcast
            {//做智能合约的签名
                byte[] sgasScript = null;
                {
                    var urlgetscript = Helper.MakeRpcUrl(Config.api, "getcontractstate", new MyJson.JsonNode_ValueString(Config.dapp_sgas.ToString()));
                    var resultgetscript = await Helper.HttpGet(urlgetscript);
                    var _json = MyJson.Parse(resultgetscript).AsDict();
                    var _resultv = _json["result"].AsList()[0].AsDict();
                    sgasScript = ThinNeo.Helper.HexString2Bytes(_resultv["script"].AsString());
                }
                byte[] iscript = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    sb.EmitPushNumber(0);
                    sb.EmitPushNumber(0);
                    iscript = sb.ToArray();
                }
                tran.AddWitnessScript(sgasScript, iscript);
            }

            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(Config.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));

            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
        }

        #endregion

    }

}
