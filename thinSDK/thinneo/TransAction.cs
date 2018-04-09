using ThinNeo.Cryptography.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ThinNeo
{
    public enum TransactionType : byte
    {
        /// <summary>
        /// 用于分配字节费的特殊交易
        /// </summary>
        MinerTransaction = 0x00,
        /// <summary>
        /// 用于分发资产的特殊交易
        /// </summary>
        IssueTransaction = 0x01,
        /// <summary>
        /// 用于分配小蚁币的特殊交易
        /// </summary>
        ClaimTransaction = 0x02,
        /// <summary>
        /// 用于报名成为记账候选人的特殊交易
        /// </summary>
        EnrollmentTransaction = 0x20,
        /// <summary>
        /// 用于资产登记的特殊交易
        /// </summary>
        RegisterTransaction = 0x40,
        /// <summary>
        /// 合约交易，这是最常用的一种交易
        /// </summary>
        ContractTransaction = 0x80,
        /// <summary>
        /// Publish scripts to the blockchain for being invoked later.
        /// </summary>
        PublishTransaction = 0xd0,
        InvocationTransaction = 0xd1
    }
    public enum TransactionAttributeUsage : byte
    {
        /// <summary>
        /// 外部合同的散列值
        /// </summary>
        ContractHash = 0x00,

        /// <summary>
        /// 用于ECDH密钥交换的公钥，该公钥的第一个字节为0x02
        /// </summary>
        ECDH02 = 0x02,
        /// <summary>
        /// 用于ECDH密钥交换的公钥，该公钥的第一个字节为0x03
        /// </summary>
        ECDH03 = 0x03,

        /// <summary>
        /// 用于对交易进行额外的验证
        /// </summary>
        Script = 0x20,

        Vote = 0x30,

        DescriptionUrl = 0x81,
        Description = 0x90,

        Hash1 = 0xa1,
        Hash2 = 0xa2,
        Hash3 = 0xa3,
        Hash4 = 0xa4,
        Hash5 = 0xa5,
        Hash6 = 0xa6,
        Hash7 = 0xa7,
        Hash8 = 0xa8,
        Hash9 = 0xa9,
        Hash10 = 0xaa,
        Hash11 = 0xab,
        Hash12 = 0xac,
        Hash13 = 0xad,
        Hash14 = 0xae,
        Hash15 = 0xaf,

        /// <summary>
        /// 备注
        /// </summary>
        Remark = 0xf0,
        Remark1 = 0xf1,
        Remark2 = 0xf2,
        Remark3 = 0xf3,
        Remark4 = 0xf4,
        Remark5 = 0xf5,
        Remark6 = 0xf6,
        Remark7 = 0xf7,
        Remark8 = 0xf8,
        Remark9 = 0xf9,
        Remark10 = 0xfa,
        Remark11 = 0xfb,
        Remark12 = 0xfc,
        Remark13 = 0xfd,
        Remark14 = 0xfe,
        Remark15 = 0xff
    }
    public class Attribute
    {
        public TransactionAttributeUsage usage;
        public byte[] data;
    }
    public struct Fixed8
    {
        const ulong D = 100000000;
        ulong value;
        public Fixed8(ulong lv)
        {
            this.value = lv;
        }
        public static implicit operator decimal(Fixed8 value)
        {
            return value.value / (decimal)D;
        }
        public static implicit operator Fixed8(decimal value)
        {
            return new Fixed8((ulong)(value * (decimal)D));
        }
        public byte[] toBytes()
        {
            return BitConverter.GetBytes(value);
        }
        public static Fixed8 FromBytes(byte[] data, int pos)
        {
            ulong value = BitConverter.ToUInt64(data, pos);
            return new Fixed8(value);
        }
        public override string ToString()
        {
            decimal num = this;
            return num.ToString();
        }
    }

    public struct TransactionOutput
    {
        public byte[] assetId;
        public Fixed8 value;
        public byte[] toAddress;
    }
    public class TransactionInput
    {
        public byte[] hash;
        public UInt16 index;
    }
    public class Witness
    {
        public byte[] InvocationScript;//设置参数脚本，通常是吧signdata push进去
        public byte[] VerificationScript;//校验脚本，通常是 push 公钥, CheckSig 两条指令   验证的东西就是未签名的交易
        //这个就是地址的脚本
        public string Address
        {
            get
            {
                var hash = ThinNeo.Helper.GetScriptHashFromScript(VerificationScript);
                return ThinNeo.Helper.GetAddressFromScriptHash(hash);
            }
        }
        public Hash160 Hash
        {
            get
            {
                return ThinNeo.Helper.GetScriptHashFromScript(VerificationScript);
            }
        }


        public bool isSmartContract
        {
            get
            {
                if (VerificationScript.Length != 35)
                    return true;
                if (VerificationScript[0] != VerificationScript.Length - 2)
                    return true;
                if (VerificationScript[VerificationScript.Length - 1] != 0xac)
                    return true;

                return false;
            }
        }

        public override string ToString()
        {
            if (isSmartContract)
            {
                return "[SC]" + Hash.ToString();
            }
            else
            {
                return Address;
            }
        }
    }

    public interface IExtData
    {
        void Serialize(Transaction trans, System.IO.Stream writer);
        void Deserialize(Transaction trans, System.IO.Stream reader);
    }

    public class InvokeTransData : IExtData
    {
        public byte[] script;
        public Fixed8 gas;
        public void Deserialize(Transaction trans, Stream reader)
        {
            var slen = Transaction.readVarInt(reader);
            script = new byte[slen];
            reader.Read(script, 0, (int)slen);
            if (trans.version >= 1)
            {
                var bs = new byte[8];
                reader.Read(bs, 0, 8);
                gas = new Fixed8(BitConverter.ToUInt64(bs, 0));
            }
        }

        public void Serialize(Transaction trans, Stream writer)
        {
            Transaction.writeVarInt(writer, (ulong)script.Length);
            writer.Write(script, 0, script.Length);
            if (trans.version >= 1)
            {
                var bs = gas.toBytes();
                writer.Write(bs, 0, 8);
            }
        }
    }
    public class ClaimTransData : IExtData
    {
        public TransactionInput[] claims;
        public void Serialize(Transaction trans, Stream writer)
        {
            Transaction.writeVarInt(writer, (ulong)this.claims.Length);
            for (var i = 0; i < this.claims.Length; i++)
            {
                writer.Write(this.claims[i].hash, 0, 32);
                var buf = BitConverter.GetBytes(claims[i].index);
                writer.Write(buf, 0, 2);
            }
        }
        public void Deserialize(Transaction trans, Stream reader)
        {
            var countClaims = (int)Transaction.readVarInt(reader);
            this.claims = new TransactionInput[countClaims];
            for (var i = 0; i < countClaims; i++)
            {
                this.claims[i] = new TransactionInput();
                this.claims[i].hash = new byte[32];
                reader.Read(this.claims[i].hash, 0, 32);
                var buf = new byte[2];
                reader.Read(buf, 0, 2);
                UInt16 index = (UInt16)(buf[1] * 256 + buf[0]);
                this.claims[i].index = index;
            }
        }
    }
    public class MinerTransData : IExtData
    {
        public UInt32 nonce;
        public void Serialize(Transaction trans, Stream writer)
        {
            var buf = BitConverter.GetBytes(this.nonce);
            writer.Write(buf, 0, 4);

        }
        public void Deserialize(Transaction trans, Stream reader)
        {
            var buf = new byte[4];
            reader.Read(buf, 0, 4);

            this.nonce = BitConverter.ToUInt32(buf, 0);
        }
    }
    public class Transaction
    {
        public TransactionType type;
        public byte version;
        public Attribute[] attributes;
        public TransactionInput[] inputs;
        public TransactionOutput[] outputs;
        public Witness[] witnesses;//见证人
        public void SerializeUnsigned(System.IO.Stream writer)
        {
            //write type
            writer.WriteByte((byte)type);
            //write version
            writer.WriteByte(version);
            //SerializeExclusiveData(writer);
            if (type == TransactionType.ContractTransaction
                || type == TransactionType.IssueTransaction)//每个交易类型有一些自己独特的处理
            {
                //ContractTransaction 就是最常见的转账交易
                //他没有自己的独特处理
            }
            else if (type == TransactionType.InvocationTransaction)
            {
                extdata.Serialize(this, writer);
            }
            else if (type == TransactionType.InvocationTransaction)
            {
                extdata.Serialize(this, writer);
            }
            else if (type == TransactionType.ClaimTransaction)
            {
                extdata.Serialize(this, writer);
            }
            else if (type == TransactionType.MinerTransaction)
            {
                extdata.Serialize(this, writer);
            }
            else
            {
                throw new Exception("未编写针对这个交易类型的代码");
            }
            #region write attribute
            var countAttributes = (uint)attributes.Length;
            writeVarInt(writer, countAttributes);
            for (var i = 0; i < countAttributes; i++)
            {
                byte[] attributeData = attributes[i].data;
                var Usage = attributes[i].usage;
                writer.WriteByte((byte)Usage);
                if (Usage == TransactionAttributeUsage.ContractHash || Usage == TransactionAttributeUsage.Vote || (Usage >= TransactionAttributeUsage.Hash1 && Usage <= TransactionAttributeUsage.Hash15))
                {
                    //attributeData =new byte[32];
                    writer.Write(attributeData, 0, 32);
                }
                else if (Usage == TransactionAttributeUsage.ECDH02 || Usage == TransactionAttributeUsage.ECDH03)
                {
                    //attributeData = new byte[33];
                    //attributeData[0] = (byte)Usage;
                    writer.Write(attributeData, 1, 32);
                }
                else if (Usage == TransactionAttributeUsage.Script)
                {
                    //attributeData = new byte[20];

                    writer.Write(attributeData, 0, 20);
                }
                else if (Usage == TransactionAttributeUsage.DescriptionUrl)
                {
                    //var len = (byte)ms.ReadByte();
                    //attributeData = new byte[len];
                    var len = attributeData.Length;
                    writer.WriteByte((byte)len);
                    writer.Write(attributeData, 0, len);
                }
                else if (Usage == TransactionAttributeUsage.Description || Usage >= TransactionAttributeUsage.Remark)
                {
                    //var len = (int)readVarInt(ms, 65535);
                    //attributeData = new byte[len];
                    var len = (int)attributeData.Length;
                    writeVarInt(writer, (uint)len);
                    writer.Write(attributeData, 0, len);
                }
                else
                    throw new FormatException();
            }
            #endregion
            #region write Input
            var countInputs = (uint)inputs.Length;
            writeVarInt(writer, countInputs);
            for (var i = 0; i < countInputs; i++)
            {
                writer.Write(inputs[i].hash, 0, 32);
                var buf = BitConverter.GetBytes(inputs[i].index);
                writer.Write(buf, 0, 2);
            }
            #endregion
            #region write Outputs
            var countOutputs = (uint)outputs.Length;
            writeVarInt(writer, countOutputs);
            for (var i = 0; i < countOutputs; i++)
            {
                var item = outputs[i];
                //资产种类
                writer.Write(item.assetId, 0, 32);

                //Int64 value = 0; //钱的数字是一个定点数，乘以D 
                //Int64 D = 100000000;
                byte[] buf = item.value.toBytes();
                writer.Write(buf, 0, 8);
                //value = BitConverter.ToInt64(buf, 0);


                //decimal number = ((decimal)value / (decimal)D);
                //if (number <= 0)
                //    throw new FormatException();
                //资产数量

                //目标地址
                writer.Write(item.toAddress, 0, 20);

            }
            #endregion
        }
        public void Serialize(System.IO.Stream writer)
        {
            this.SerializeUnsigned(writer);

            #region write witnesses
            var witnesscount = (ulong)witnesses.Length;
            writeVarInt(writer, witnesscount);
            for (var i = 0; i < (int)witnesscount; i++)
            {
                var _witness = this.witnesses[i];
                writeVarInt(writer, (ulong)_witness.InvocationScript.Length);
                writer.Write(_witness.InvocationScript, 0, _witness.InvocationScript.Length);
                writeVarInt(writer, (ulong)_witness.VerificationScript.Length);
                writer.Write(_witness.VerificationScript, 0, _witness.VerificationScript.Length);
            }
            #endregion
        }
        public IExtData extdata;

        public void Deserialize(System.IO.Stream ms)
        {
            //参考源码来自
            //      https://github.com/neo-project/neo
            //      Transaction.cs
            //      源码采用c#序列化技术

            //参考源码2
            //      https://github.com/AntSharesSDK/antshares-ts
            //      Transaction.ts
            //      采用typescript开发

            this.type = (TransactionType)ms.ReadByte();//读一个字节，交易类型
            this.version = (byte)ms.ReadByte();
            if (this.type == TransactionType.ContractTransaction
                || this.type == TransactionType.IssueTransaction)//每个交易类型有一些自己独特的处理
            {
                //ContractTransaction 就是最常见的合约交易，
                //他没有自己的独特处理
                extdata = null;
            }
            else if (this.type == TransactionType.InvocationTransaction)
            {
                extdata = new InvokeTransData();
            }
            else if (this.type == TransactionType.ClaimTransaction)
            {
                extdata = new ClaimTransData();
            }
            else if (this.type == TransactionType.MinerTransaction)
            {
                extdata = new MinerTransData();
            }

            else
            {
                throw new Exception("未编写针对这个交易类型的代码");
            }
            if (extdata != null)
            {
                extdata.Deserialize(this, ms);
            }
            //attributes
            var countAttributes = readVarInt(ms);
            this.attributes = new Attribute[countAttributes];
            Console.WriteLine("countAttributes:" + countAttributes);
            for (UInt64 i = 0; i < countAttributes; i++)
            {
                this.attributes[i] = new Attribute();

                //读取attributes
                byte[] attributeData;
                var Usage = (TransactionAttributeUsage)ms.ReadByte();
                if (Usage == TransactionAttributeUsage.ContractHash || Usage == TransactionAttributeUsage.Vote || (Usage >= TransactionAttributeUsage.Hash1 && Usage <= TransactionAttributeUsage.Hash15))
                {
                    attributeData = new byte[32];
                    ms.Read(attributeData, 0, 32);
                }
                else if (Usage == TransactionAttributeUsage.ECDH02 || Usage == TransactionAttributeUsage.ECDH03)
                {
                    attributeData = new byte[33];
                    attributeData[0] = (byte)Usage;
                    ms.Read(attributeData, 1, 32);
                }
                else if (Usage == TransactionAttributeUsage.Script)
                {
                    attributeData = new byte[20];
                    ms.Read(attributeData, 0, 20);
                }
                else if (Usage == TransactionAttributeUsage.DescriptionUrl)
                {
                    var len = (byte)ms.ReadByte();
                    attributeData = new byte[len];
                    ms.Read(attributeData, 0, len);
                }
                else if (Usage == TransactionAttributeUsage.Description || Usage >= TransactionAttributeUsage.Remark)
                {
                    var len = (int)readVarInt(ms, 65535);
                    attributeData = new byte[len];
                    ms.Read(attributeData, 0, len);
                }
                else
                    throw new FormatException();

                this.attributes[i].usage = Usage;
                this.attributes[i].data = attributeData;
            }

            //inputs  输入表示基于哪些交易
            var countInputs = readVarInt(ms);
            Console.WriteLine("countInputs:" + countInputs);
            this.inputs = new TransactionInput[countInputs];
            for (UInt64 i = 0; i < countInputs; i++)
            {
                this.inputs[i] = new TransactionInput();
                this.inputs[i].hash = new byte[32];
                byte[] buf = new byte[2];
                ms.Read(this.inputs[i].hash, 0, 32);
                ms.Read(buf, 0, 2);
                UInt16 index = (UInt16)(buf[1] * 256 + buf[0]);
                this.inputs[i].index = index;
            }

            //outputes 输出表示最后有哪几个地址得到多少钱，肯定有一个是自己的地址,因为每笔交易都会把之前交易的余额清空,刨除自己,就是要转给谁多少钱

            //这个机制叫做UTXO
            var countOutputs = readVarInt(ms);
            Console.WriteLine("countOutputs:" + countOutputs);
            this.outputs = new TransactionOutput[countOutputs];
            for (UInt64 i = 0; i < countOutputs; i++)
            {
                this.outputs[i] = new TransactionOutput();
                TransactionOutput outp = this.outputs[i];
                //资产种类
                var assetid = new byte[32];
                ms.Read(assetid, 0, 32);

                byte[] buf = new byte[8];
                ms.Read(buf, 0, 8);
                var value = Fixed8.FromBytes(buf, 0);
                //资产数量

                var scripthash = new byte[20];

                ms.Read(scripthash, 0, 20);
                outp.assetId = assetid;
                outp.value = value;
                outp.toAddress = scripthash;

                this.outputs[i] = outp;

            }

            //读取鉴证
            if (ms.Position < ms.Length)
            {
                var witnesscount = readVarInt(ms);
                this.witnesses = new Witness[witnesscount];
                for (var i = 0; i < (int)witnesscount; i++)
                {
                    this.witnesses[i] = new Witness();
                    var _witness = this.witnesses[i];

                    var iscriptlen = readVarInt(ms);
                    _witness.InvocationScript = new byte[iscriptlen];
                    ms.Read(_witness.InvocationScript, 0, _witness.InvocationScript.Length);

                    var vscriptlen = readVarInt(ms);
                    _witness.VerificationScript = new byte[vscriptlen];
                    ms.Read(_witness.VerificationScript, 0, _witness.VerificationScript.Length);
                }
            }
        }
        public static void writeVarInt(System.IO.Stream stream, UInt64 value)
        {
            if (value > 0xffffffff)
            {
                stream.WriteByte((byte)(0xff));
                var bs = BitConverter.GetBytes(value);
                stream.Write(bs, 0, 8);
            }
            else if (value > 0xffff)
            {
                stream.WriteByte((byte)(0xfe));
                var bs = BitConverter.GetBytes((UInt32)value);
                stream.Write(bs, 0, 4);
            }
            else if (value > 0xfc)
            {
                stream.WriteByte((byte)(0xfd));
                var bs = BitConverter.GetBytes((UInt16)value);
                stream.Write(bs, 0, 2);
            }
            else
            {
                stream.WriteByte((byte)value);
            }
        }
        public static UInt64 readVarInt(System.IO.Stream stream, UInt64 max = 9007199254740991)
        {
            var fb = (byte)stream.ReadByte();
            UInt64 value = 0;
            byte[] buf = new byte[8];
            if (fb == 0xfd)
            {
                stream.Read(buf, 0, 2);
                value = (UInt64)(buf[1] * 256 + buf[0]);
            }
            else if (fb == 0xfe)
            {
                stream.Read(buf, 0, 4);
                value = (UInt64)(buf[1] * 256 * 256 * 256 + buf[1] * 256 * 256 + buf[1] * 256 + buf[0]);
            }
            else if (fb == 0xff)
            {
                stream.Read(buf, 0, 8);
                //我懒得展开了，规则同上
                value = BitConverter.ToUInt64(buf, 0);// (UInt64)(buf[1] * 256 * 256 * 256 + buf[1] * 256 * 256 + buf[1] * 256 + buf[0]);
            }
            else
                value = fb;
            if (value > max) throw new Exception("to large.");
            return value;
        }

        public byte[] GetMessage()
        {
            byte[] msg = null;
            using (var ms = new System.IO.MemoryStream())
            {
                SerializeUnsigned(ms);
                msg = ms.ToArray();
            }
            return msg;
        }
        public byte[] GetRawData()
        {
            byte[] msg = null;
            using (var ms = new System.IO.MemoryStream())
            {
                Serialize(ms);
                msg = ms.ToArray();
            }
            return msg;
        }
        //增加个人账户见证人（就是用这个人的私钥对交易签个名，signdata传进来）
        public void AddWitness(byte[] signdata, byte[] pubkey, string addrs)
        {
            {//额外的验证
                byte[] msg = null;
                using (var ms = new System.IO.MemoryStream())
                {
                    SerializeUnsigned(ms);
                    msg = ms.ToArray();
                }
                bool bsign = ThinNeo.Helper.VerifySignature(msg, signdata, pubkey);
                if (bsign == false)
                    throw new Exception("wrong sign");

                var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                if (addr != addrs)
                    throw new Exception("wrong script");
            }

            var vscript = ThinNeo.Helper.GetScriptFromPublicKey(pubkey);

            //iscript 对个人账户见证人他是一条pushbytes 指令

            var sb = new ThinNeo.ScriptBuilder();
            sb.EmitPushBytes(signdata);

            var iscript = sb.ToArray();

            AddWitnessScript(vscript, iscript);
        }

        //增加智能合约见证人
        public void AddWitnessScript(byte[] vscript, byte[] iscript)
        {
            var scripthash = ThinNeo.Helper.GetScriptHashFromScript(vscript);
            List<Witness> wit = null;
            if (witnesses == null)
            {
                wit = new List<Witness>();
            }
            else
            {
                wit = new List<Witness>(witnesses);
            }
            Witness newwit = new Witness();
            newwit.VerificationScript = vscript;
            newwit.InvocationScript = iscript;
            foreach (var w in wit)
            {
                if (w.Address == newwit.Address)
                    throw new Exception("alread have this witness");
            }

            wit.Add(newwit);
            wit.Sort((a, b) =>
            {
                return a.Hash.CompareTo(b.Hash);
            });
            witnesses = wit.ToArray();

        }

        //TXID
        public Hash256 GetHash()
        {
            var msg = GetMessage();
            var data = ThinNeo.Helper.Sha256(msg);
            data = ThinNeo.Helper.Sha256(data);
            return data;

        }
    }
}
