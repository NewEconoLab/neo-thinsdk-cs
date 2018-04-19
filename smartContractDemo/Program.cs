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
            RegTest(new nnc());
            RegTest(new nns_admin());
            RegTest(new nns_user());

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
                if (line == "?" || line == "？")
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
