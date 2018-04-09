using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinNeo
{
    public class Hash160
    {
        public Hash160(byte[] data)
        {
            if (data.Length != 20)
                throw new Exception("error length.");
            this.data = data;

        }
        public Hash160(string hexstr)
        {
            var bts = ThinNeo.Helper.HexString2Bytes(hexstr);
            if (bts.Length != 20)
                throw new Exception("error length.");
            this.data = bts.Reverse().ToArray();
        }
        public override string ToString()
        {
            return "0x" + ThinNeo.Helper.Bytes2HexString(this.data.Reverse().ToArray());
        }
        public byte[] data;

        public static implicit operator byte[] (Hash160 value)
        {
            return value.data;
        }
        public static implicit operator Hash160(byte[] value)
        {
            return new Hash160(value);
        }
    }
}
