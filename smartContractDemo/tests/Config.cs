using System;
using System.Collections.Generic;
using System.Text;
using ThinNeo;
namespace smartContractDemo.tests
{
    class Config
    {
        public readonly static string superadminAddress = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
        public  static string test_wif = "";
        //0x8826206138420e79936123a18176c597cd173173
        //0xfc732edee1efdf968c23c20a9628eaa5a6ccb934
        public readonly static Hash160 dapp_nnc = new Hash160("0xfc732edee1efdf968c23c20a9628eaa5a6ccb934");//nnc 合约代码
        public readonly static Hash160 dapp_sgas  = new Hash160("0x9121e89e8a0849857262d67c8408601b5e8e0524");//sgas 新合约地址
        public static readonly Hash160 sc_nns = new Hash160("0x348387116c4a75e420663277d9c02049907128c7");//nns 跳板合约地址
        public static readonly Hash160 domaincenterhash = new Hash160("0xbd3fa97e2bc841292c1e77f9a97a1393d5208b48");//nns 域名中心合约地址

        public static readonly Hash160 domainTransactionhash = new Hash160("0xebc451326bba0cb87cabb20d47089add921c638b");//nns 域名交易合约


        public static readonly Hash160 dapp_multisign = new Hash160("0x4c0f57b61d997297560190b1e397fe6d58fce94a");  //应用合约多签验证测试
        public const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";


        public readonly static string api_local = "https://seed1.spotcoin.com:10332";// https://api.nel.group/api/mainnet";// "http://localhost:20332";
        public readonly static string api = "https://api.nel.group/api/testnet";// "https://seed1.spotcoin.com:10332";//"http://localhost:20332";//"http://seed4.aphelion-neo.com:10332";// "https://api.nel.group/api/testnet";



        public readonly static string root = "neo";

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
