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
using System.Windows.Threading;

namespace thinWallet
{
    /// <summary>
    /// Window_thinwallet.xaml 的交互逻辑
    /// </summary>
    public partial class Window_thinwallet : Window
    {
        public Window_thinwallet()
        {
            InitializeComponent();
        }
        byte[] privatekey = null;
        void update_labelAccount()
        {

            if (this.privatekey == null)
            {
                labelAccount.Text = "no key";
            }
            else
            {
                try
                {
                    var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
                    var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                    labelAccount.Text = address + " with PrivateKey";
                }
                catch (Exception err)
                {
                    labelAccount.Text = err.Message;
                }
            }
        }

        //import WIF 
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var wif = Dialog_Input.ShowDialog(this, "input WIF");
            if (wif != null)
            {
                this.privatekey = null;
                try
                {
                    this.privatekey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
                }
                catch (Exception err)
                {
                    MessageBox.Show("error:" + err.Message);
                }
            }
            update_labelAccount();
        }

        //import Nep2
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var pkey = Dialog_Input_Nep2.ShowDialog(this);
            if (pkey != null)
            {
                this.privatekey = pkey;
            }
            update_labelAccount();

        }
        //import nep6
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var pkey = Dialog_Import_Nep6.ShowDialog(this);
            if (pkey != null)
            {
                this.privatekey = pkey;
            }
            update_labelAccount();

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("NEL Mainnet API not ready yet.");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(3000);
            timer.Tick += (s, ee) =>
              {
                  this.update();
              };
            timer.Start();
        }
        async Task<int> api_getHeight()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var str = WWW.MakeRpcUrl(this.labelApi.Text, "getblockcount");
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsList();
            var height = json[0].AsDict()["blockcount"].AsInt() - 1;
            return height;
        }
        async Task<int> rpc_getHeight()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var str = WWW.MakeRpcUrl(this.labelRPC.Text, "getblockcount");
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"];
            var height = json.AsInt() - 1;
            return height;
        }
        async void update()
        {
            try
            {
                var height = await api_getHeight();
                this.Dispatcher.Invoke(() =>
                {
                    this.stateAPI.Text = "height=" + height;
                });
            }
            catch
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.stateAPI.Text = "offline";
                });
            }
            try
            {
                var height = await rpc_getHeight();
                this.Dispatcher.Invoke(() =>
                {
                    this.stateRPC.Text = "height=" + height;
                });
            }
            catch
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.stateRPC.Text = "offline";
                });
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var script = Dialog_Script_Make.ShowDialog(this, this.labelRPC.Text);
            if (script != null)
            {
                var ops = ThinNeo.Compiler.Avm2Asm.Trans(script);
                listCode.Items.Clear();
                foreach(var o in ops)
                {
                    listCode.Items.Add(o);
                }
            }
        }
    }
}
