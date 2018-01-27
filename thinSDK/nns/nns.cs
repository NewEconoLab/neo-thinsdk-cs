using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinNeo
{
    public class NNSUrl
    {
        public override string ToString()
        {
            var body = "";
            for (var i = this.namearray.Length - 1; i >= 0; i--)
            {
                body += this.namearray[i];
                if (i != 0)
                    body += ".";
            }
            if (string.IsNullOrEmpty(this.protocol) == false)
            {
                return this.protocol + "://" + body;
            }
            return body;
        }
        public NNSUrl(string str)
        {
            str = str.Replace(" ", "");

            var i = str.IndexOf("://");
            int endi = -1;
            if (i < 0)
            {
                this.protocol = "";
                endi = str.IndexOf('/');
                i = 0;
            }
            else
            {
                this.protocol = str.Substring(0, i);
                endi = str.IndexOf('/', i + 3);
                i = i + 3;
            }
            string body = "";
            if (endi < 0)
            {
                body = str.Substring(i);
            }
            else
            {
                body = str.Substring(i + 3, (endi - (i + 3)));
            }
            var list = body.Split('.');
            this.namearray = new string[list.Length];
            for (var j = list.Length - 1; j >= 0; j--)
            {
                this.namearray[list.Length - 1 - j] = list[j];
            }
        }
        public string protocol;
        public string[] namearray;
        public string lastname
        {
            get
            {
                if (namearray.Length == 1)
                    return "";
                return namearray[namearray.Length - 1];
            }
        }
        public byte[] parenthash
        {
            get
            {
                byte[] hash = GetNameHash(namearray[0]);
                for (var i = 1; i < namearray.Length - 1; i++)
                {
                    hash = GetNameHashSub(hash, namearray[i]);
                }
                return hash;
            }
        }
        public byte[] namehash
        {
            get
            {
                return GetNameHashArray(namearray);

            }
        }
        #region 域名转hash算法
        //域名转hash算法
        //aaa.bb.test =>{"test","bb","aa"}
        static byte[] GetNameHash(string domain)
        {
            return ThinNeo.Helper.Sha256(System.Text.Encoding.UTF8.GetBytes(domain));
        }
        static byte[] GetNameHashSub(byte[] roothash, string subdomain)
        {
            var bs = System.Text.Encoding.UTF8.GetBytes(subdomain);
            if (bs.Length == 0)
                return roothash;

            var domain = ThinNeo.Helper.Sha256(bs).Concat(roothash).ToArray();
            return ThinNeo.Helper.Sha256(domain);
        }
        static byte[] GetNameHashWithSubHash(byte[] roothash, byte[] subhash)
        {
            var domain = subhash.Concat(roothash).ToArray();
            return ThinNeo.Helper.Sha256(domain);
        }
        static byte[] GetNameHashArray(string[] domainarray)
        {
            byte[] hash = GetNameHash(domainarray[0]);
            for (var i = 1; i < domainarray.Length; i++)
            {
                hash = GetNameHashSub(hash, domainarray[i]);
            }
            return hash;
        }

        #endregion
    }
}
