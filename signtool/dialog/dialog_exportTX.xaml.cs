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
    /// dialog_exportTX.xaml 的交互逻辑
    /// </summary>
    public partial class dialog_exportTX : Window
    {
        public dialog_exportTX()
        {
            InitializeComponent();
        }
        public static void ShowDialog(Window owner,string showinfo)
        {
            dialog_exportTX d = new dialog_exportTX();
            d.Owner = owner;
            d.text.AppendText(showinfo);
            d.ShowDialog();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
