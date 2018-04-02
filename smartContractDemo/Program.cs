using System;
using Neo;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace smartContractDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始进行合约三连");
            Console.WriteLine("按任意健开始");
            Console.ReadKey();
            Console.WriteLine("开始合约一连--------根据合约脚本散列和存储的key，返回存储的value");
            new Demo1().Demo();
            Console.WriteLine("按任意健继续");
            Console.ReadKey();

            //demo2
            Console.WriteLine("再合约二连--------通过虚拟机传递脚本之后返回结果。");
            new Demo2().Demo();
            Console.WriteLine("按任意健继续");
            Console.ReadKey();


            //demo3
            Console.WriteLine("最后合约三连--------调用合约得到结果");
            new Demo3().Demo();
            Console.WriteLine("按任意健结束");
            Console.ReadKey();
        }

    }
}
