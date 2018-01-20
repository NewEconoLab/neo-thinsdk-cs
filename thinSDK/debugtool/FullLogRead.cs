using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinNeo.SmartContract.Debug
{
    [Flags]
    public enum VMState : byte
    {
        NONE = 0,

        HALT = 1 << 0,
        FAULT = 1 << 1,
        BREAK = 1 << 2,
    }
    public enum OpType
    {
        Non,
        Clear,
        Insert,
        Peek,
        Pop,
        Push,
        Remove,
        Set,
    }
    public struct Op
    {
        public Op(OpType type, int ind = -1)
        {
            this.type = type;
            this.ind = ind;
        }
        public OpType type;
        public int ind;
    }
    public class StackItem
    {
        public string type;
        public string strvalue;
        public List<StackItem> subItems;
        public StackItem Clone()
        {
            if (this.type != "Struct")
                return this;
            StackItem item = new StackItem();
            item.type = this.type;
            item.strvalue = this.strvalue;
            if (this.subItems != null)
            {
                item.subItems = new List<StackItem>();
                foreach (var i in this.subItems)
                {
                    item.subItems.Add(i.Clone());
                }
            }
            return item;
        }
        public int AsInt()
        {
            return int.Parse(strvalue);
        }
        public System.Numerics.BigInteger AsBigInteger()
        {
            return System.Numerics.BigInteger.Parse(strvalue);
        }
        public byte[] asBytes()
        {
            return null;
        }
        public override string ToString()
        {
            if (type == "Array" || type == "Struct")
            {
                string outstr = type + "[";
                for (var i = 0; i < subItems.Count; i++)
                {
                    outstr += subItems[i].ToShortString();
                    if (i != subItems.Count - 1)
                    {
                        outstr += ",";
                    }
                }
                outstr += "]";
                return outstr;
            }
            else
            {
                return type + ":" + strvalue;
            }
        }
        public string ToShortString()
        {
            if (type == "Array" || type == "Struct")
            {
                string outstr = "[";
                for (var i = 0; i < subItems.Count; i++)
                {
                    outstr += subItems[i].ToShortString();
                    if (i != subItems.Count - 1)
                    {
                        outstr += ",";
                    }
                }
                outstr += "]";
                return outstr;
            }
            else
            {
                return strvalue;
            }
        }
        //public static MyJson.JsonNode_Object StatkItemToJson(StackItem item)
        //{
        //    //MyJson.JsonNode_Object json = new MyJson.JsonNode_Object();
        //    //var type = item.GetType().Name;
        //    //if (type == "InteropInterface")
        //    //{
        //    //    json.SetDictValue(type, item.GetInterface<VM.IInteropInterface>().GetType().Name);
        //    //}
        //    //else if (type == "Boolean")
        //    //{
        //    //    json.SetDictValue(type, item.GetBoolean().ToString());
        //    //}
        //    //else if (type == "ByteArray")
        //    //{
        //    //    json.SetDictValue(type, item.GetByteArray().ToHexString());
        //    //}
        //    //else if (type == "Integer")
        //    //{
        //    //    json.SetDictValue(type, item.GetBigInteger().ToString());
        //    //}
        //    //else if (item.IsArray || item.IsStruct)
        //    //{
        //    //    MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
        //    //    json.SetDictValue(type, array);
        //    //    foreach (var i in item.GetArray())
        //    //    {
        //    //        array.Add(StatkItemToJson(i));
        //    //    }
        //    //}
        //    //return json;
        //}
        public static StackItem FromJson(MyJson.JsonNode_Object json)
        {
            StackItem item = new StackItem();
            item.type = json.Keys.ToArray()[0];
            var strvalue = json[item.type] as MyJson.JsonNode_ValueString;
            var arrayvalue = json[item.type] as MyJson.JsonNode_Array;
            if (arrayvalue == null)
            {
                item.strvalue = strvalue.AsString();
            }
            else
            {
                item.subItems = new List<StackItem>();
                foreach (var subitem in arrayvalue)
                {
                    item.subItems.Add(StackItem.FromJson(subitem as MyJson.JsonNode_Object));
                }
            }
            return item;
        }
    }

    public class LogScript
    {
        public int GetAllScriptName(List<string> names)
        {
            names.Add(this.hash);
            int scount = 1;
            foreach (var op in ops)
            {
                if (op.subScript != null)
                {
                    scount += op.subScript.GetAllScriptName(names);
                }
            }
            return scount;
        }
        public LogScript parent;
        public string hash;
        public List<LogOp> ops = new List<LogOp>();
        public LogScript(string hash)
        {
            this.hash = hash;
        }
        //public MyJson.JsonNode_Object ToJson()
        //{
        //    MyJson.JsonNode_Object script = new MyJson.JsonNode_Object();
        //    script.SetDictValue("hash", this.hash);
        //    MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
        //    script.SetDictValue("ops", array);
        //    foreach (var op in ops)
        //    {
        //        array.Add(op.ToJson());
        //    }
        //    return script;
        //}
        public static LogScript FromJson(MyJson.JsonNode_Object json)
        {
            var hash = json["hash"].AsString();
            LogScript script = new LogScript(hash);
            var array = json["ops"].AsList();
            foreach (var op in array)
            {
                script.ops.Add(LogOp.FromJson(op as MyJson.JsonNode_Object));
                var ss = script.ops.Last().subScript;
                if (ss != null)
                    ss.parent = script;
            }
            return script;

        }
        public LogScript Clone()
        {
            LogScript s = new LogScript(this.hash);
            s.parent = this;
            s.ops = new List<LogOp>();
            foreach(var o in this.ops)
            {
                s.ops.Add(o.Clone());
            }

            return s;
        }
    }
    public class LogOp
    {
        public int addr;
        public VM.OpCode op;
        public bool error;
        //public string syscall;
        //public string[] syscallinfo;
        public Op[] stack;
        public byte[] param;
        public StackItem opresult;
        public string GetHeader()
        {
            string name = addr.ToString("x04") + ":";
            if (op > VM.OpCode.PUSHBYTES1 && op < VM.OpCode.PUSHBYTES75)
                return name + "PUSHBYTES" + (op - VM.OpCode.PUSHBYTES1);
            else
                return name + op.ToString();
        }
        public LogOp(int addr, VM.OpCode op)
        {
            this.addr = addr;
            this.op = op;
        }
        public LogScript subScript;
        //public static MyJson.JsonNode_Object StatkItemToJson(StackItem item)
        //{
        //    //MyJson.JsonNode_Object json = new MyJson.JsonNode_Object();
        //    //var type = item.GetType().Name;
        //    //if (type == "InteropInterface")
        //    //{
        //    //    json.SetDictValue(type, item.GetInterface<VM.IInteropInterface>().GetType().Name);
        //    //}
        //    //else if (type == "Boolean")
        //    //{
        //    //    json.SetDictValue(type, item.GetBoolean().ToString());
        //    //}
        //    //else if (type == "ByteArray")
        //    //{
        //    //    json.SetDictValue(type, item.GetByteArray().ToHexString());
        //    //}
        //    //else if (type == "Integer")
        //    //{
        //    //    json.SetDictValue(type, item.GetBigInteger().ToString());
        //    //}
        //    //else if (item.IsArray || item.IsStruct)
        //    //{
        //    //    MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
        //    //    json.SetDictValue(type, array);
        //    //    foreach (var i in item.GetArray())
        //    //    {
        //    //        array.Add(StatkItemToJson(i));
        //    //    }
        //    //}
        //    //return json;
        //}
        //public MyJson.JsonNode_Object ToJson()
        //{
        //    MyJson.JsonNode_Object _op = new MyJson.JsonNode_Object();
        //    _op.SetDictValue("addr", addr);
        //    _op.SetDictValue("op", op.ToString());
        //    if (this.stack != null)
        //    {
        //        MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
        //        _op.SetDictValue("stack", array);
        //        foreach (var r in stack)
        //        {
        //            if (r.ind > 0)
        //            {
        //                array.AddArrayValue(r.type.ToString() + "|" + r.ind);
        //            }
        //            else
        //            {
        //                array.AddArrayValue(r.type.ToString());
        //            }
        //        }
        //    }
        //    if (opresult != null)
        //    {
        //        _op.SetDictValue("result", StatkItemToJson(opresult));
        //    }
        //    if (subScript != null)
        //    {
        //        _op.SetDictValue("subscript", subScript.ToJson());
        //    }
        //    return _op;
        //}
        public static LogOp FromJson(MyJson.JsonNode_Object json)
        {
            var opstr = json["op"].AsString();
            var addr = json["addr"].AsInt();
            var op = (VM.OpCode)Enum.Parse(typeof(VM.OpCode), opstr);
            LogOp _op = new LogOp(addr, op);
            if (json.ContainsKey("stack"))
            {

                var array = json["stack"].AsList();
                _op.stack = new Op[array.Count];
                for (var i = 0; i < array.Count; i++)
                {
                    var str = array[i].AsString();
                    var ind = -1;
                    if (str.Contains('|'))
                    {
                        var strs = str.Split('|');
                        ind = int.Parse(strs[1]);
                        str = strs[0];
                    }
                    var type = (OpType)Enum.Parse(typeof(OpType), str);
                    _op.stack[i] = new Op(type, ind);
                }
            }
            if (json.ContainsKey("param"))
            {
                _op.param = ThinNeo.Debug.DebugTool.HexString2Bytes(json["param"].AsString());
            }
            if (json.ContainsKey("result"))
            {
                _op.opresult = StackItem.FromJson(json["result"] as MyJson.JsonNode_Object);
            }
            if (json.ContainsKey("subscript"))
            {
                _op.subScript = LogScript.FromJson(json["subscript"] as MyJson.JsonNode_Object);
            }
            return _op;
        }
        public LogOp Clone()
        {
            LogOp op = new LogOp(this.addr,this.op);
            op.error = this.error;
            if (this.stack != null)
            {
                op.stack = new Op[this.stack.Length];
                for (var i = 0; i < this.stack.Length; i++)
                {
                    op.stack[i] = this.stack[i];
                }
            }
            if (this.param != null)
            {
                op.param = this.param.Clone() as byte[];
            }
            if (this.opresult != null)
            {
                op.opresult = this.opresult.Clone();
            }
            op.subScript = this.subScript;
            return op;
        }
    }


    public class FullLog
    {
        public LogScript script = null;
        public string error = null;
        public VMState state = VMState.NONE;

        LogScript curScript = null;
        LogOp curOp = null;
        //public void LoadScript(string hash)
        //{
        //    if (script == null)
        //    {
        //        script = new LogScript(hash);
        //        curScript = script;
        //    }
        //    else
        //    {
        //        curOp.subScript = new LogScript(hash);
        //        curOp.subScript.parent = curScript;
        //        curScript = curOp.subScript;
        //    }
        //}
        //public void NextOp(int addr, VM.OpCode op)
        //{
        //    LogOp _op = new LogOp(addr, op);
        //    curScript.ops.Add(_op);
        //    curOp = _op;
        //    if (op == VM.OpCode.RET || op == VM.OpCode.TAILCALL)
        //    {
        //        curScript = curScript.parent;
        //    }
        //}
        //public void OPStackRecord(Op[] records)
        //{
        //    curOp.stack = records;
        //}
        //public void OpResult(StackItem item)
        //{
        //    curOp.opresult = item;
        //}
        //public void Error(string info)
        //{
        //    this.error = info;
        //}
        //public void Finish(VMState state)
        //{
        //    this.state = state;
        //}
        //public void Save(string filename)
        //{
        //    var path = System.IO.Path.GetDirectoryName(filename);
        //    if (System.IO.Directory.Exists(path) == false)
        //        System.IO.Directory.CreateDirectory(path);

        //    System.IO.File.Delete(filename + ".json");
        //    System.IO.File.Delete(filename);

        //    var json = new MyJson.JsonNode_Object();
        //    json.SetDictValue("script", script.ToJson());
        //    if (string.IsNullOrEmpty(error) == false)
        //        json.SetDictValue("error", error);
        //    json.SetDictValue("VMState", state.ToString());

        //    StringBuilder sb = new StringBuilder();
        //    json.ConvertToStringWithFormat(sb, 0);
        //    System.IO.File.WriteAllText(filename + ".json", sb.ToString());

        //    var compressor = new SevenZip.SevenZipCompressor();
        //    compressor.CompressionMethod = SevenZip.CompressionMethod.Lzma2;
        //    compressor.CompressionLevel = SevenZip.CompressionLevel.Fast;
        //    compressor.FastCompression = true;

        //    //compressor.path = path;
        //    compressor.CompressFiles(filename, System.IO.Path.GetFullPath(filename + ".json"));
        //    System.IO.File.Delete(filename + ".json");

        //}
        public static FullLog FromJson(MyJson.JsonNode_Object json)
        {
            FullLog fulllog = new FullLog();
            if (json.ContainsKey("error"))
                fulllog.error = json["error"].AsString();
            if (json.ContainsKey("VMState"))
            {
                var state = json["VMState"].AsString();
                fulllog.state = (VMState)Enum.Parse(typeof(VMState), state);
            }
            if (json.ContainsKey("script"))
            {
                fulllog.script = LogScript.FromJson(json["script"] as MyJson.JsonNode_Object);
            }
            return fulllog;
        }
    }
}
