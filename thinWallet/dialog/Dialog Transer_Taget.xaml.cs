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
    public partial class Dialog_Transfer_Target : Window
    {
        public Dialog_Transfer_Target()
        {
            InitializeComponent();
        }
        string[] assets;
        void updateAssets()
        {
            foreach (var a in assets)
            {
                var asset = new Asset();
                asset.assetid = a;
                asset.showname = Tools.CoinTool.GetName(a);
                this.tokens.Items.Add(asset);
            }
        }
        Tools.Output output;
        public static Tools.Output ShowDialog(Window owner, string[] assets)
        {
            var d = new Dialog_Transfer_Target();
            d.assets = assets;
            d.updateAssets();
            d.Owner = owner;
            if (d.ShowDialog() == true)
            {
                return d.output;
            }
            return null;
        }
        class Asset
        {
            public string assetid;
            public string showname;
            public override string ToString()
            {
                return "(" + showname + ")" + assetid;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.output = new Tools.Output();
                this.output.isTheChange = false;
                var hash = ThinNeo.Helper.GetPublicKeyHashFromAddress(this.tboxAddr.Text);
                this.output.assetID = (this.tokens.SelectedItem as Asset).assetid;
                this.output.Target = ThinNeo.Helper.GetAddressFromScriptHash(hash);
                this.output.Fix8 = new System.Numerics.BigInteger(decimal.Parse(tboxValue.Text) * (decimal)100000000.0);
                if (this.output.Fix8 <= 0)
                    throw new Exception("must have a value greatthan zero.");
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
    }
}
