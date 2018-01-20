using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinNeo.Debug
{
    public class DebugScript
    {
        public string srcfile;
        public Compiler.Op[] codes;
        public Helper.AddrMap maps;
    }
    public class DebugTool
    {
        public string pathScript;
        public Dictionary<string, DebugScript> scripts = new Dictionary<string, DebugScript>();
        public SmartContract.Debug.FullLog fullLog;
        public SimVM simvm = new SimVM();
        public bool LoadScript(string scriptid)
        {
            var scriptAvm = System.IO.Path.Combine(pathScript, scriptid + ".avm");
            var scriptMap = System.IO.Path.Combine(pathScript, scriptid + ".map.json");
            var scriptSrc = System.IO.Path.Combine(pathScript, scriptid + ".cs");
            if (System.IO.File.Exists(scriptAvm) == false)
            {
                return false;
            }
            var debug = new DebugScript();
            debug.srcfile = System.IO.File.ReadAllText(scriptSrc);
            debug.codes = Compiler.Avm2Asm.Trans(System.IO.File.ReadAllBytes(scriptAvm));
            var jsonstr = System.IO.File.ReadAllText(scriptMap);
            debug.maps = Helper.AddrMap.FromJsonStr(jsonstr);
            scripts[scriptid] = debug;
            return true;
        }
        public void Load(string _pathlog, string _pathscript, string transid)
        {
            this.pathScript = _pathscript;
            if (transid.ToLower().IndexOf("0x") == 0)
            {
                transid = transid.Substring(2);
            }
            byte[] info = HexString2Bytes(transid);
            var filename = "0x" + ToHexString(info);

            var tranfile = System.IO.Path.Combine(_pathlog, filename + ".llvmhex.txt");


            if (System.IO.File.Exists(tranfile) == false)
            {
                throw new Exception("load file error");
            }
            var str = System.IO.File.ReadAllText(tranfile);
            var bts = ThinNeo.Helper.HexString2Bytes(str);
            using (var ms = new System.IO.MemoryStream(bts))
            {
                var outms = llvm.QuickFile.FromFile(ms);
                var text = System.Text.Encoding.UTF8.GetString(outms.ToArray());
                var json = MyJson.Parse(text) as MyJson.JsonNode_Object;
                fullLog = SmartContract.Debug.FullLog.FromJson(json);
            }
            //循环所有的
            simvm.Execute(fullLog);
        }
        public static byte[] HexString2Bytes(string str)
        {
            byte[] b = new byte[str.Length / 2];
            for (var i = 0; i < b.Length; i++)
            {
                b[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return b;
        }
        public static string ToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.Append(b.ToString("x02"));
            }
            return sb.ToString();
        }
    }
}
