using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinNeo.Debug.Helper
{

    public class AddrMap
    {
        List<MethodInfo> methods = new List<MethodInfo>();
        public int GetAddr(int line)
        {
            foreach (var m in methods)
            {
                var i = m.GetAddr(line);
                if (i > 0)
                    return i + m.startAddr;
            }
            return -1;
        }
        public int GetAddrBack(int line)
        {
            foreach (var m in methods)
            { 
                var i = m.GetAddrBack(line);
                if (i > 0)
                    return i + m.startAddr;
            }
            return -1;
        }
        public int GetLine(int addr)
        {
            foreach (var m in methods)
            {
                var i = m.GetLine(addr);
                if (i > 0)
                    return i;
            }
            return -1;
        }
        public int GetLineBack(int addr)
        {
            for (var _i = methods.Count - 1;_i >= 0;_i--)
            {
                var m = methods[_i];
                var i = m.GetLineBack(addr);
                if (i > 0)
                    return i;
            }
            return -1;
        }
        public static AddrMap FromJsonStr(string jsonstr)
        {
            return FromJson(MyJson.Parse(jsonstr) as MyJson.JsonNode_Array);
        }
        static AddrMap FromJson(MyJson.JsonNode_Array json)
        {
            AddrMap info = new AddrMap();
            foreach (var item in json)
            {
                MethodInfo minfo = new MethodInfo();
                minfo.name = item.GetDictItem("name").AsString();
                minfo.startAddr = int.Parse(item.GetDictItem("addr").AsString(), System.Globalization.NumberStyles.HexNumber);
                var map = item.GetDictItem("map").AsList();
                foreach (var mapitem in map)
                {
                    var src = int.Parse(mapitem.ToString().Substring(5));
                    var addr = int.Parse(mapitem.ToString().Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
                    if (src < 0 || src >= 0xffff) continue;
                    minfo.Add(src, addr);

                }
                minfo.Sort();
                info.methods.Add(minfo);
                info.methods.Sort((a, b) =>
                {
                    return a.startAddr - b.startAddr;
                });
            }
            
            //        [{"name":"Main","addr":"0000","map":["0011-11","0012-12","0023-14","004C-16707566","0054-15","0055-16","0074-17","007F-19","00B0-25","00C2-26","00D4-27","00E1-29","0101-32","0139-33","0155-34","0160-35"]
            //}]
            return info;
        }
        public class MethodInfo
        {
            public string name;
            public int startAddr;
            public void Add(int line, int addr)
            {
                addr2line[addr] = line;
                if (line2addr.ContainsKey(line) == false)
                    line2addr[line] = addr;
                if (lines.Contains(line) == false)
                    lines.Add(line);
                if (addrs.Contains(addr) == false)
                    addrs.Add(addr);
            }
            public void Sort()
            {

            }
            public Dictionary<int, int> addr2line = new Dictionary<int, int>();
            public Dictionary<int, int> line2addr = new Dictionary<int, int>();
            public List<int> lines = new List<int>();
            public List<int> addrs = new List<int>();
            public int GetAddr(int line)
            {
                if (line > line2addr.Keys.Max()) return -1;

                for (var i = 0; ; i++)
                {
                    if (line2addr.ContainsKey(line + i))
                        return line2addr[line + i];
                }
            }
            public int GetAddrBack(int line)
            {
                if (line2addr.Count == 0) return -1;
                if (line < line2addr.Keys.Min()) return -1;

                for (var i = 0; ; i--)
                {
                    if (line2addr.ContainsKey(line + i))
                        return line2addr[line + i];
                }
            }
            public int GetLine(int addr)
            {
                if (addr > addr2line.Keys.Max()) return -1;

                for (var i = 0; ; i++)
                {
                    if (addr2line.ContainsKey(addr + i))
                        return addr2line[addr + i];
                }
            }
            public int GetLineBack(int addr)
            {
                if (addr2line.Count == 0)
                    return -1;
                if (addr < addr2line.Keys.Min()) return -1;

                for (var i = 0; ; i--)
                {
                    if (addr2line.ContainsKey(addr + i))
                        return addr2line[addr + i];
                }
            }
        }

    }
}
