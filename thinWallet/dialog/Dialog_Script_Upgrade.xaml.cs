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
    public partial class Dialog_Script_Upgrade : Window
    {
        public Dialog_Script_Upgrade()
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
            var d = new Dialog_Script_Upgrade();
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
                return json.AsDict()["result"].AsList()[0] ;
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
                    iCharge.IsChecked = true;
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
            int need_storage = iStorage.IsChecked == true ? 1 : 0;
            int need_nep4 = iDyncall.IsChecked == true ? 2 : 0;
            int can_charge = iCharge.IsChecked == true ? 4 : 0;

            ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();
            //倒叙插入数据
            var array = new MyJson.JsonNode_Array();
            array.AddArrayValue("(bytes)" + this.asmBinText.Text);
            array.AddArrayValue("(bytes)"+ iPList.Text);
            array.AddArrayValue("(bytes)"+iRType.Text);
            array.AddArrayValue("(int)"+ (need_storage | need_nep4 | can_charge));
            array.AddArrayValue("(str)"+iName.Text);//name
            array.AddArrayValue("(str)"+iVersion.Text);//version
            array.AddArrayValue("(str)"+iAuthor.Text);//author
            array.AddArrayValue("(str)"+iEmail.Text);//email
            array.AddArrayValue("(str)"+ iDescription.Text);//desc
            sb.EmitParamJson(array);//参数倒序入
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)upgrade"));//参数倒序入
            var shash = new ThinNeo.Hash160(iOldScHash.Text); ;
            sb.EmitAppCall(shash);
            this.script = sb.ToArray();
            this.DialogResult = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "*.avm|*.avm";
                if (ofd.ShowDialog() == true)
                {
                    var bin = System.IO.File.ReadAllBytes(ofd.FileName);
                    var hash = ThinNeo.Helper.GetScriptHashFromScript(bin);
                    var strHash = ThinNeo.Helper.Bytes2HexString(hash);
                    iOldScHash.Text = "0x"+strHash;
                }
            }
            catch
            {

            }
        }
    }
}
