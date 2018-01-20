using System;

namespace ThinNeo.NEP6
{
    public class NEP6Account
    {
        public byte[] ScriptHash;
        public string nep2key;
        public NEP6Contract Contract;

        public NEP6Account(byte[] scriptHash, string nep2key = null)
        {
            this.ScriptHash = scriptHash;
            this.nep2key = nep2key;
        }



        public static NEP6Account FromJson(MyJson.JsonNode_Object json, NEP6Wallet wallet)
        {
            var strAdd = json["address"].AsString();
            var pubkeyhash = Helper.GetPublicKeyHashFromAddress(strAdd);
            var key = json["key"]?.AsString();
            return new NEP6Account(pubkeyhash, key)
            {
                //Label = json["label"]?.AsString(),
                //IsDefault = json["isDefault"].AsBoolean(),
                //Lock = json["lock"].AsBoolean(),
                Contract = NEP6Contract.FromJson(json["contract"].AsDict()),
                //Extra = json["extra"]
            };
        }


        public byte[] GetPrivate(string password)
        {
            if (nep2key == null) return null;
            return Helper.GetPrivateKeyFromNEP2(nep2key, password);
        }

        public MyJson.JsonNode_Object ToJson()
        {
            MyJson.JsonNode_Object account = new MyJson.JsonNode_Object();
            byte[] shash = (ScriptHash);
            var addr = Helper.GetAddressFromScriptHash(shash);
            account["address"] = new MyJson.JsonNode_ValueString(addr);
            var _null = new MyJson.JsonNode_ValueNumber();
            _null.SetNull();
            account["label"] = _null;
            account["isDefault"] = new MyJson.JsonNode_ValueNumber(false);
            account["lock"] = new MyJson.JsonNode_ValueNumber(false);
            account["key"] = new MyJson.JsonNode_ValueString(nep2key);
            account["contract"] = ((NEP6Contract)Contract)?.ToJson();
            account["extra"] = _null;
            return account;
        }

        public bool VerifyPassword(string password)
        {
            try
            {
                var prikey = Helper.GetPrivateKeyFromNEP2(nep2key, password);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
