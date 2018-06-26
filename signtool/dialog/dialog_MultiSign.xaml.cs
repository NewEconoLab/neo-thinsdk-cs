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

namespace signtool
{
    /// <summary>
    /// dialog_MultiSign.xaml 的交互逻辑
    /// </summary>
    public partial class dialog_MultiSign : Window
    {
        public dialog_MultiSign()
        {
            InitializeComponent();
        }
        public Key key;
        bool bError = false;
        public static Key ShowDialog(Window owner)
        {
            var d = new dialog_MultiSign();
            d.key = new Key();
            d.key.prikey = null;
            d.key.multisignkey = true;
            d.key.MKey_NeedCount = 1;
            d.key.MKey_Pubkeys = new List<byte[]>();

            d.Owner = owner;
            if (d.ShowDialog() == true)
            {
                return d.key;
            }
            return null;
        }

        private void _sign_count_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                bError = false;
                if (int.TryParse(this._sign_count.Text, out int v))
                {
                    if (this.key != null)
                    {
                        this.key.MKey_NeedCount = v;
                        this._multisign_address.Content = this.key.GetAddress();
                    }
                }
            }
            catch
            {
                bError = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//ok
            if (bError)
            {
                MessageBox.Show("有错误，不能用");
                return;
            }
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//cancel
            this.key = null;
            this.DialogResult = false;
        }
        private void updateUI()
        {
            this._listKey.Items.Clear();
            for (var i = 0; i < this.key.MKey_Pubkeys.Count; i++)
            {
                var pubkey = this.key.MKey_Pubkeys[i];
                var pubkeystr = ThinNeo.Helper.Bytes2HexString(pubkey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                this._listKey.Items.Add(address + "[" + pubkeystr + "]");
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                bError = false;
                var pkey = ThinNeo.Helper.HexString2Bytes(this._add_pubkey.Text);
                if (pkey.Length != 33)
                    return;
                this.key.AddPubkey(pkey);
                updateUI();
                this._multisign_address.Content = this.key.GetAddress();
            }
            catch
            {
                bError = true;
            }
        }
    }
}