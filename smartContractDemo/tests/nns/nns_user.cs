using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace smartContractDemo
{
    class nns_user : ITest
    {
        public string Name => "NNS测试 用户";

        public string ID => "nns user";

        public static readonly Hash160 sc_nns = new Hash160("0x954f285a93eed7b4aed9396a7806a5812f1a5950");//nns 合约地址

        #region menuandlog
        public delegate Task testAction();
        public Dictionary<string, testAction> infos = new Dictionary<string, testAction>();
        string[] submenu;
        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public nns_user()
        {
            infos["get .test info"] = test_gettestinfo;
            infos["get abc.test info"] = test_gettestinfo;
            this.submenu = new List<string>(infos.Keys).ToArray();
        }
        #endregion
        static System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();

        Hash256 nameHash(string text)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return new Hash256(sha256.ComputeHash(data));
        }
        #region apitool
        static async Task<string> api_InvokeScript(Hash160 scripthash, string methodname, params string[] subparam)
        {
            byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                for (var i = 0; i < subparam.Length; i++)
                {
                    array.AddArrayValue(subparam[i]);
                }
                sb.EmitParamJson(array);
                sb.EmitPushString(methodname);
                sb.EmitAppCall(scripthash);
                data = sb.ToArray();
            }
            string script = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(nnc_1.api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var result = await Helper.HttpPost(url, postdata);
            return result;// subPrintLine("得到的结果是：" + result);
        }
        #endregion
        #region testarea
        async Task test_gettestinfo()
        {
            var r = await api_InvokeScript(sc_nns, "nameHash", "(string)test");
            subPrintLine("得到:" + r);
            var mh = nameHash("test");
            subPrintLine("calc=" + mh.ToString());
            var info = await api_InvokeScript(sc_nns, "getOwnerInfo", "(hex256)" + mh.ToString());
            subPrintLine("getinfo=" + info);
        }
        #endregion

        void showMenu()
        {
            for (var i = 0; i < submenu.Length; i++)
            {
                var key = submenu[i];
                subPrintLine((i + 1).ToString() + ":" + key);
            }
            subPrintLine("0:exit");
            subPrintLine("?:show this");
        }
        public async Task Demo()
        {
            showMenu();

            while (true)
            {
                var line = Console.ReadLine().Replace(" ", "").ToLower();
                if (line == "?" || line == "？")
                {
                    showMenu();
                }
                else if (line == "")
                {
                    continue;
                }
                else if (line == "0")
                {
                    return;
                }
                else//get .test's info
                {
                    var id = int.Parse(line) - 1;
                    var key = submenu[id];
                    subPrintLine("[begin]" + key);
                    try
                    {
                        await infos[key]();
                    }
                    catch (Exception err)
                    {
                        subPrintLine(err.Message);
                    }
                    subPrintLine("[end]" + key);
                }
            }
        }
    }
}
