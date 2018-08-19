using System;
using System.Collections.Generic;
using System.Text;
using ThinNeo;
namespace smartContractDemo.tests
{
    class Config
    {
        public readonly static string superadminAddress = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
        public  static string test_wif = "KwZih114osBp58RwpEn4ZAcEcCTLP6yMAhdikb6oPRxqgvWpcqF1";
        //0x8826206138420e79936123a18176c597cd173173
        //0xfc732edee1efdf968c23c20a9628eaa5a6ccb934
        public readonly static Hash160 dapp_nnc = new Hash160("0xfc732edee1efdf968c23c20a9628eaa5a6ccb934");//nnc 合约代码
        public readonly static Hash160 dapp_sgas  = new Hash160("0xf5630f4baba6a0333bfb10153e5f853125465b48");//sgas 新合约地址
        public static readonly Hash160 sc_nns = new Hash160("0x77e193f1af44a61ed3613e6e3442a0fc809bb4b8");//nns 跳板合约地址
        public static readonly Hash160 domaincenterhash = new Hash160("0x7754b7dbacd840f7b9f1b02277d8745901df8a22");//nns 域名中心合约地址

        public static readonly Hash160 domainTransactionhash = new Hash160("0xebc451326bba0cb87cabb20d47089add921c638b");//nns 域名交易合约


        public static readonly Hash160 dapp_multisign = new Hash160("0x4c0f57b61d997297560190b1e397fe6d58fce94a");  //应用合约多签验证测试
        public const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";


        public readonly static string api_local = "https://seed1.spotcoin.com:10332";// https://api.nel.group/api/mainnet";// "http://localhost:20332";
        public readonly static string api = "https://api.nel.group/api/testnet";// "https://seed1.spotcoin.com:10332";//"http://localhost:20332";//"http://seed4.aphelion-neo.com:10332";// "https://api.nel.group/api/testnet";



        public readonly static string root = "neo2";

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
