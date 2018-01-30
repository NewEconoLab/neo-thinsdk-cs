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
    public partial class Dialog_Script_Publish : Window
    {
        public Dialog_Script_Publish()
        {
            InitializeComponent();
        }
        public string rpcurl
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
        public static byte[] ShowDialog(Window owner, string rpcurl)
        {
            var d = new Dialog_Script_Publish();
            d.Owner = owner;
            d.rpcurl = rpcurl;
            if (d.ShowDialog() == true)
            {
                return d.script;
            }
            return null;
        }
        MyJson.IJsonNode rpc_getScript(byte[] scripthash)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var url = this.rpcurl;
            //url = "http://127.0.0.1:20332/";//本地测试

            var sid = ThinNeo.Helper.Bytes2HexString(scripthash);
            var str = WWW.MakeRpcUrl(url, "getcontractstate", new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(sid) });
            var result = WWW.GetWithDialog(this, str);
            if (result != null)
            {

                var json = MyJson.Parse(result);
                if (json.AsDict().ContainsKey("error"))
                    return null;
                return json.AsDict()["result"];
            }
            return null;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "*.avm|*.avm";
                if (ofd.ShowDialog() == true)
                {
                    var bin = System.IO.File.ReadAllBytes(ofd.FileName);
                    asmBinText.Text = ThinNeo.Helper.Bytes2HexString(bin);

                    var ops = ThinNeo.Compiler.Avm2Asm.Trans(bin);
                    this.asmList.Items.Clear();
                    bool bDyncall = false;
                    bool bStorage = false;
                    foreach (var op in ops)
                    {
                        var str = op.ToString();
                        this.asmList.Items.Add(op);
                        if (op.code == ThinNeo.VM.OpCode.APPCALL)
                        {

                            bool allzero = true;
                            for (var i = 0; i < op.paramData.Length; i++)
                            {
                                if (op.paramData[i] > 0)
                                {
                                    allzero = false;
                                    break;
                                }
                            }
                            if (allzero)
                            {
                                //dyncall
                                bDyncall = true;
                            }
                        }
                        if (op.code == ThinNeo.VM.OpCode.SYSCALL)
                        {
                            var name = System.Text.Encoding.UTF8.GetString(op.paramData);
                            if (name == "Neo.Storage.Put")
                            {
                                //need storage
                                bStorage = true;
                            }
                        }
                    }
                    iStorage.IsChecked = bStorage;
                    iDyncall.IsChecked = bDyncall;
                }
            }
            catch
            {

            }
        }

        private void jsonParam_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();
            sb.EmitPushString(iDescription.Text);
            sb.EmitPushString(iEmail.Text);
            sb.EmitPushString(iAuthor.Text);
            sb.EmitPushString(iVersion.Text);
            sb.EmitPushString(iName.Text);
            int need_storage = iStorage.IsChecked == true ? 1 : 0;
            int need_nep4 = iDyncall.IsChecked == true ? 2 : 0;
            sb.EmitPushNumber(need_storage | need_nep4);
            var br = ThinNeo.Helper.HexString2Bytes(iRType.Text);
            var bp = ThinNeo.Helper.HexString2Bytes(iPList.Text);
            sb.EmitPushBytes(br);
            sb.EmitPushBytes(bp);
            var _ss = ThinNeo.Helper.HexString2Bytes(this.asmBinText.Text);
            sb.EmitPushBytes(_ss);

            sb.EmitSysCall("Neo.Contract.Create");

            //sb.EmitSysCall("Neo.Contract.Create", script, parameter_list, return_type, need_storage | need_nep4, name, version, author, email, description);
            this.script = sb.ToArray();

            this.DialogResult = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
