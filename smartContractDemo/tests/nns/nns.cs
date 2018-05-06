using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;
using static MyJson;

namespace smartContractDemo
{
    public class nns_common
    {
        public const string api = "https://api.nel.group/api/testnet";
        public const string testwif = "L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";

        public static readonly Hash160 sc_nns = new Hash160("0x954f285a93eed7b4aed9396a7806a5812f1a5950");//nns 合约地址

        public static readonly System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();

        public static Hash256 nameHash(string domain)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(domain);
            return new Hash256(sha256.ComputeHash(data));
        }
        public static Hash256 nameHashSub(byte[] roothash, string subdomain)
        {
            var bs = System.Text.Encoding.UTF8.GetBytes(subdomain);
            if (bs.Length == 0)
                return roothash;

            var domain = sha256.ComputeHash(bs).Concat(roothash).ToArray();
            return new Hash256(sha256.ComputeHash(domain));
        }

        #region apitool
        public class ResultItem
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

        public class Result
        {
            public string textInfo;
            public ResultItem value; //不管什么类型统一转byte[]
        }

        public static async Task<Result> api_InvokeScript(Hash160 scripthash, string methodname, params string[] subparam)
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
            var url = Helper.MakeRpcUrlPost(nnc_1.api_local, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var text = await Helper.HttpPost(url, postdata);
            MyJson.JsonNode_Object json = MyJson.Parse(text) as MyJson.JsonNode_Object;

            Result rest = new Result();
            rest.textInfo = text;
            if (json.ContainsKey("result"))
            {
                var result = (json["result"] as MyJson.JsonNode_Object)["stack"].AsList();
                rest.value = ResultItem.FromJson("Array", result);
            }
            return rest;// subPrintLine("得到的结果是：" + result);
        }

        public static async Task<string> api_SendTransaction(byte[] prikey, Hash160 schash, string methodname, params string[] subparam)
        {
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(nnc_1.api, address);
            if (dir.ContainsKey(Nep55_1.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return null;
            }
            //MakeTran
            ThinNeo.Transaction tran = null;
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
                    sb.EmitAppCall(schash);
                    data = sb.ToArray();
                    Console.WriteLine(ThinNeo.Helper.Bytes2HexString(data));
                }

                tran = Helper.makeTran(dir[Nep55_1.id_GAS], null, new ThinNeo.Hash256(Nep55_1.id_GAS), 0);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = data;
                idata.gas = 0;
            }

            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(nnc_1.api_local, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            return result;
        }

        /// <summary>
        /// 重载交易构造方法，对于复杂交易传入脚本
        /// </summary>
        /// <param name="prikey">私钥</param>
        /// <param name="script">交易脚本</param>
        /// <returns></returns>
        public static async Task<string> api_SendTransaction(byte[] prikey, byte[] script)
        {
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            //获取地址的资产列表
              Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(nnc_1.api, address);
            if (dir.ContainsKey(Nep55_1.id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return null;
            }
            //MakeTran
            ThinNeo.Transaction tran = null;
            {

                byte[] data = script;
                tran = Helper.makeTran(dir[Nep55_1.id_GAS], null, new ThinNeo.Hash256(Nep55_1.id_GAS), 0);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = data;
                idata.gas = 0;
            }

            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(nnc_1.api_local, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = await Helper.HttpPost(url, postdata);
            return result;
        }
        #endregion

    }
}
