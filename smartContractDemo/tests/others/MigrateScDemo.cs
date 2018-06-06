using smartContractDemo.tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace smartContractDemo
{
    public class testtt
    {
        public int a;
        public int b;
    }
    class MigrateScDemo : ITest
    {
        public string Name => "升级智能合约";

        public string ID => "mig";

        public async Task Demo()
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(Config.test_wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(Config.api, address);

            //从文件中读取合约脚本
            byte[] script = System.IO.File.ReadAllBytes("dapp_sgas_migrate.avm"); //这里填你的合约所在地址
            string str_script = ThinNeo.Helper.Bytes2HexString(script);
            byte[] aa = ThinNeo.Helper.HexString2Bytes(str_script);
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                //倒叙插入数据
                var array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(bytes)" + str_script);
                array.AddArrayValue("(bytes)0710");
                array.AddArrayValue("(bytes)05");
                array.AddArrayValue("(int)"+ 5);
                array.AddArrayValue("(str)合约测试");//name
                array.AddArrayValue("(str)1");//version
                array.AddArrayValue("(str)ss");//author
                array.AddArrayValue("(str)1");//email
                array.AddArrayValue("(str)sssss");//desc
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)upgrade"));//参数倒序入
                var shash = Config.dapp_sgas;
                sb.EmitAppCall(shash);

                string scriptPublish = ThinNeo.Helper.Bytes2HexString(sb.ToArray());
                byte[] postdata;
                var url = Helper.MakeRpcUrlPost(Config.api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(scriptPublish));
                var result = await Helper.HttpPost(url, postdata);
                //string result = http.Post(api, "invokescript", new MyJson.JsonNode_Array() { new MyJson.JsonNode_ValueString(scriptPublish) },Encoding.UTF8);
                var consume = (((MyJson.Parse(result) as MyJson.JsonNode_Object)["result"] as MyJson.JsonNode_Array)[0] as MyJson.JsonNode_Object)["gas_consumed"].ToString();
                decimal gas_consumed = decimal.Parse(consume);
                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.gas = 500;// Math.Ceiling(gas_consumed - 10);
                extdata.script = sb.ToArray();

                //拼装交易体
                ThinNeo.Transaction tran = Helper.makeTran(dir[Config.id_GAS], null, new ThinNeo.Hash256(Config.id_GAS), extdata.gas);
                tran.version = 1;
                tran.extdata = extdata;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);
                url = Helper.MakeRpcUrlPost(Config.api_local, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
                result = await Helper.HttpPost(url, postdata);

                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                Console.WriteLine(resJO.ToString());
            }
        }
    }
}
