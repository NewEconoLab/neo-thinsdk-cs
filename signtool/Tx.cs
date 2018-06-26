using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace signtool
{
    public enum KeyType
    {
        Unknown,
        Simple,
        MultiSign,
    }

    public class KeyInfo
    {
        public string keyaddress;
        public KeyType type;
        public Key MultiSignKey;
        public byte[] pubkey;
        /// <summary>
        /// unknown 无签名，
        /// simple signdata 只有一个
        /// multisign，和 pubkey数量相同，没签就空着
        /// </summary>
        public List<byte[]> signdata;
    }

    /// <summary>
    /// 一个TX对象表达一个交易
    /// </summary>
    public class Tx
    {
        public ThinNeo.Transaction txraw;
        public Dictionary<string, KeyInfo> keyinfos;
        public bool HasKeyInfo
        {
            get
            {
                foreach (var k in keyinfos)
                {
                    if (k.Value.type != KeyType.Unknown)
                        return true;
                }
                return false;
            }
        }
        public bool HasAllKeyInfo
        {
            get
            {
                foreach (var k in keyinfos)
                {
                    if (k.Value.type == KeyType.Unknown)
                        return false;
                    if (k.Value.type == KeyType.Simple)
                    {
                        if (k.Value.signdata == null || k.Value.signdata[0] == null || k.Value.signdata[0].Length == 0)
                            return false;
                    }
                    if (k.Value.type == KeyType.MultiSign)
                    {
                        var m = k.Value.MultiSignKey.MKey_NeedCount;
                        var c = 0;
                        for (var i = 0; i < k.Value.MultiSignKey.MKey_Pubkeys.Count; i++)
                        {
                            var data = k.Value.signdata[i];
                            if (data != null && data.Length > 0)
                                c++;
                        }
                        if (c < m)
                            return false;
                    }
                }
                return true;
            }
        }
        public void FillRaw()
        {
            this.txraw.witnesses = new ThinNeo.Witness[keyinfos.Count];
            List<KeyInfo> keys = new List<KeyInfo>();
            foreach (var key in keyinfos)
            {
                keys.Add(key.Value);
            }
            //keys 這個需要排序
            for (var i = 0; i < keys.Count; i++)
            {
                this.txraw.witnesses[i] = new ThinNeo.Witness();
                if (keys[i].type == KeyType.Simple)
                {
                    //算出vscript
                    this.txraw.witnesses[i].VerificationScript = ThinNeo.Helper.GetScriptFromPublicKey(keys[i].pubkey);
                    using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
                    {
                        sb.EmitPushBytes(keys[i].signdata[0]);
                        this.txraw.witnesses[i].InvocationScript = sb.ToArray();
                    }
                }
                if (keys[i].type == KeyType.MultiSign)
                {
                    //算出vscript
                    this.txraw.witnesses[i].VerificationScript = keys[i].MultiSignKey.GetMultiContract();
                    List<byte[]> signs = new List<byte[]>();
                    foreach (var s in keys[i].signdata)
                    {
                        if (s != null && s.Length > 0)
                            signs.Add(s);
                    }
                    //?這個signs 要不要倒序？试一试
                    using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
                    {
                        for (var iss = 0; iss < keys[i].MultiSignKey.MKey_NeedCount; iss++)
                        {
                            sb.EmitPushBytes(signs[iss]);
                        }
                        this.txraw.witnesses[i].InvocationScript = sb.ToArray();
                    }

                }
            }
        }
        public override string ToString()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                txraw.SerializeUnsigned(ms);
                var data = ms.ToArray();
                var outstr = ThinNeo.Helper.Bytes2HexString(data);
                if (HasKeyInfo)
                {
                    var json = ExportKeyInfo();
                    outstr += "|" + json.ToString();
                }
                return outstr;
            }
        }
        MyJson.JsonNode_Object ExportKeyInfo()
        {
            MyJson.JsonNode_Object json = new MyJson.JsonNode_Object();
            foreach (var k in keyinfos)
            {
                if (k.Value.type == KeyType.Unknown)
                    continue;
                MyJson.JsonNode_Object keyitem = new MyJson.JsonNode_Object();
                json.SetDictValue(k.Value.keyaddress, keyitem);
                keyitem.SetDictValue("type", k.Value.type.ToString());
                if (k.Value.type == KeyType.Simple)
                {
                    var strsigndata = k.Value.signdata[0] == null ? "<null>" : ThinNeo.Helper.Bytes2HexString(k.Value.signdata[0]);
                    keyitem.SetDictValue("sign0", strsigndata);
                    var strpubkey = k.Value.pubkey == null ? "<null>" : ThinNeo.Helper.Bytes2HexString(k.Value.pubkey);
                    keyitem.SetDictValue("pkey0", strpubkey);
                }
                else if (k.Value.type == KeyType.MultiSign)
                {
                    keyitem.SetDictValue("m", k.Value.MultiSignKey.MKey_NeedCount);
                    keyitem.SetDictValue("c", k.Value.MultiSignKey.MKey_Pubkeys.Count);
                    for (var i = 0; i < k.Value.MultiSignKey.MKey_Pubkeys.Count; i++)
                    {
                        var strpubkey = ThinNeo.Helper.Bytes2HexString(k.Value.MultiSignKey.MKey_Pubkeys[i]);
                        keyitem.SetDictValue("pkey" + i, strpubkey);
                        var strsigndata = k.Value.signdata[i] == null ? "<null>" : ThinNeo.Helper.Bytes2HexString(k.Value.signdata[i]);
                        keyitem.SetDictValue("sign" + i, strsigndata);
                    }
                }
            }
            return json;
        }
        public void ImportKeyInfo(IList<Key> keys, MyJson.JsonNode_Object json = null)
        {
            if (keyinfos == null)
                keyinfos = new Dictionary<string, KeyInfo>();

            //检查需要的签名
            foreach (var i in txraw.inputs)
            {
                //要找api要这个utxo的归属，添加到 keyinfo里，要签名
                //要网络晚些弄，留给印玮
            }
            foreach (var att in txraw.attributes)
            {
                if (att.usage == ThinNeo.TransactionAttributeUsage.Script)
                {//附加鉴证，有这个，说明需要这个签名
                    var addr = ThinNeo.Helper.GetAddressFromScriptHash(att.data);
                    if (keyinfos.ContainsKey(addr) == false)
                    {
                        keyinfos[addr] = new KeyInfo();
                        keyinfos[addr].type = KeyType.Unknown;
                        keyinfos[addr].keyaddress = addr;
                    }
                    foreach (var k in keys)
                    {
                        if (k.GetAddress() == addr)
                        {
                            if (k.multisignkey == false)
                            {
                                keyinfos[addr].type = KeyType.Simple;
                                keyinfos[addr].pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(k.prikey);
                                if (keyinfos[addr].signdata == null)
                                {
                                    keyinfos[addr].signdata = new List<byte[]>();
                                    keyinfos[addr].signdata.Add(null);
                                }
                            }
                            else
                            {
                                keyinfos[addr].type = KeyType.MultiSign;
                                keyinfos[addr].MultiSignKey = k;
                                if (keyinfos[addr].signdata == null)
                                {
                                    keyinfos[addr].signdata = new List<byte[]>();
                                    for (var i = 0; i < k.MKey_Pubkeys.Count; i++)
                                        keyinfos[addr].signdata.Add(null);
                                }
                            }
                        }
                    }
                }
            }


            //从json填入已经做了的签名
            if (json != null)
            {
                foreach (var k in json)
                {
                    if (keyinfos.ContainsKey(k.Key))
                    {
                        var type = k.Value.GetDictItem("type").AsString();
                        Enum.TryParse<KeyType>(type, out keyinfos[k.Key].type);
                        if (keyinfos[k.Key].type == KeyType.Simple)
                        {
                            if (keyinfos[k.Key].signdata == null)
                            {
                                keyinfos[k.Key].signdata = new List<byte[]>();
                                keyinfos[k.Key].signdata.Add(null);
                            }
                            var data = k.Value.GetDictItem("sign0").AsString();
                            if (data != "<null>")
                            {
                                keyinfos[k.Key].signdata[0] = ThinNeo.Helper.HexString2Bytes(data);
                            }
                            var pkey = k.Value.GetDictItem("pkey0").AsString();
                            keyinfos[k.Key].pubkey = ThinNeo.Helper.HexString2Bytes(pkey);
                        }
                        if (keyinfos[k.Key].type == KeyType.MultiSign)
                        {

                            var m = k.Value.GetDictItem("m").AsInt();
                            var c = k.Value.GetDictItem("c").AsInt();
                            List<byte[]> pubkeys = new List<byte[]>();
                            if (keyinfos[k.Key].signdata == null)
                            {
                                keyinfos[k.Key].signdata = new List<byte[]>();
                                for (var i = 0; i < c; i++)
                                {
                                    keyinfos[k.Key].signdata.Add(null);
                                }
                            }
                            for (var i = 0; i < c; i++)
                            {
                                var data = k.Value.GetDictItem("sign" + i).AsString();
                                if (data != "<null>")
                                {
                                    keyinfos[k.Key].signdata[i] = ThinNeo.Helper.HexString2Bytes(data);
                                }
                                var pkey = k.Value.GetDictItem("pkey" + i).AsString();
                                pubkeys.Add(ThinNeo.Helper.HexString2Bytes(pkey));
                            }
                            Key key = null;
                            foreach (var _key in keys)
                            {
                                if (_key.GetAddress() == k.Key)
                                {
                                    key = _key;
                                    break;
                                }
                            }
                            //没有这个key，直接导入
                            if (key == null)
                            {
                                key = new Key();
                                key.MKey_NeedCount = m;
                                key.MKey_Pubkeys = pubkeys;
                                key.multisignkey = true;
                                key.prikey = null;
                                keys.Add(key);
                            }
                            keyinfos[k.Key].MultiSignKey = key;
                        }
                    }
                }
                //从json导入keyinfo
            }
        }
        public void FromString(IList<Key> keys, string info)
        {
            byte[] txdata;
            //有附加信息
            MyJson.JsonNode_Object keyinfo = null;
            if (info.Contains("|"))
            {
                var ss = info.Split('|');
                txdata = ThinNeo.Helper.HexString2Bytes(ss[0]);
                keyinfo = MyJson.Parse(ss[1]).AsDict();
            }
            else
            {
                txdata = ThinNeo.Helper.HexString2Bytes(info);
            }
            txraw = new ThinNeo.Transaction();
            using (var ms = new System.IO.MemoryStream(txdata))
            {
                txraw.Deserialize(ms);
            }
            ImportKeyInfo(keys, keyinfo);

        }

    }
}
