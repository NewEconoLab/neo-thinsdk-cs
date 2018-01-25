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
            this.value = json["value"].AsString();
            this.desc = json["desc"].AsString();
        }
    }
    public class DApp_Call
    {
        public enum Type
        {
            invokescript,
            invoketransaction,
        }
        public Type type;
        public string scriptcall;
        public string scriptparam;
        //public InputCoin coins;
        //public Witness witnesses;
        public void Load(MyJson.JsonNode_Object json)
        {
            this.type = (Type)Enum.Parse(typeof(Type), json["type"].AsString());
            this.scriptcall = json["scriptcall"].AsString();
            this.scriptparam = json["scriptparam"].ToString();

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
            this.desc = json["desc"].AsString();
            var inputs = json["inputs"].AsList();
            var call = json["call"].AsDict();
            var results = json["results"].AsList();

            this.inputs = new DApp_Input[inputs.Count];
            for (var i = 0; i < inputs.Count; i++)
            {
                this.inputs[i] = new DApp_Input();
                this.inputs[i].Load(inputs[i].AsDict());
            }
            this.call = new DApp_Call();
            this.call.Load(call);

            this.results = new DApp_Result[results.Count];
            for (var i = 0; i < results.Count; i++)
            {
                this.results[i] = new DApp_Result();
                this.results[i].Load(results[i].AsDict());
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
                catch
                {

                }
            }
        }
    }
}
