
namespace ThinNeo.NEP6
{
    public class ScryptParameters
    {
        public static ScryptParameters Default { get; } = new ScryptParameters(16384, 8, 8);

        public readonly int N, R, P;

        public ScryptParameters(int n, int r, int p)
        {
            this.N = n;
            this.R = r;
            this.P = p;
        }

        public static ScryptParameters FromJson(MyJson.JsonNode_Object json)
        {
            return new ScryptParameters((int)json["n"].AsInt(), (int)json["r"].AsInt(), (int)json["p"].AsInt());
        }

        public MyJson.JsonNode_Object ToJson()
        {
            MyJson.JsonNode_Object json = new MyJson.JsonNode_Object();
            json.SetDictValue("n", N);
            json.SetDictValue("r", R);
            json.SetDictValue("p", P);
            return json;
        }
    }
}
