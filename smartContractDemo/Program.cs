using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace smartContractDemo
{
    interface ITest
    {
        string Name
        {
            get;
        }
        string ID
        {
            get;
        }
        Task Demo();
    }
    class Program
    {
        static Dictionary<string, ITest> alltest = new System.Collections.Generic.Dictionary<string, ITest>();
        static void RegTest(ITest test)
        {
            alltest[test.ID.ToLower()] = test;
        }
        static void InitTest()
        {
            var wif = "L2CmHCqgeNHL1i9XFhTLzUXsdr5LGjag4d56YY98FqEi4j5d83Mv";
            var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            var signdata = "010203ff1122abcd";
            var message = ThinNeo.Helper.HexString2Bytes(signdata);
            var data = ThinNeo.Helper.Sign(message, prikey);
            Console.WriteLine("wif=" + wif);
            Console.WriteLine("addr=" + addr);
            Console.WriteLine("sign=" + ThinNeo.Helper.Bytes2HexString(data));

            var b = ThinNeo.Helper.VerifySignature(message, data, pubkey);
            Console.WriteLine("verify=" + b);

            RegTest(new Height());

            RegTest(new SCDemo1());
            RegTest(new SCDemo2());
            RegTest(new SCDemo3());
            RegTest(new PubScDemo());
            RegTest(new Nep55());
            RegTest(new Nep55_1());
            RegTest(new Nep55_2());
            RegTest(new Nep55_3());
            RegTest(new Nep55_4());
            RegTest(new nnc_1());
            RegTest(new nnc_2());
            RegTest(new nnc_3());
            RegTest(new nnc_4());
            RegTest(new Tran_1());
            RegTest(new Tran_2());
            RegTest(new SGAS());
            RegTest(new CoinPool());
            RegTest(new nns_admin());
            RegTest(new nns_user());
            RegTest(new nns_sell());

        }
        static void ShowMenu()
        {
            Console.WriteLine("===all test===");
            foreach (var item in alltest)
            {
                Console.WriteLine("type '" + item.Key + "' to Run: " + item.Value.Name);
            }
            Console.WriteLine("type '?' to Get this list.");
        }
        async static void AsyncLoop()
        {
            while (true)
            {
                var line = Console.ReadLine().ToLower();
                if (line == "?" || line == "？" || line=="ls")
                {
                    ShowMenu();
                }
                else if (line == "")
                {
                    continue;
                }
                else if (alltest.ContainsKey(line))
                {
                    var test = alltest[line];
                    try
                    {
                        Console.WriteLine("[begin]" + test.Name);

                        await test.Demo();

                        Console.WriteLine("[end]" + test.Name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine("unknown line.");

                }
            }
        }

        static void Main(string[] args)
        {
            InitTest();
            ShowMenu();

            AsyncLoop();
            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

    }

}
