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
    public partial class Dialog_Input : Window
    {
        public Dialog_Input()
        {
            InitializeComponent();
        }

        public string showtext
        {
            get
            {
                return this.label.Content.ToString();
            }
            set
            {
                this.label.Content = value;
            }
        }
        public string text = "";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            text = this.tbox.Text;
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public static string ShowDialog(Window owner,string label, string defvalue = "")
        {
            var d = new Dialog_Input();
            d.Owner = owner;
            d.showtext = label;
            d.text = defvalue;
            if (d.ShowDialog() == true)
            {
                return d.text;
            }
            return null;
        }
    }
}
