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
    public partial class Dialog_Config_Nep5 : Window
    {
        public Dialog_Config_Nep5()
        {
            InitializeComponent();
        }
        string rpcurl;
        public static bool ShowDialog(Window owner, string rpcurl)
        {

            var d = new Dialog_Config_Nep5();
            d.rpcurl = rpcurl;
            d.Owner = owner;

            if (d.ShowDialog() == true)
            {
                return true;
            }
            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateNep5List();
        }
        void updateNep5List()
        {
            listNep5.Items.Clear();
            foreach (var nep5 in Tools.CoinTool.assetNep5)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = "(" + nep5.Value.symbol + ")" + nep5.Key;
                item.Tag = nep5.Key;
                listNep5.Items.Add(item);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sitem = listNep5.SelectedItem as ListBoxItem;
            if (sitem == null)
                return;
            if (MessageBox.Show("delete it?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Tools.CoinTool.assetNep5.Remove(sitem.Tag as string);
                Tools.CoinTool.SaveNep5();
                updateNep5List();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        //addassetid
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string[] result = GetNep5Info();
            var nep5 = new Tools.Nep5Info();
            nep5.name = result[0];
            nep5.symbol = result[1];
            nep5.decimals = int.Parse(result[2]);
            Tools.CoinTool.assetNep5[assetid.Text] = nep5;
            updateNep5List();

        }
        static byte[] createNep5FindScript(string _assetid)
        {
            var asset = ThinNeo.Helper.HexString2Bytes(_assetid);
            ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();
            sb.EmitPushNumber(0);
            sb.Emit(ThinNeo.VM.OpCode.PACK);
            sb.EmitPushString("name"); //name//totalSupply//symbol//decimals
            sb.EmitAppCall(asset.Reverse().ToArray());

            sb.EmitPushNumber(0);
            sb.Emit(ThinNeo.VM.OpCode.PACK);
            sb.EmitPushString("symbol"); //name//totalSupply//symbol//decimals
            sb.EmitAppCall(asset.Reverse().ToArray());

            sb.EmitPushNumber(0);
            sb.Emit(ThinNeo.VM.OpCode.PACK);
            sb.EmitPushString("decimals"); //name//totalSupply//symbol//decimals
            sb.EmitAppCall(asset.Reverse().ToArray());

            return sb.ToArray();
        }
        private string[] GetNep5Info()
        {
            var symbol = ThinNeo.Helper.Bytes2HexString(createNep5FindScript(assetid.Text));
            var str = WWW.MakeRpcUrl(rpcurl, "invokescript", new MyJson.JsonNode_ValueString(symbol));
            var info = WWW.GetWithDialog(this, str);
            var json = MyJson.Parse(info).AsDict()["result"].AsDict();
            if (json["state"].AsString().Contains("HALT") == false)
            {
                throw new Exception("not succ.");
            }
            string[] result = new string[3];
            var v = json["stack"].AsList();
            var bs0 = ThinNeo.Helper.HexString2Bytes(v[0].AsDict()["value"].AsString());
            var bs1 = ThinNeo.Helper.HexString2Bytes(v[1].AsDict()["value"].AsString());
            var t2 = v[2].AsDict()["type"].AsString();
            string str2 = null;
            if (t2 == "Integer")
            {
                str2 = v[2].AsDict()["value"].AsString();
            }
            else
            {
                var bs2 = ThinNeo.Helper.HexString2Bytes(v[2].AsDict()["value"].AsString());
                str2 = bs2[0].ToString();
            }
            result[0] = Encoding.UTF8.GetString(bs0);
            result[1] = Encoding.UTF8.GetString(bs1);
            result[2] = str2;
            return result;
        }
    }
}
