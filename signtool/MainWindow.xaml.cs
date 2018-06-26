using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace signtool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public class Key
        {
            public bool multisignkey;
            public byte[] prikey;
            public int MKey_NeedCount;
            public List<byte[]> MKey_Pubkeys;
            public override string ToString()
            {
                if (multisignkey == false)
                {
                    var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.prikey);
                    var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                    return address;
                }
                else
                {
                    var pkey = new List<byte[]>(this.MKey_Pubkeys);
                    pkey.Sort();
                    var outstr = "M(" + this.MKey_NeedCount + ")";
                    for (var i = 0; i < pkey.Count; i++)
                    {
                        var address = ThinNeo.Helper.GetAddressFromPublicKey(pkey[i]);
                        outstr += "[" + address + "]";
                    }
                    return outstr;
                }
            }
        }
        List<Key> keys = new List<Key>();
        private void AddSimpleKey(byte[] prikey)
        {
            var _key = new Key();
            _key.MKey_NeedCount = 0;
            _key.MKey_Pubkeys = null;
            _key.multisignkey = false;
            _key.prikey = prikey;
            foreach (var k in keys)
            {
                if (k.ToString() == _key.ToString())
                    return;
            }
            keys.Add(_key);

            UpdateKeyUI();
        }
        private void AddMultiSignKey(IEnumerable<byte[]> pubkeys, int needcount)
        {
            var _key = new Key();
            _key.MKey_NeedCount = needcount;
            _key.MKey_Pubkeys = new List<byte[]>(pubkeys);
            _key.multisignkey = true;
            _key.prikey = null;
            foreach (var k in keys)
            {
                if (k.ToString() == _key.ToString())
                    return;
            }
            keys.Add(_key);

            UpdateKeyUI();
        }
        private void UpdateKeyUI()
        {
            treeAccounts.Items.Clear();
            foreach (var k in keys)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = k.ToString();
                item.Tag = k;
                treeAccounts.Items.Add(item);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//load nep6
            var prikey = Dialog_Import_Nep6.ShowDialog(this);
            if (prikey != null)
            {
                AddSimpleKey(prikey);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//import wif string
            string wif = Dialog_Input.ShowDialog(this, "type wif here.");
            try
            {
                var privatekey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
                AddSimpleKey(privatekey);
            }
            catch (Exception err)
            {
                MessageBox.Show("error:" + err.Message);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {//创建多方签名

        }
    }
}
