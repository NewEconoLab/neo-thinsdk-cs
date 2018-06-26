using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace signtool
{
    /// <summary>
    /// 一个key对象可以是一个简单账户，也可以是一个多签账户
    /// </summary>
    public class Key
    {
        public bool multisignkey;
        public byte[] prikey;
        public int MKey_NeedCount;
        public List<byte[]> MKey_Pubkeys;
        public override string ToString()
        {
            if (multisignkey == false)
            {
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                return address;
            }
            else
            {
                try
                {
                    return "M" + MKey_NeedCount + "/" + MKey_Pubkeys.Count + ":" + GetAddress();
                }
                catch
                {
                    return "M<error>";
                }
            }
        }
        public byte[] GetMultiContract()
        {
            if (!(1 <= this.MKey_NeedCount && MKey_NeedCount <= MKey_Pubkeys.Count && MKey_Pubkeys.Count <= 1024))
                throw new ArgumentException();
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                sb.EmitPushNumber(MKey_NeedCount);
                foreach (var pkey in this.MKey_Pubkeys)
                {
                    sb.EmitPushBytes(pkey);
                }
                sb.EmitPushNumber(MKey_Pubkeys.Count);
                sb.Emit(ThinNeo.VM.OpCode.CHECKMULTISIG);
                return sb.ToArray();
            }
        }
        public string GetAddress()
        {
            if (this.multisignkey == false)
            {
                return ToString();
            }
            else
            {//计算多签地址
                var contract = GetMultiContract();
                var scripthash = ThinNeo.Helper.GetScriptHashFromScript(contract);
                var address = ThinNeo.Helper.GetAddressFromScriptHash(scripthash);
                return address;
            }
        }
        public void AddPubkey(byte[] pubkey)
        {
            foreach (var k in this.MKey_Pubkeys)
            {
                var s1 = ThinNeo.Helper.Bytes2HexString(k);
                var s2 = ThinNeo.Helper.Bytes2HexString(pubkey);
                if (s1 == s2)
                    return;
            }
            this.MKey_Pubkeys.Add(pubkey);
            this.MKey_Pubkeys.Sort((a, b) =>
            {
                var pa = ThinNeo.Cryptography.ECC.ECPoint.DecodePoint(a, ThinNeo.Cryptography.ECC.ECCurve.Secp256r1);
                var pb = ThinNeo.Cryptography.ECC.ECPoint.DecodePoint(b, ThinNeo.Cryptography.ECC.ECCurve.Secp256r1);
                return pa.CompareTo(pb);
            });
        }
    }

}
