using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace smartContractDemo
{
    public class Demo2
    {
        string api = "https://api.nel.group/api/testnet";

        httpHelper http = new httpHelper();

        public void Demo()
        {
            string nnc = "0x3fccdb91c9bb66ef2446010796feb6ca4ed96b05".Replace("0x", "");
            var sb = new ThinNeo.ScriptBuilder();

            sb.EmitParamJson(new MyJson.JsonNode_Array());//参数倒序入
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)name"));//参数倒序入
            byte[] shash = ThinNeo.Helper.HexString2Bytes(nnc);
            sb.EmitAppCall(shash.Reverse().ToArray());//nep5脚本

            sb.EmitParamJson(new MyJson.JsonNode_Array());
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)symbol"));
            sb.EmitAppCall(shash.Reverse().ToArray());

            sb.EmitParamJson(new MyJson.JsonNode_Array());
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)decimals"));
            sb.EmitAppCall(shash.Reverse().ToArray());

            sb.EmitParamJson(new MyJson.JsonNode_Array());
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)totalSupply"));
            sb.EmitAppCall(shash.Reverse().ToArray());
            var data = sb.ToArray();
            var scripthash = ThinNeo.Helper.Bytes2HexString(data);

            //api 是 https://api.nel.group/api/testne?jsonrpc=2.0&id=1&method=getstorage&params=[]
            string result = http.HttpGet(api + "?jsonrpc=2.0&id=1&method=invokescript&params=[\"" + scripthash + "\"]");
            Console.WriteLine("得到的结果是：" + result);

        }
    }
}
