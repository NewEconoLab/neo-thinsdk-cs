using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

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
        void Demo();
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
            RegTest(new SCDemo1());
            RegTest(new SCDemo2());
            RegTest(new SCDemo3());
            RegTest(new PubScDemo());

        }
        static void ShowMenu()
        {
            Console.WriteLine("===all test===");
            foreach (var item in alltest)
            {
                Console.WriteLine("type '" + item + "' to Run: " + item.Value.Name);
            }
            Console.WriteLine("type '?' to Get this list.");
        }
        static void Main(string[] args)
        {
            InitTest();
            ShowMenu();
            while (true)
            {
                var line = Console.ReadLine().ToLower();
                if (line == "?" || line == "？")
                {
                    ShowMenu();
                }
                else if(line=="")
                {
                    continue;
                }
                else if (alltest.ContainsKey(line))
                {
                    var test = alltest[line];
                    try
                    {
                        Console.WriteLine("[begin]" + test.Name);

                        test.Demo();

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

    }


    public class Utxo
    {
        public string addr;
        public string txid;
        public string asset;
        public decimal value;
        public int n;
        public Utxo(string _addr, string _txid, string _asset, decimal _value, int _n)
        {
            this.addr = _addr;
            this.txid = _txid;
            this.asset = _asset;
            this.value = _value;
            this.n = _n;
        }
    }
}
