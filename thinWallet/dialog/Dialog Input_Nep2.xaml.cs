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
    public partial class Dialog_Input_Nep2 : Window
    {
        public Dialog_Input_Nep2()
        {
            InitializeComponent();
        }

        public byte[] prikey = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                prikey = ThinNeo.Helper.GetPrivateKeyFromNEP2(this.tbox.Text, this.pbox.Password);
                this.DialogResult = true;
            }
            catch
            {
                this.DialogResult = false;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public static byte[] ShowDialog(Window owner)
        {
            var d = new Dialog_Input_Nep2();
            d.Owner = owner;
            if (d.ShowDialog() == true)
            {
                return d.prikey;
            }
            return null;
        }
    }
}
