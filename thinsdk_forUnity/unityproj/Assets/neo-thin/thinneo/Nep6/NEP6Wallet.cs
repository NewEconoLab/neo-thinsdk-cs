
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ThinNeo.Cryptography;

namespace ThinNeo.NEP6
{
    public class NEP6Wallet
    {

        private readonly string path;
        public readonly ScryptParameters scrypt;
        public readonly Dictionary<string, NEP6Account> accounts;

        public NEP6Wallet(string path)
        {
            this.accounts = new Dictionary<string, NEP6Account>();

            this.path = path;
            if (File.Exists(path))
            {
                string txt = System.IO.File.ReadAllText(path);
                MyJson.JsonNode_Object wallet = MyJson.Parse(txt) as MyJson.JsonNode_Object;

                this.scrypt = ScryptParameters.FromJson(wallet["scrypt"] as MyJson.JsonNode_Object);
                var accounts = wallet["accounts"].AsList();
                foreach (MyJson.JsonNode_Object a in accounts)
                {
                    var ac = NEP6Account.FromJson(a, this);
                    this.accounts[Helper.Bytes2HexString(ac.ScriptHash)] = ac;
                }
            }
            else
            {
                this.scrypt = ScryptParameters.Default;
            }
        }

        private void AddAccount(NEP6Account account)
        {
            accounts[Helper.Bytes2HexString(account.ScriptHash)] = account;
        }


        public NEP6Account CreateAccount(byte[] privateKey, string password)
        {
            var pubkey = Helper.GetPublicKeyFromPrivateKey(privateKey);
            NEP6Contract contract = new NEP6Contract
            {
                Script = Helper.GetScriptFromPublicKey(pubkey)
            };
            var scripthash = Helper.GetScriptHashFromPublicKey(pubkey);

            var nep2key = Helper.GetNep2FromPrivateKey(privateKey, password);

            NEP6Account account = new NEP6Account(scripthash, nep2key)
            {
                Contract = contract
            };
            AddAccount(account);

            return account;
        }

        public void Save()
        {
            MyJson.JsonNode_Object wallet = new MyJson.JsonNode_Object();
            var n = new MyJson.JsonNode_ValueNumber();
            n.SetNull();
            wallet["name"] = n;
            wallet["version"] = new MyJson.JsonNode_ValueString("1.0");
            wallet["scrypt"] = scrypt.ToJson();
            wallet["accounts"] = new MyJson.JsonNode_Array();
            foreach (var ac in accounts.Values)
            {
                var jnot = ac.ToJson();
                wallet["accounts"].AsList().Add(jnot);
            }
            wallet["extra"] = n;
            File.WriteAllText(path, wallet.ToString());
        }
       
    }
}
