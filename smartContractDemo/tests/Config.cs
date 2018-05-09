using System;
using System.Collections.Generic;
using System.Text;
using ThinNeo;
namespace smartContractDemo.tests
{
    class Config
    {
        public readonly static string superadminAddress = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
        public  static string test_wif = "L3tDHnEAvwnnPE4sY4oXpTvNtNhsVhbkY4gmEmWmWWf1ebJhVPVW";


        public readonly static Hash160 dapp_sgas  = new Hash160("0xbc0fdb1c1b84601a9c66594cb481b684b90e05bb");//sgas 合约地址
        public readonly static Hash160 dapp_coinpool = new Hash160("0x7ef0366e03dfda239981a41e20d25258b07fb19a");//coinpool 合约地址
        public static readonly Hash160 sc_nns = new Hash160("0x954f285a93eed7b4aed9396a7806a5812f1a5950");//nns 合约地址
        public readonly static string utxo_nnc = "0xc12c6ccc5be5235b90822c4feee70645b9d0bac0636b07bd1d68e34ba8804747";
        public const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";


        public readonly static string api_local = "http://localhost:20332";
        public readonly static string api = "https://api.nel.group/api/testnet";
        

        public readonly static string root = "sell";

        public static void changeWif(string wif)
        {
            if (wif.Length == 52)
                Config.test_wif = wif;
        }
        


    }
}
