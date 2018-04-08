using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace smartContractDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                /*
                Console.WriteLine("按任意健开始");
                Console.ReadKey();
                Console.WriteLine("合约一连--------根据合约脚本散列和存储的key，返回存储的value");
                new Demo1().Demo();
                Console.WriteLine("按任意健继续");
                Console.ReadKey();

                //demo2
                Console.WriteLine("合约二连--------通过虚拟机传递脚本之后返回结果。");
                new Demo2().Demo();
                Console.WriteLine("按任意健继续");
                Console.ReadKey();


                //demo3
                Console.WriteLine("合约三连--------调用合约得到结果");
                new Demo3().Demo();
                Console.WriteLine("按任意健结束");
                Console.ReadKey();
                */

                Console.WriteLine("发布");
                new Demo4().Demo();
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
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
