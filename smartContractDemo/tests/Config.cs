using System;
using System.Collections.Generic;
using System.Text;
using ThinNeo;
namespace smartContractDemo.tests
{
    class Config
    {
        public readonly static string superadminAddress = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
        public  static string test_wif = "KwF2QTMzDEmjEEfkbvkGTZTL8Ch17h1jvQjbLtFj4vHs7KfLrKir";


        public readonly static Hash160 dapp_sgas  = new Hash160("0xed022d586745e3b0d3654ac86aa81ca88d68c0a2");//sgas 新合约地址
        public readonly static Hash160 dapp_coinpool = new Hash160("0x5d6b91ee7cde1f8bb1868d36d4bf134f6887d231");//coinpool 新合约地址
        public static readonly Hash160 sc_nns = new Hash160("0x537758fbe85505801faa7d7d7b75b37686ad7e2d");//nns 跳板合约地址
        public static readonly Hash160 domaincenterhash = new Hash160("0x04f4ff5237391169b42b5c84a5313b43080a6990");//nns 域名中心合约地址


        public static readonly Hash160 dapp_multisign = new Hash160("0x4c0f57b61d997297560190b1e397fe6d58fce94a");  //应用合约多签验证测试

        public readonly static Hash160 dapp_nnc = new Hash160("0x7054c7e606f7abd353838d2b03190d302f0db938");
                                                            //("0xd9f8803a66acdd0c4218b4dca21a43c43e28b66c"); //nnc 合约代码

        public const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";


        public readonly static string api_local = "http://localhost:20332";
        public readonly static string api = "https://api.nel.group/api/testnet";
        

        public readonly static string root = "wei";

        public static void changeWif(string wif)
        {
            if (wif.Length == 52)
                Config.test_wif = wif;
        }
        
        public static void LogLn(string content,ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Log(string content, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(content);
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
