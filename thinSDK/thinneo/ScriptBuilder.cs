using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinNeo.VM;

namespace ThinNeo
{

    public class ScriptBuilder : IDisposable
    {
        private readonly MemoryStream ms = new MemoryStream();
        private readonly BinaryWriter writer;

        public int Offset => (int)ms.Position;

        public ScriptBuilder()
        {
            this.writer = new BinaryWriter(ms);
        }

        public void Dispose()
        {
            writer.Dispose();
            ms.Dispose();
        }

        public ScriptBuilder Emit(OpCode op, byte[] arg = null)
        {
            writer.Write((byte)op);
            if (arg != null)
                writer.Write(arg);
            return this;
        }

        public ScriptBuilder EmitAppCall(byte[] scriptHash, bool useTailCall = false)
        {
            if (scriptHash.Length != 20)
                throw new ArgumentException();
            return Emit(useTailCall ? OpCode.TAILCALL : OpCode.APPCALL, scriptHash);
        }

        public ScriptBuilder EmitJump(OpCode op, short offset)
        {
            if (op != OpCode.JMP && op != OpCode.JMPIF && op != OpCode.JMPIFNOT && op != OpCode.CALL)
                throw new ArgumentException();
            return Emit(op, BitConverter.GetBytes(offset));
        }

        public ScriptBuilder EmitPushNumber(BigInteger number)
        {
            if (number == -1) return Emit(OpCode.PUSHM1);
            if (number == 0) return Emit(OpCode.PUSH0);
            if (number > 0 && number <= 16) return Emit(OpCode.PUSH1 - 1 + (byte)number);
            return EmitPushBytes(number.ToByteArray());
        }

        public ScriptBuilder EmitPushBool(bool data)
        {
            return Emit(data ? OpCode.PUSHT : OpCode.PUSHF);
        }

        public ScriptBuilder EmitPushBytes(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException();
            if (data.Length <= (int)OpCode.PUSHBYTES75)
            {
                writer.Write((byte)data.Length);
                writer.Write(data);
            }
            else if (data.Length < 0x100)
            {
                Emit(OpCode.PUSHDATA1);
                writer.Write((byte)data.Length);
                writer.Write(data);
            }
            else if (data.Length < 0x10000)
            {
                Emit(OpCode.PUSHDATA2);
                writer.Write((ushort)data.Length);
                writer.Write(data);
            }
            else// if (data.Length < 0x100000000L)
            {
                Emit(OpCode.PUSHDATA4);
                writer.Write(data.Length);
                writer.Write(data);
            }
            return this;
        }

        public ScriptBuilder EmitPushString(string data)
        {
            return EmitPushBytes(Encoding.UTF8.GetBytes(data));
        }

        public ScriptBuilder EmitSysCall(string api)
        {
            if (api == null)
                throw new ArgumentNullException();
            byte[] api_bytes = Encoding.ASCII.GetBytes(api);
            if (api_bytes.Length == 0 || api_bytes.Length > 252)
                throw new ArgumentException();
            byte[] arg = new byte[api_bytes.Length + 1];
            arg[0] = (byte)api_bytes.Length;
            Buffer.BlockCopy(api_bytes, 0, arg, 1, api_bytes.Length);
            return Emit(OpCode.SYSCALL, arg);
        }

        public ScriptBuilder EmitParamJson(MyJson.IJsonNode param)
        {
            if (param is MyJson.JsonNode_ValueNumber)//bool 或小整数
            {
                var num = param as MyJson.JsonNode_ValueNumber;
                if (num.isBool)
                {
                    this.EmitPushBool(num.AsBool());
                }
                else
                {
                    this.EmitPushNumber(num.AsInt());
                }
            }
            else if (param is MyJson.JsonNode_Array)
            {
                var list = param.AsList();
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    EmitParamJson(list[i]);
                }
                this.EmitPushNumber(param.AsList().Count);
                this.Emit(ThinNeo.VM.OpCode.PACK);
            }
            else if (param is MyJson.JsonNode_ValueString)//复杂格式
            {
                var str = param.AsString();
                if (str[0] != '(')
                    throw new Exception("must start with:(str) or (hex) or (hexrev) or (int) or (addr)");
                if (str.IndexOf("(str)") == 0)
                {
                    this.EmitPushString(str.Substring(5));
                }
                else if (str.IndexOf("(int)") == 0)
                {
                    var num = System.Numerics.BigInteger.Parse(str.Substring(5));
                    this.EmitPushNumber(num);
                }
                else if (str.IndexOf("(hex)") == 0)
                {
                    var hex = ThinNeo.Helper.HexString2Bytes(str.Substring(5));
                    this.EmitPushBytes(hex);
                }
                else if (str.IndexOf("(hexrev)") == 0)
                {
                    var hex = ThinNeo.Helper.HexString2Bytes(str.Substring(8));
                    this.EmitPushBytes(hex.Reverse().ToArray());
                }
                else if (str.IndexOf("(addr)") == 0)
                {
                    var addr = (str.Substring(6));
                    var hex = ThinNeo.Helper.GetPublicKeyHashFromAddress(addr);
                    this.EmitPushBytes(hex);
                }
                else
                    throw new Exception("must start with:(str) or (hex) or (hexbig) or (int)");
            }
            else
            {
                throw new Exception("should not pass a {}");
            }
            return this;
        }

        public byte[] ToArray()
        {
            writer.Flush();
            return ms.ToArray();
        }
    }


}
