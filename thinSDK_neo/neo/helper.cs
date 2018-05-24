using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ThinNeo.Cryptography;
using ThinNeo.Cryptography.Cryptography;

namespace ThinNeo
{
    public static class Helper
    {
        public static int BitLen(int w)
        {
            return (w < 1 << 15 ? (w < 1 << 7
                ? (w < 1 << 3 ? (w < 1 << 1
                ? (w < 1 << 0 ? (w < 0 ? 32 : 0) : 1)
                : (w < 1 << 2 ? 2 : 3)) : (w < 1 << 5
                ? (w < 1 << 4 ? 4 : 5)
                : (w < 1 << 6 ? 6 : 7)))
                : (w < 1 << 11
                ? (w < 1 << 9 ? (w < 1 << 8 ? 8 : 9) : (w < 1 << 10 ? 10 : 11))
                : (w < 1 << 13 ? (w < 1 << 12 ? 12 : 13) : (w < 1 << 14 ? 14 : 15)))) : (w < 1 << 23 ? (w < 1 << 19
                ? (w < 1 << 17 ? (w < 1 << 16 ? 16 : 17) : (w < 1 << 18 ? 18 : 19))
                : (w < 1 << 21 ? (w < 1 << 20 ? 20 : 21) : (w < 1 << 22 ? 22 : 23))) : (w < 1 << 27
                ? (w < 1 << 25 ? (w < 1 << 24 ? 24 : 25) : (w < 1 << 26 ? 26 : 27))
                : (w < 1 << 29 ? (w < 1 << 28 ? 28 : 29) : (w < 1 << 30 ? 30 : 31)))));
        }
        public static int GetBitLength(this BigInteger i)
        {
            byte[] b = i.ToByteArray();
            return (b.Length - 1) * 8 + BitLen(i.Sign > 0 ? b[b.Length - 1] : 255 - b[b.Length - 1]);
        }

        public static int GetLowestSetBit(this BigInteger i)
        {
            if (i.Sign == 0)
                return -1;
            byte[] b = i.ToByteArray();
            int w = 0;
            while (b[w] == 0)
                w++;
            for (int x = 0; x < 8; x++)
                if ((b[w] & 1 << x) > 0)
                    return x + w * 8;
            throw new Exception();
        }

        public static BigInteger Mod(this BigInteger x, BigInteger y)
        {
            x %= y;
            if (x.Sign < 0)
                x += y;
            return x;
        }

        public static bool TestBit(this BigInteger i, int index)
        {
            return (i & (BigInteger.One << index)) > BigInteger.Zero;
        }
        internal static BigInteger ModInverse(this BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }

        internal static BigInteger NextBigInteger(this Random rand, int sizeInBits)
        {
            if (sizeInBits < 0)
                throw new ArgumentException("sizeInBits must be non-negative");
            if (sizeInBits == 0)
                return 0;
            byte[] b = new byte[sizeInBits / 8 + 1];
            rand.NextBytes(b);
            if (sizeInBits % 8 == 0)
                b[b.Length - 1] = 0;
            else
                b[b.Length - 1] &= (byte)((1 << sizeInBits % 8) - 1);
            return new BigInteger(b);
        }

        internal static BigInteger NextBigInteger(this System.Security.Cryptography.RandomNumberGenerator rng, int sizeInBits)
        {
            if (sizeInBits < 0)
                throw new ArgumentException("sizeInBits must be non-negative");
            if (sizeInBits == 0)
                return 0;
            byte[] b = new byte[sizeInBits / 8 + 1];
            rng.GetBytes(b);
            if (sizeInBits % 8 == 0)
                b[b.Length - 1] = 0;
            else
                b[b.Length - 1] &= (byte)((1 << sizeInBits % 8) - 1);
            return new BigInteger(b);
        }

        public static string Bytes2HexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var d in data)
            {
                sb.Append(d.ToString("x02"));
            }
            return sb.ToString();
        }
        public static byte[] HexString2Bytes(string str)
        {
            if (str.IndexOf("0x") == 0)
                str = str.Substring(2);
            byte[] outd = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                outd[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return outd;
        }
        static System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();
        static RIPEMD160Managed ripemd160 = new RIPEMD160Managed();
        //static System.Security.Cryptography.RIPEMD160 ripemd160 = System.Security.Cryptography.RIPEMD160.Create();

        public static string GetWifFromPrivateKey(byte[] prikey)
        {
            if (prikey.Length != 32)
                throw new Exception("error prikey.");
            byte[] data = new byte[34];
            data[0] = 0x80;
            data[33] = 0x01;
            for (var i = 0; i < 32; i++)
            {
                data[i + 1] = prikey[i];
            }
            byte[] checksum = sha256.ComputeHash(data);
            checksum = sha256.ComputeHash(checksum);
            checksum = checksum.Take(4).ToArray();
            byte[] alldata = data.Concat(checksum).ToArray();
            string wif = Base58.Encode(alldata);
            return wif;
        }
        public static byte[] GetPrivateKeyFromWIF(string wif)
        {
            if (wif == null) throw new ArgumentNullException();
            byte[] data = Base58.Decode(wif);
            //检查标志位
            if (data.Length != 38 || data[0] != 0x80 || data[33] != 0x01)
                throw new Exception("wif length or tag is error");
            //取出检验字节
            var sum = data.Skip(data.Length - 4);
            byte[] realdata = data.Take(data.Length - 4).ToArray();

            //验证,对前34字节进行进行两次hash取前4个字节
            byte[] checksum = sha256.ComputeHash(realdata);
            checksum = sha256.ComputeHash(checksum);
            var sumcalc = checksum.Take(4);
            if (sum.SequenceEqual(sumcalc) == false)
                throw new Exception("the sum is not match.");

            byte[] privateKey = new byte[32];
            Buffer.BlockCopy(data, 1, privateKey, 0, privateKey.Length);
            Array.Clear(data, 0, data.Length);
            return privateKey;
        }

        public static byte[] GetPublicKeyFromPrivateKey(byte[] privateKey)
        {
            var PublicKey = ThinNeo.Cryptography.ECC.ECCurve.Secp256r1.G * privateKey;
            return PublicKey.EncodePoint(true);
        }
        public static byte[] GetPublicKeyFromPrivateKey_NoComp(byte[] privateKey)
        {
            var PublicKey = ThinNeo.Cryptography.ECC.ECCurve.Secp256r1.G * privateKey;
            return PublicKey.EncodePoint(false);//.Skip(1).ToArray();
        }
        public static byte[] GetScriptFromPublicKey(byte[] publicKey)
        {
            byte[] script = new byte[publicKey.Length + 2];
            script[0] = (byte)publicKey.Length;
            Array.Copy(publicKey, 0, script, 1, publicKey.Length);
            script[script.Length - 1] = 172;//CHECKSIG
            return script;
        }
        public static Hash160 GetScriptHashFromScript(byte[] script)
        {
            var scripthash = sha256.ComputeHash(script);
            scripthash = ripemd160.ComputeHash(scripthash);
            return scripthash;
        }
        public static Hash160 GetScriptHashFromPublicKey(byte[] publicKey)
        {
            byte[] script = GetScriptFromPublicKey(publicKey);
            var scripthash = sha256.ComputeHash(script);
            scripthash = ripemd160.ComputeHash(scripthash);
            return scripthash;
        }
        public static string GetAddressFromScriptHash(Hash160 scripthash)
        {
            byte[] data = new byte[20 + 1];
            data[0] = 0x17;
            Array.Copy(scripthash, 0, data, 1, 20);
            var hash = sha256.ComputeHash(data);
            hash = sha256.ComputeHash(hash);

            var alldata = data.Concat(hash.Take(4)).ToArray();

            return Base58.Encode(alldata);
        }
        public static string GetAddressFromPublicKey(byte[] publickey)
        {
            byte[] scriptHash = GetScriptHashFromPublicKey(publickey);
            return GetAddressFromScriptHash(scriptHash);
        }
        //public static byte[] GetPublicKeyHash(byte[] publickey)
        //{
        //    var hash1 = sha256.ComputeHash(publickey);
        //    var hash2 = ripemd160.ComputeHash(hash1);
        //    return hash2;
        //}
        public static Hash160 GetPublicKeyHashFromAddress(string address)
        {
            var alldata = Base58.Decode(address);
            if (alldata.Length != 25)
                throw new Exception("error length.");
            var data = alldata.Take(alldata.Length - 4).ToArray();
            if (data[0] != 0x17)
                throw new Exception("not a address");
            var hash = sha256.ComputeHash(data);
            hash = sha256.ComputeHash(hash);
            var hashbts = hash.Take(4).ToArray();
            var datahashbts = alldata.Skip(alldata.Length - 4).ToArray();
            if (hashbts.SequenceEqual(datahashbts) == false)
                throw new Exception("not match hash");
            var pkhash = data.Skip(1).ToArray();
            return new Hash160(pkhash);
        }
        public static Hash160 GetPublicKeyHashFromAddress_WithoutCheck(string address)
        {
            var alldata = Base58.Decode(address);
            if (alldata.Length != 25)
                throw new Exception("error length.");
            if (alldata[0] != 0x17)
                throw new Exception("not a address");
            var data = alldata.Take(alldata.Length - 4).ToArray();
            var pkhash = data.Skip(1).ToArray();
            return new Hash160(pkhash);
        }




        public static byte[] Sign(byte[] message, byte[] prikey)
        {
            var Secp256r1_G = Helper.HexString2Bytes("04" + "6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296" + "4FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5");

            var PublicKey = ThinNeo.Cryptography.ECC.ECCurve.Secp256r1.G * prikey;
            var pubkey = PublicKey.EncodePoint(false).Skip(1).ToArray();

            var ecdsa = new ThinNeo.Cryptography.ECC.ECDsa(prikey, ThinNeo.Cryptography.ECC.ECCurve.Secp256r1);
            var hash = sha256.ComputeHash(message);
            var result = ecdsa.GenerateSignature(hash);
            var data1 = result[0].ToByteArray();
            if (data1.Length > 32)
                data1 = data1.Take(32).ToArray();
            var data2 = result[1].ToByteArray();
            if (data2.Length > 32)
                data2 = data2.Take(32).ToArray();

            data1 = data1.Reverse().ToArray();
            data2 = data2.Reverse().ToArray();

            byte[] newdata = new byte[64];
            Array.Copy(data1, 0, newdata, 32 - data1.Length, data1.Length);
            Array.Copy(data2, 0, newdata, 64 - data2.Length, data2.Length);

            return newdata;// data1.Concat(data2).ToArray();
            //#if NET461
            //const int ECDSA_PRIVATE_P256_MAGIC = 0x32534345;
            //byte[] first = { 0x45, 0x43, 0x53, 0x32, 0x20, 0x00, 0x00, 0x00 };
            //prikey = first.Concat(pubkey).Concat(prikey).ToArray();
            //using (System.Security.Cryptography.CngKey key = System.Security.Cryptography.CngKey.Import(prikey, System.Security.Cryptography.CngKeyBlobFormat.EccPrivateBlob))
            //using (System.Security.Cryptography.ECDsaCng ecdsa = new System.Security.Cryptography.ECDsaCng(key))

            //using (var ecdsa = System.Security.Cryptography.ECDsa.Create(new System.Security.Cryptography.ECParameters
            //{
            //    Curve = System.Security.Cryptography.ECCurve.NamedCurves.nistP256,
            //    D = prikey,
            //    Q = new System.Security.Cryptography.ECPoint
            //    {
            //        X = pubkey.Take(32).ToArray(),
            //        Y = pubkey.Skip(32).ToArray()
            //    }
            //}))
            //{
            //    var hash = sha256.ComputeHash(message);
            //    return ecdsa.SignHash(hash);
            //}
        }

        public static bool VerifySignature(byte[] message, byte[] signature, byte[] pubkey)
        {
            //unity dotnet不完整，不能用dotnet自带的ecdsa
            var PublicKey = ThinNeo.Cryptography.ECC.ECPoint.DecodePoint(pubkey, ThinNeo.Cryptography.ECC.ECCurve.Secp256r1);
            var ecdsa = new ThinNeo.Cryptography.ECC.ECDsa(PublicKey);
            var b1 = signature.Take(32).Reverse().Concat(new byte[] { 0x00 }).ToArray();
            var b2 = signature.Skip(32).Reverse().Concat(new byte[] { 0x00 }).ToArray();
            var num1 = new BigInteger(b1);
            var num2 = new BigInteger(b2);
            var hash = sha256.ComputeHash(message);
            return ecdsa.VerifySignature(hash, num1, num2);
            //var PublicKey = ThinNeo.Cryptography.ECC.ECPoint.DecodePoint(pubkey, ThinNeo.Cryptography.ECC.ECCurve.Secp256r1);
            //var usepk = PublicKey.EncodePoint(false).Skip(1).ToArray();

            ////byte[] first = { 0x45, 0x43, 0x53, 0x31, 0x20, 0x00, 0x00, 0x00 };
            ////usepk = first.Concat(usepk).ToArray();

            ////using (System.Security.Cryptography.CngKey key = System.Security.Cryptography.CngKey.Import(usepk, System.Security.Cryptography.CngKeyBlobFormat.EccPublicBlob))
            ////using (System.Security.Cryptography.ECDsaCng ecdsa = new System.Security.Cryptography.ECDsaCng(key))

            //using (var ecdsa = System.Security.Cryptography.ECDsa.Create(new System.Security.Cryptography.ECParameters
            //{
            //    Curve = System.Security.Cryptography.ECCurve.NamedCurves.nistP256,
            //    Q = new System.Security.Cryptography.ECPoint
            //    {
            //        X = usepk.Take(32).ToArray(),
            //        Y = usepk.Skip(32).ToArray()
            //    }
            //}))
            //{
            //    var hash = sha256.ComputeHash(message);
            //    return ecdsa.VerifyHash(hash, signature);
            //}
        }


        public static string GetNep2FromPrivateKey(byte[] prikey, string passphrase)
        {
            var pubkey = Helper.GetPublicKeyFromPrivateKey(prikey);
            var script_hash = Helper.GetScriptHashFromPublicKey(pubkey);

            string address = Helper.GetAddressFromScriptHash(script_hash);

            var b1 = Sha256(Encoding.ASCII.GetBytes(address));
            var b2 = Sha256(b1);
            byte[] addresshash = b2.Take(4).ToArray();
            byte[] derivedkey = SCrypt.DeriveKey(Encoding.UTF8.GetBytes(passphrase), addresshash, 16384, 8, 8, 64);
            byte[] derivedhalf1 = derivedkey.Take(32).ToArray();
            byte[] derivedhalf2 = derivedkey.Skip(32).ToArray();
            var xorinfo = XOR(prikey, derivedhalf1);
            byte[] encryptedkey = AES256Encrypt(xorinfo, derivedhalf2);
            byte[] buffer = new byte[39];
            buffer[0] = 0x01;
            buffer[1] = 0x42;
            buffer[2] = 0xe0;
            Buffer.BlockCopy(addresshash, 0, buffer, 3, addresshash.Length);
            Buffer.BlockCopy(encryptedkey, 0, buffer, 7, encryptedkey.Length);
            return Base58CheckEncode(buffer);
        }
        public static byte[] GetPrivateKeyFromNEP2(string nep2, string passphrase, int N = 16384, int r = 8, int p = 8)
        {
            if (nep2 == null) throw new ArgumentNullException(nameof(nep2));
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));
            byte[] data = Base58CheckDecode(nep2);
            if (data.Length != 39 || data[0] != 0x01 || data[1] != 0x42 || data[2] != 0xe0)
                throw new FormatException();
            byte[] addresshash = new byte[4];
            Buffer.BlockCopy(data, 3, addresshash, 0, 4);
            byte[] derivedkey = SCrypt.DeriveKey(Encoding.UTF8.GetBytes(passphrase), addresshash, N, r, p, 64);
            byte[] derivedhalf1 = derivedkey.Take(32).ToArray();
            byte[] derivedhalf2 = derivedkey.Skip(32).ToArray();
            byte[] encryptedkey = new byte[32];
            Buffer.BlockCopy(data, 7, encryptedkey, 0, 32);
            byte[] prikey = XOR(AES256Decrypt(encryptedkey, derivedhalf2), derivedhalf1);
            var pubkey = GetPublicKeyFromPrivateKey(prikey);
            var address = GetAddressFromPublicKey(pubkey);
            var hash = Sha256(Encoding.ASCII.GetBytes(address));
            hash = Sha256(hash);
            for (var i = 0; i < 4; i++)
            {
                if (hash[i] != addresshash[i])
                    throw new Exception("check error.");
            }
            //Cryptography.ECC.ECPoint pubkey = Cryptography.ECC.ECCurve.Secp256r1.G * prikey;
            //UInt160 script_hash = Contract.CreateSignatureRedeemScript(pubkey).ToScriptHash();
            //string address = ToAddress(script_hash);
            //if (!Encoding.ASCII.GetBytes(address).Sha256().Sha256().Take(4).SequenceEqual(addresshash))
            //    throw new FormatException();
            return prikey;


        }
        public static byte[] Sha256(byte[] data, int start = 0, int length = -1)
        {
            byte[] tdata = null;

            if (start == 0 && length == -1)
            {
                tdata = data;
            }
            else
            {
                tdata = new byte[length];
                Array.Copy(data, 0, tdata, 0, length);
            }
            System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(tdata);

        }
        public static byte[] Base58CheckDecode(string input)
        {
            byte[] buffer = ThinNeo.Cryptography.Cryptography.Base58.Decode(input);
            if (buffer.Length < 4) throw new FormatException();

            var b1 = Sha256(buffer, 0, buffer.Length - 4);

            byte[] checksum = Sha256(b1);

            if (!buffer.Skip(buffer.Length - 4).SequenceEqual(checksum.Take(4)))
                throw new FormatException();
            return buffer.Take(buffer.Length - 4).ToArray();
        }
        public static string Base58CheckEncode(byte[] data)
        {
            var b1 = Sha256(data);
            byte[] checksum = Sha256(b1);
            byte[] buffer = new byte[data.Length + 4];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
            Buffer.BlockCopy(checksum, 0, buffer, data.Length, 4);
            return ThinNeo.Cryptography.Cryptography.Base58.Encode(buffer);
        }
        static byte[] XOR(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) throw new ArgumentException();
            return x.Zip(y, (a, b) => (byte)(a ^ b)).ToArray();
        }
        internal static byte[] AES256Encrypt(byte[] block, byte[] key)
        {
            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = key;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;
                using (System.Security.Cryptography.ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(block, 0, block.Length);
                }
            }
        }
        internal static byte[] AES256Decrypt(byte[] block, byte[] key)
        {
            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = key;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;
                using (System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(block, 0, block.Length);
                }
            }
        }

        public static byte[] nameHash(string domain)
        {
            return Sha256(System.Text.Encoding.UTF8.GetBytes(domain));
        }

        public static byte[] nameHashSub(byte[] roothash, string subdomain)
        {
            var bs = System.Text.Encoding.UTF8.GetBytes(subdomain);
            if (bs.Length == 0)
                return roothash;

            byte[] domain = Sha256(bs).Concat(roothash).ToArray();
            return Sha256(domain);
        }
    }
}
