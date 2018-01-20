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
    public partial class Dialog_Input_password : Window
    {
        public Dialog_Input_password()
        {
            InitializeComponent();
        }
        public string password;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            password = this.pbox.Password;
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public static string ShowDialog(Window owner)
        {
            var d = new Dialog_Input_password();
            d.Owner = owner;

            if (d.ShowDialog() == true)
            {
                return d.password;
            }
            return null;
        }
    }
}
