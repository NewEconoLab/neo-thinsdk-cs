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
            return null;
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
