using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace smartContractDemo
{
    public class SCDemo2:ITest
    {
        string api = "https://api.nel.group/api/testnet";

        public string Name => "智能合约3连 2/3";

        public string ID => "SC2/3";
        async public Task Demo()
        {
            string nnc = "0x3fccdb91c9bb66ef2446010796feb6ca4ed96b05".Replace("0x", "");
            string script = null;
            using (var sb = new ThinNeo.ScriptBuilder())
            {

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
                script = ThinNeo.Helper.Bytes2HexString(data);

            }

            //var url = Helper.MakeRpcUrl(api, "invokescript", new MyJson.JsonNode_ValueString(script));
            //string result = await Helper.HttpGet(url);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var result = await Helper.HttpPost(url, postdata);

            Console.WriteLine("得到的结果是：" + result);

        }
    }
}
