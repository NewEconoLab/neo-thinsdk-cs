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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace thinWallet
{
    /// <summary>
    /// Page_FuncTest1.xaml 的交互逻辑
    /// </summary>
    public partial class Page_FuncTest_Wallet : Page
    {
        public Page_FuncTest_Wallet()
        {
            InitializeComponent();
        }


        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                tonep2_listWifOut.Items.Clear();
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(tonep2_textWif.Text);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                var nep2 = ThinNeo.Helper.GetNep2FromPrivateKey(prikey, tonep2_textPass.Text);
                tonep2_listWifOut.Items.Add("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
                tonep2_listWifOut.Items.Add("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
                tonep2_listWifOut.Items.Add("address=" + address);
                tonep2_listWifOut.Items.Add("Nep2=" + nep2);

            }
            catch
            {
                tonep2_listWifOut.Items.Clear();
                tonep2_listWifOut.Items.Add("parse nep2 error");

            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                fromnep2_listOut.Items.Clear();
                var prikey = ThinNeo.Helper.GetPrivateKeyFromNEP2(fromnep2_textNep2.Text, fromnep2_textPass.Text);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                fromnep2_listOut.Items.Add("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
                fromnep2_listOut.Items.Add("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
                fromnep2_listOut.Items.Add("address=" + address);
            }
            catch
            {
                fromnep2_listOut.Items.Clear();
                fromnep2_listOut.Items.Add("parse nep2 error");
            }
        }
        ThinNeo.NEP6.NEP6Wallet nep6wallet = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "*.json|*.json";
            if (ofd.ShowDialog() == true)
            {
                this.nep6_listOut.Items.Clear();

                nep6wallet = new ThinNeo.NEP6.NEP6Wallet(ofd.FileName);
                foreach (var v in nep6wallet.accounts)
                {
                    this.nep6_listOut.Items.Add(v.Value);
                }
            }
            //    Dialog_Input_password dlg = new Dialog_Input_password();
            //if(dlg.ShowDialog()==true)
            //{
            //    string pass = dlg.password;
            //    ThinNeo.NEP6.NEP6Wallet wallet = new ThinNeo.NEP6.NEP6Wallet();
            //}
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ThinNeo.NEP6.NEP6Account acc = this.nep6_listOut.SelectedItem as ThinNeo.NEP6.NEP6Account;
            if (acc == null)
                return;
            if (acc.nep2key == null)
                return;
            var pass = Dialog_Input_password.ShowDialog(null);
            if (pass != null)
            {
                try
                {
                    var prikey = acc.GetPrivate(nep6wallet.scrypt, pass);
                    labelPri.Content = ThinNeo.Helper.GetWifFromPrivateKey(prikey);
                }
                catch
                {
                    labelPri.Content = "密码错误或者其他错误";
                }
            }
        }
    }
}
