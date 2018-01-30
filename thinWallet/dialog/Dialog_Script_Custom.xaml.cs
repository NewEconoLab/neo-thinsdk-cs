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
    public partial class Dialog_Script_Custom : Window
    {
        public Dialog_Script_Custom()
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
            var d = new Dialog_Script_Custom();
            d.Owner = owner;
            d.rpcurl = rpcurl;
            if (d.ShowDialog() == true)
            {
                return d.script;
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
                }
                updateCode();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        void updateCode()
        {
            try
            {
                this.script = ThinNeo.Helper.HexString2Bytes(asmBinText.Text);
                info2.Text = "length=" + script.Length;
                var hash = ThinNeo.Helper.GetScriptHashFromScript(script);
                info1.Text = "script hash=" + ThinNeo.Helper.Bytes2HexString(hash);
                var ops = ThinNeo.Compiler.Avm2Asm.Trans(script);
                this.asmList.Items.Clear();
                foreach (var op in ops)
                {
                    var str = op.ToString();
                    this.asmList.Items.Add(op);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        private void jsonParam_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.updateCode();
        }
    }
}
