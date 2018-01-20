using System.Linq;

namespace ThinNeo.NEP6
{
    public class NEP6Contract 
    {
        public byte[] Script;
        public static NEP6Contract FromJson(MyJson.JsonNode_Object json)
        {
            if (json == null) return null;
            return new NEP6Contract
            {
                Script = Helper.HexString2Bytes(json["script"].AsString()),
            };
        }

        public MyJson.JsonNode_Object ToJson()
        {
            MyJson.JsonNode_Object contract = new MyJson.JsonNode_Object();
            contract["script"] = new MyJson.JsonNode_ValueString(Helper.Bytes2HexString(Script));
            contract["parameters"] = new MyJson.JsonNode_Array();

            {
                MyJson.JsonNode_Object item = new MyJson.JsonNode_Object();
                item["name"] = new MyJson.JsonNode_ValueString("signature");
                item["type"] = new MyJson.JsonNode_ValueString("Signature");
                contract["parameters"].AsList().Add(item);
            }
            contract["deployed"] = new MyJson.JsonNode_ValueNumber(false);
            return contract;
        }
    }
}
