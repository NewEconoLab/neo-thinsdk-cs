using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinNeo
{
    public class Hash256
    {
        public Hash256(byte[] data)
        {
            if (data.Length != 32)
                throw new Exception("error length.");
            this.data = data;
        }
        public Hash256(string hexstr)
        {
            var bts = ThinNeo.Helper.HexString2Bytes(hexstr);
            if (bts.Length != 32)
                throw new Exception("error length.");
            this.data = bts.Reverse().ToArray();
        }
        public override string ToString()
        {
            return "0x" + ThinNeo.Helper.Bytes2HexString(this.data.Reverse().ToArray());
        }
        public byte[] data;

        public static implicit operator byte[] (Hash256 value)
        {
            return value.data;
        }
        public static implicit operator Hash256(byte[] value)
        {
            return new Hash256(value);
        }
    }
}
