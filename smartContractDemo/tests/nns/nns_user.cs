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

        public static readonly Hash160 sc_nns = new Hash160("0x954f285a93eed7b4aed9396a7806a5812f1a5950");//nns 合约地址

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
            infos["get abc.test info"] = test_get_abc_test_info;
            infos["request neodun.test domain"] = test_request_neodun_test_domain;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion
        static System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();

        static Hash256 nameHash(string domain)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(domain);
            return new Hash256(sha256.ComputeHash(data));
        }
        static Hash256 nameHashSub(byte[] roothash, string subdomain)
        {
            var bs = System.Text.Encoding.UTF8.GetBytes(subdomain);
            if (bs.Length == 0)
                return roothash;

            var domain = sha256.ComputeHash(bs).Concat(roothash).ToArray();
            return new Hash256(sha256.ComputeHash(domain));
        }
        #region apitool
        class ResultItem
        {
            public byte[] data;
            public ResultItem[] subItem;
            public static ResultItem FromJson(string type, MyJson.IJsonNode value)
            {
                ResultItem item = new ResultItem();
                if (type == "Array")
                {
                    item.subItem = new ResultItem[value.AsList().Count];
                    for (var i = 0; i < item.subItem.Length; i++)
                    {
                        var subjson = value.AsList()[i].AsDict();
                        var subtype = subjson["type"].AsString();
                        item.subItem[i] = FromJson(subtype, subjson["value"]);
                    }
                }
                else if (type == "ByteArray")
                {
                    item.data = ThinNeo.Helper.HexString2Bytes(value.AsString());
                }
                else if (type == "Integer")
                {
                    item.data = System.Numerics.BigInteger.Parse(value.AsString()).ToByteArray();
                }
                else if (type == "Boolean")
                {
                    if (value.AsBool())
                        item.data = new byte[1] { 0x01 };
                    else
                        item.data = new byte[0];
                }
                else if (type == "String")
                {
                    item.data = System.Text.Encoding.UTF8.GetBytes(value.AsString());
                }
                else
                {
                    throw new Exception("not support type:" + type);
                }
                return item;
            }
            public string AsHexString()
            {
                return ThinNeo.Helper.Bytes2HexString(data);
            }
            public string AsHashString()
            {
                return "0x" + ThinNeo.Helper.Bytes2HexString(data.Reverse().ToArray());
            }
            public string AsString()
            {
                return System.Text.Encoding.UTF8.GetString(data);
            }
            public Hash160 AsHash160()
            {
                if (data.Length == 0)
                    return null;
                return new Hash160(data);
            }
            public Hash256 AsHash256()
            {
                if (data.Length == 0)
                    return null;
                return new Hash256(data);
            }
            public bool AsBoolean()
            {
                if (data.Length == 0 || data[0] == 0)
                    return false;
                return true;
            }
            public System.Numerics.BigInteger AsInteger()
            {
                return new System.Numerics.BigInteger(data);
            }
        }

        class Result
        {
            public string textInfo;
            public ResultItem value; //不管什么类型统一转byte[]
        }

        static async Task<Result> api_InvokeScript(Hash160 scripthash, string methodname, params string[] subparam)
        {
            byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                for (var i = 0; i < subparam.Length; i++)
                {
                    array.AddArrayValue(subparam[i]);
                }
                sb.EmitParamJson(array);
                sb.EmitPushString(methodname);
                sb.EmitAppCall(scripthash);
                data = sb.ToArray();
            }
            string script = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(nnc_1.api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var text = await Helper.HttpPost(url, postdata);
            MyJson.JsonNode_Object json = MyJson.Parse(text) as MyJson.JsonNode_Object;

            Result rest = new Result();
            rest.textInfo = text;
            if (json.ContainsKey("result"))
            {

                var result = json["result"].AsList()[0].AsDict()["stack"].AsList();
                rest.value = ResultItem.FromJson("Array", result);
            }
            return rest;// subPrintLine("得到的结果是：" + result);
        }
        #endregion
        #region testarea
        async Task test_gettestinfo()
        {
            var r = await api_InvokeScript(sc_nns, "nameHash", "(string)test");
            subPrintLine("得到:" + new Hash256(r.value.subItem[0].data).ToString());
            var mh = nameHash("test");
            subPrintLine("calc=" + mh.ToString());
            var info = await api_InvokeScript(sc_nns, "getOwnerInfo", "(hex256)" + mh.ToString());
            subPrintLine("getinfo owner=" + info.value.subItem[0].subItem[0].AsHash160());
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[2].AsInteger());
            subPrintLine("getinfo parentOwner=" + info.value.subItem[0].subItem[2].AsHash160());
        }
        async Task test_get_abc_test_info()
        {
            var r_test = await api_InvokeScript(sc_nns, "nameHash", "(string)test");
            var hash_test = r_test.value.subItem[0].AsHash256();
            var r_abc_test = await api_InvokeScript(sc_nns, "nameHashSub", "(hex256)" + r_test.value.subItem[0].AsHash256().ToString(), "(string)abc");
            subPrintLine("得到:" + r_abc_test.value.subItem[0].AsHash256());

            var mh = nameHash("test");
            var mh_abc = nameHashSub(mh, "abc");

            subPrintLine("calc=" + mh_abc.ToString());
            var info = await api_InvokeScript(sc_nns, "getOwnerInfo", "(hex256)" + mh_abc.ToString());
            subPrintLine("getinfo owner=" + info.value.subItem[0].subItem[0].AsHash160());
            subPrintLine("getinfo register=" + info.value.subItem[0].subItem[1].AsHash160());
            subPrintLine("getinfo resovler=" + info.value.subItem[0].subItem[2].AsHash160());
            subPrintLine("getinfo ttl=" + info.value.subItem[0].subItem[2].AsInteger());
            subPrintLine("getinfo parentOwner=" + info.value.subItem[0].subItem[2].AsHash160());
        }

        async Task test_request_neodun_test_domain()
        {
            string testwif = nnc_1.testwif;
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(testwif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            byte[] scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Nep55_1.api, address);
            if (dir.ContainsKey(Nep55_1.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return;
            }
            //MakeTran
            ThinNeo.Transaction tran = null;
            {

                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var rootHash = new ThinNeo.Hash256(ThinNeo.Helper.nameHash("test"));

                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(addr)" + address);
                    array.AddArrayValue("(hex256)" + rootHash);
                    array.AddArrayValue("(str)neodun");//可以更改成想注册的二级域名
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)requestSubDomain"));//参数倒序入
                    string testRegister = "0x9a20a91392d90f468fb18dd3070754bec8e573e6"; //这是test根域名的注册器  可以用test_gettestinfo例子获取
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(testRegister);
                    sb.EmitAppCall(shash);//
                    script = sb.ToArray();
                }

                tran = Helper.makeTran(dir[Nep55_1.id_GAS], null, new ThinNeo.Hash256(Nep55_1.id_GAS), 0);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;
                idata.gas = 0;
            }

            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(nnc_1.api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            Console.WriteLine("sendrawtransaction得到的结果是：" + result);
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
