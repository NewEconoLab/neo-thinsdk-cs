using System;
using System.Collections.Generic;
using System.Text;
using ThinNeo;
namespace smartContractDemo.tests
{
    class Config
    {
        public readonly static string superadminAddress = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
        public  static string test_wif = "KwwJMvfFPcRx2HSgQRPviLv4wPrxRaLk7kfQntkH8kCXzTgAts8t";


        public readonly static Hash160 dapp_nnc = new Hash160("0xd8fa0cfdd54493dfc9e908b26ba165605363137b");//nnc 合约代码
        public readonly static Hash160 dapp_sgas  = new Hash160("0xc7816d11287c08135f4e5f907af9e39754910ba3");//sgas 新合约地址
        public static readonly Hash160 sc_nns = new Hash160("0x537758fbe85505801faa7d7d7b75b37686ad7e2d");//nns 跳板合约地址
        public static readonly Hash160 domaincenterhash = new Hash160("0xd30348a37fb57b7a61f6637ecfc0da6b4eb08dd0");//nns 域名中心合约地址


        public static readonly Hash160 dapp_multisign = new Hash160("0x4c0f57b61d997297560190b1e397fe6d58fce94a");  //应用合约多签验证测试
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
