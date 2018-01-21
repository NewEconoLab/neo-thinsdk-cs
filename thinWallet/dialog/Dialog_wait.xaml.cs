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
    public partial class Dialog_Wait : Window
    {
        public Dialog_Wait()
        {
            InitializeComponent();
        }
        public delegate Task<object> waitcall();
        public object callresult;
        public Exception callerror;
        public waitcall call;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            callresult = null;
            callerror = null;
            try
            {
                callresult = await this.call();

                this.DialogResult = true;
            }
            catch (Exception err)
            {
                this.callerror = err;
                this.DialogResult = false;
            }
        }
    }
}
