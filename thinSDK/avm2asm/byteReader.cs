
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using ThinNeo.VM;

namespace ThinNeo.Compiler
{
    public class ByteReader
    {
        public ByteReader(byte[] data)
        {
            this.data = data;
        }
        public byte[] data;
        public int addr = 0;
        public OpCode ReadOP()
        {
            OpCode op = (OpCode)this.data[this.addr];
            addr++;
            return op;
        }
        public byte[] ReadBytes(int count)
        {
            byte[] _data = new byte[count];
            Array.Copy(data, addr, _data, 0, count);
            addr += count;
            return _data;
        }

        public byte ReadByte()
        {
            var b = data[addr];
            addr++;
            return b;
        }
        public UInt16 ReadUInt16()
        {
            var u16 = BitConverter.ToUInt16(data, addr);
            addr += 2;
            return u16;
        }
        public Int16 ReadInt16()
        {
            var u16 = BitConverter.ToInt16(data, addr);
            addr += 2;
            return u16;
        }
        public UInt32 ReadUInt32()
        {
            var u16 = BitConverter.ToUInt32(data, addr);
            addr += 4;
            return u16;
        }
        public UInt64 ReadUInt64()
        {
            var u16 = BitConverter.ToUInt64(data, addr);
            addr += 8;
            return u16;
        }
        public Int32 ReadInt32()
        {
            var u16 = BitConverter.ToInt32(data, addr);
            addr += 4;
            return u16;
        }
        public Int64 ReadInt64()
        {
            var u16 = BitConverter.ToInt64(data, addr);
            addr += 8;
            return u16;
        }
        public byte[] ReadVarBytes(int max = 0X7fffffc7)
        {
            var count = ReadVarInt((ulong)max);
            return ReadBytes((int)count);
        }
        public ulong ReadVarInt(ulong max = ulong.MaxValue)
        {
            byte fb = this.ReadByte();
            ulong value;
            if (fb == 0xFD)
                value = this.ReadUInt16();
            else if (fb == 0xFE)
                value = this.ReadUInt32();
            else if (fb == 0xFF)
                value = this.ReadUInt64();
            else
                value = fb;
            if (value > max) throw new FormatException();
            return value;
        }
        public bool End
        {
            get
            {
                return this.addr >= data.Length;
            }
        }
    }
}
