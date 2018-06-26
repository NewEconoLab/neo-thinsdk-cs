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
    /// Dialog_Input_password.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Import_Nep6 : Window
    {
        public Dialog_Import_Nep6()
        {
            InitializeComponent();
        }

        public byte[] prikey = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ThinNeo.NEP6.NEP6Account acc = this.listAccount.SelectedItem as ThinNeo.NEP6.NEP6Account;
            if (acc == null)
                return;
            if (acc.nep2key == null)
                return;

            try
            {
                this.prikey = acc.GetPrivate(nep6wallet.scrypt, pbox.Password);
                this.DialogResult = true;
            }
            catch
            {
                MessageBox.Show("密码错误或者其他错误");
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public static byte[] ShowDialog(Window owner)
        {
            var d = new Dialog_Import_Nep6();
            d.Owner = owner;
            if (d.ShowDialog() == true)
            {
                return d.prikey;
            }
            return null;
        }
        //load nep6
        ThinNeo.NEP6.NEP6Wallet nep6wallet = null;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "*.json|*.json";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    this.listAccount.Items.Clear();


                    nep6wallet = new ThinNeo.NEP6.NEP6Wallet(ofd.FileName);
                    foreach (var v in nep6wallet.accounts)
                    {
                        if (v.Value.nep2key != null)
                            this.listAccount.Items.Add(v.Value);
                    }
                    if (this.listAccount.Items.Count > 0)
                        this.listAccount.SelectedIndex = 0;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }
    }
}
