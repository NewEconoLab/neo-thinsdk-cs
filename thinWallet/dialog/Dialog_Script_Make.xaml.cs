using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace thinWallet
{
    /// <summary>
    /// Dialog_Input_password.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Script_Make : Window
    {
        public Dialog_Script_Make()
        {
            InitializeComponent();
        }
        public string apiurl
        {
            get
            {
                return textRpc.Text;
            }
            set
            {
                textRpc.Text = value;
            }
        }
        public byte[] script;
        public static byte[] ShowDialog(Window owner, string apiurl)
        {
            var d = new Dialog_Script_Make();
            d.Owner = owner;
            d.apiurl = apiurl;
            if (d.ShowDialog() == true)
            {
                return d.script;
            }
            return null;
        }
        MyJson.IJsonNode rpc_getScript(byte[] scripthash)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var url = this.apiurl;
            //url = "http://127.0.0.1:20332/";//本地测试

            var sid = ThinNeo.Helper.Bytes2HexString(scripthash);
            var str = WWW.MakeRpcUrl(url, "getcontractstate", new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(sid) });
            var result = WWW.GetWithDialog(this, str);
            if (result != null)
            {

                var json = MyJson.Parse(result);
                if (json.AsDict().ContainsKey("error"))
                    return null;
                return json.AsDict()["result"].AsList()[0];
            }
            return null;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sh = ThinNeo.Helper.HexString2Bytes(textScriptHash.Text);
                var json = rpc_getScript(sh);
                if (json == null)
                {
                    info1.Text = "no script";
                    info2.Text = "can't help you to fill the parameter json.";
                }
                else
                {
                    info1.Text = "scripthash=" + json.AsDict()["hash"].AsString();
                    info2.Text = "scriptlen=" + (json.AsDict()["script"].AsString().Length / 2);
                    var param = json.AsDict()["parameters"];
                    var array = new MyJson.JsonNode_Array();
                    foreach (var p in param.AsList())
                    {
                        if (p.AsString() == "String")
                        {
                            array.Add(new MyJson.JsonNode_ValueString("(str)textParam"));
                        }
                        if (p.AsString() == "Array")
                        {
                            array.Add(new MyJson.JsonNode_Array());
                        }
                        if (p.AsString() == "Boolean")
                        {
                            array.Add(new MyJson.JsonNode_ValueNumber(false));
                        }
                        if (p.AsString() == "Bytes")
                        {
                            array.Add(new MyJson.JsonNode_ValueString("(hex)0x00"));
                        }
                        if (p.AsString() == "UINT160")
                        {
                            array.Add(new MyJson.JsonNode_ValueString("(hexbig)0xaa020304050607080910bb020304050607080910"));
                        }
                        if (p.AsString() == "UINT256")
                        {
                            array.Add(new MyJson.JsonNode_ValueString("(hexbig)0xaa020304050607080910bb020304050607080910cc020304050607080910dd02"));
                        }
                        if (p.AsString() == "BigInteger")
                        {
                            array.Add(new MyJson.JsonNode_ValueString("(int)735200"));
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    array.ConvertToStringWithFormat(sb, 4);
                    jsonParam.Text = sb.ToString();
                }

            }
            catch
            {

            }
        }

        private void jsonParam_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                this.asmList.Items.Clear();

                var json = MyJson.Parse(jsonParam.Text).AsList();
                jsonParam.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();
                var list = json.AsList();
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    sb.EmitParamJson(list[i]);
                }
                var scripthash = ThinNeo.Helper.HexString2Bytes(textScriptHash.Text).Reverse().ToArray();
                sb.EmitAppCall(scripthash);
                this.script = sb.ToArray();
                var ops = ThinNeo.Compiler.Avm2Asm.Trans(this.script);
                for (int i = 0; i < ops.Length; i++)
                {
                    this.asmList.Items.Add(ops[i]);
                }
                this.asmBinText.Text = ThinNeo.Helper.Bytes2HexString(sb.ToArray());
            }
            catch
            {
                jsonParam.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
