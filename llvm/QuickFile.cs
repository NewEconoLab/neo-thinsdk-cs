using System;
using System.Collections.Generic;
using System.Text;

namespace llvm
{
    public class QuickFile
    {
        public static byte[] FromFile(System.IO.Stream stream)
        {
            var decoder = new SevenZip.Compression.LZMA.Decoder();
            var ms = new System.IO.MemoryStream();
            byte[] ps = new byte[5];
            stream.Read(ps, 0, 5);
            decoder.SetDecoderProperties(ps);

            byte[] buf = new byte[4];
            stream.Read(buf, 0, 4);
            var unpacklen = BitConverter.ToUInt32(buf, 0);
            stream.Read(buf, 0, 4);
            var packlen = BitConverter.ToUInt32(buf, 0);

            decoder.Code(stream, ms, packlen, unpacklen, null);
            var array = ms.ToArray();
            ms.Close();

            return array;
        }
        public static bool ToFile(string srcfile, string destfile)
        {
            var src = System.IO.File.OpenRead(srcfile);
            var dest = System.IO.File.OpenWrite(destfile);

            var encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.SetCoderProperties(new SevenZip.CoderPropID[] { SevenZip.CoderPropID.NumFastBytes }, new object[] { (int)5 });
            encoder.WriteCoderProperties(dest);
            byte[] buf = BitConverter.GetBytes(src.Length);
            dest.Write(buf, 0, 4);
            var poslen = dest.Position;
            dest.Write(buf, 0, 4);
            var start = dest.Position;
            encoder.Code(src, dest, src.Length, -1, null);
            var len = dest.Position - start;
            buf = BitConverter.GetBytes(len);
            dest.Position = poslen;
            dest.Write(buf, 0, 4);
            dest.Close();
            src.Close();
            return true;
        }
        public static byte[] ToBytes(string srcfile)
        {
            var src = System.IO.File.OpenRead(srcfile);
            var dest = new System.IO.MemoryStream();

            var encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.SetCoderProperties(new SevenZip.CoderPropID[] { SevenZip.CoderPropID.NumFastBytes }, new object[] { (int)5 });
            encoder.WriteCoderProperties(dest);
            byte[] buf = BitConverter.GetBytes(src.Length);
            dest.Write(buf, 0, 4);
            var poslen = dest.Position;
            dest.Write(buf, 0, 4);
            var start = dest.Position;
            encoder.Code(src, dest, src.Length, -1, null);
            var len = dest.Position - start;
            buf = BitConverter.GetBytes(len);
            dest.Position = poslen;
            dest.Write(buf, 0, 4);
            var array = dest.ToArray();
            dest.Close();
            src.Close();
            return array;
        }
    }
}
