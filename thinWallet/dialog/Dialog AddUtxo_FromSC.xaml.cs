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
    public partial class Dialog_AddUtxo_FromSc : Window
    {
        public Dialog_AddUtxo_FromSc()
        {
            InitializeComponent();
        }
        string apiurl;
        Tools.Input value;
        public static Tools.Input ShowDialog(Window owner, string apiurl)
        {

            var d = new Dialog_AddUtxo_FromSc();
            d.Owner = owner;
            d.apiurl = apiurl;
            d.value = null;

            if (d.ShowDialog() == true)
            {
                return d.value;
            }
            return null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.scripthash == null || this.script == null)
                    return;
                var coin= this.listUTXO.SelectedItem as Tools.UTXOCoin;
                if (coin == null)
                    return;
                this.value = new Tools.Input();
                this.value.Coin = coin;
                this.value.From = this.scripthash;
                this.value.Script = this.script;
                this.DialogResult = true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        byte[] script;
        byte[] scripthash;
        void updateCode()
        {
            this.script = ThinNeo.Helper.HexString2Bytes(textAvm.Text);
            this.scripthash = ThinNeo.Helper.GetScriptHashFromScript(this.script);
            var addr = ThinNeo.Helper.GetAddressFromScriptHash(scripthash);
            this.textAddr.Text = addr;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "*.avm|*.avm";
                if (ofd.ShowDialog() == true)
                {
                    var bin = System.IO.File.ReadAllBytes(ofd.FileName);
                    textAvm.Text = ThinNeo.Helper.Bytes2HexString(bin);
                }
                updateCode();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void textAvm_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                updateCode();
            }
            catch
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {//listUTXO
            this.listUTXO.Items.Clear();

            var json = api_getUTXO(this.textAddr.Text).AsList();
            var asset = new Tools.Asset();
            asset.ParseUTXO(json);
            foreach (var t in asset.allcoins)
            {
                var aid = t.AssetID;
                foreach(var c in t.coins)
                {
                    this.listUTXO.Items.Add(c);
                }
            }
        }
        MyJson.IJsonNode api_getUTXO(string address)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var str = WWW.MakeRpcUrl(this.apiurl, "getutxo", new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(address) });
            var result = WWW.GetWithDialog(this, str);
            var json = MyJson.Parse(result).AsDict()["result"];
            return json;
        }
    }
}
