using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thinWallet.dapp_plat
{

    public class DApp_Input
    {
        public string id;
        public string type;
        public string value;
        public string desc;

        public void Load(MyJson.JsonNode_Object json)
        {
            this.id = json["id"].AsString();
            this.type = json["type"].AsString();
            this.value = json["value"].ToString();
            this.desc = json["desc"].AsString();
        }
    }

    public class DApp_Coin
    {
        public string scripthash;
        public string asset;
        public ThinNeo.Fixed8 value;

        public void Load(MyJson.JsonNode_Object json)
        {
            this.scripthash = json["scripthash"].AsString();
            this.asset = json["asset"].AsString();
            if (json["value"] is MyJson.JsonNode_ValueNumber)
            {
                decimal v = (decimal)json["value"].AsDouble();
                this.value = v;
            }
            else if (json["value"] is MyJson.JsonNode_ValueString)
            {
                decimal v = decimal.Parse(json["value"].AsString());
                this.value = v;
            }
            else
            {
                throw new Exception("error value type.");
            }
        }
    }
    public class DApp_Witness
    {
        public string vscript;
        public MyJson.IJsonNode[] iscript;
        public void Load(MyJson.JsonNode_Object json)
        {
            this.iscript = json["iscript"].AsList().ToArray();
            this.vscript = json["vscript"].AsString();
        }
    }

    public class DApp_Call
    {
        public enum Type
        {
            getstorage,
            invokescript,
            sendrawtransaction,
        }
        public Type type;
        public string scriptcall;
        public MyJson.IJsonNode[] scriptparam;
        public double scriptfee;
        public DApp_Coin[] coins;
        public DApp_Witness[] witnesses;
        public void Load(MyJson.JsonNode_Object json)
        {
            this.type = (Type)Enum.Parse(typeof(Type), json["type"].AsString());
            this.scriptcall = json["scriptcall"].AsString();
            this.scriptparam = json["scriptparam"].AsList().ToArray();
            if (json.ContainsKey("scriptfee"))
            {
                this.scriptfee = json["scriptfee"].AsDouble();
            }
            if (json.ContainsKey("coins"))
            {
                var jsoncoins = json["coins"].AsList();
                this.coins = new DApp_Coin[jsoncoins.Count];
                for (var i = 0; i < jsoncoins.Count; i++)
                {
                    this.coins[i] = new DApp_Coin();
                    this.coins[i].Load(jsoncoins[i] as MyJson.JsonNode_Object);
                }
            }
            if (json.ContainsKey("witnesses"))
            {
                var jsonwits = json["witnesses"].AsList();
                this.witnesses = new DApp_Witness[jsonwits.Count];
                for (var i = 0; i < jsonwits.Count; i++)
                {
                    this.witnesses[i] = new DApp_Witness();
                    this.witnesses[i].Load(jsonwits[i] as MyJson.JsonNode_Object);
                }
            }
        }
    }
    public class DApp_Result
    {
        public string type;
        public string desc;

        public void Load(MyJson.JsonNode_Object json)
        {
            this.type = json["type"].AsString();
            this.desc = json["desc"].AsString();
        }
    }
    public class DApp_Func
    {
        public string name;
        public string desc;
        public DApp_Input[] inputs;
        public DApp_Call call;
        public DApp_Result[] results;
        public void Load(MyJson.JsonNode_Object json)
        {
            this.name = json["name"].AsString();

            if (json.ContainsKey("desc"))
            {
                this.desc = json["desc"].AsString();
            }
            else
            {
                this.desc = "";
            }
            var inputs = json["inputs"].AsList();
            this.inputs = new DApp_Input[inputs.Count];
            for (var i = 0; i < inputs.Count; i++)
            {
                this.inputs[i] = new DApp_Input();
                this.inputs[i].Load(inputs[i].AsDict());
            }

            var call = json["call"].AsDict();
            this.call = new DApp_Call();
            this.call.Load(call);

            if (json.ContainsKey("results"))
            {
                var results = json["results"].AsList();
                this.results = new DApp_Result[results.Count];
                for (var i = 0; i < results.Count; i++)
                {
                    this.results[i] = new DApp_Result();
                    this.results[i].Load(results[i].AsDict());
                }
            }
            else
            {
                this.results = new DApp_Result[0];
            }
        }
    }

    public class DApp_SimplePlugin
    {
        public override string ToString()
        {
            return Title;
        }
        public string Title;
        public Dictionary<string, string> consts = new Dictionary<string, string>();
        public DApp_Func[] funcs;
        public void LoadJson(string jsonstr)
        {
            this.Title = "";
            this.funcs = null;

            MyJson.JsonNode_Object json = MyJson.Parse(jsonstr).AsDict();
            if (json.ContainsKey("title"))
            {
                this.Title = json["title"].AsString();
            }
            var funcs = json["funcs"].AsList();
            this.funcs = new DApp_Func[funcs.Count];
            for (var i = 0; i < funcs.Count; i++)
            {
                this.funcs[i] = new DApp_Func();

                this.funcs[i].Load(funcs[i].AsDict());
            }
            if (json.ContainsKey("consts"))
            {
                foreach (var citem in json["consts"].AsDict())
                {
                    this.consts[citem.Key] = citem.Value.ToString();
                }
            }
        }
    }
    class DApp_Plat
    {
        public List<DApp_SimplePlugin> plugins = new List<DApp_SimplePlugin>();
        public void LoadSimplePlugins()
        {
            var jsons = System.IO.Directory.GetFiles("dapp", "*.json");
            foreach (var json in jsons)
            {
                var p = new DApp_SimplePlugin();
                try
                {
                    p.LoadJson(System.IO.File.ReadAllText(json));
                    plugins.Add(p);
                }
                catch (Exception err)
                {

                }
            }
        }
    }
}
