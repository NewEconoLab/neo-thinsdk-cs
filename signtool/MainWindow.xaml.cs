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

        List<Key> keys = new List<Key>();
        Tx tx = null;
        string url = "https://api.nel.group/api/mainnet";
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
        private void AddMultiSignKey(Key _key)
        {

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
                if (k.multisignkey)
                {
                    foreach (var subpubkey in k.MKey_Pubkeys)
                    {
                        var address = ThinNeo.Helper.GetAddressFromPublicKey(subpubkey);
                        TreeViewItem pubkey = new TreeViewItem();
                        pubkey.Header = address;
                        item.Items.Add(pubkey);
                    }
                }
            }
            if (this.tx != null)
            {
                this.tx.ImportKeyInfo(this.keys);
            }
            UpdateTxUI();
        }
        private void UpdateTxUI()
        {
            treeTX.Items.Clear();
            if (this.tx == null)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = "null";
                treeTX.Items.Add(item);
            }
            else
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = tx.txraw.type + ":" + tx.txraw.GetHash();
                treeTX.Items.Add(item);

                foreach (var key in tx.keyinfos)
                {
                    TreeViewItem keyitem = new TreeViewItem();
                    keyitem.Header = key.Key + ":" + key.Value.type;
                    treeTX.Items.Add(keyitem);

                    if (key.Value.type == KeyType.Unknown)
                    {
                        TreeViewItem signdata = new TreeViewItem();
                        signdata.Header = "<unknown count...>";
                        keyitem.Items.Add(signdata);
                    }
                    if (key.Value.type == KeyType.Simple)
                    {
                        TreeViewItem signdata = new TreeViewItem();
                        var signstr = key.Value.signdata[0] == null ? "<null>" : ThinNeo.Helper.Bytes2HexString(key.Value.signdata[0]);
                        signdata.Header = "sign0:" + signstr;
                        keyitem.Items.Add(signdata);
                    }
                    if (key.Value.type == KeyType.MultiSign)
                    {
                        for (var i = 0; i < key.Value.MultiSignKey.MKey_Pubkeys.Count; i++)
                        {
                            var pubkey = key.Value.MultiSignKey.MKey_Pubkeys[i];
                            var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                            TreeViewItem signdata = new TreeViewItem();
                            var signstr = key.Value.signdata[i] == null ? "<null>" : ThinNeo.Helper.Bytes2HexString(key.Value.signdata[i]);
                            signdata.Header = "sign" + i + ":" + address + "=" + signstr;
                            keyitem.Items.Add(signdata);
                        }
                    }
                    keyitem.IsExpanded = true;
                }

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
            var key = dialog_MultiSign.ShowDialog(this);
            if (key != null)
            {
                AddMultiSignKey(key);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {//导入交易
            this.tx = dialog_importTX.ShowDialog(keys,url, this);
            UpdateKeyUI();

            UpdateTxUI();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {//导出交易
            dialog_exportTX.ShowDialog(this, this.tx.ToString());
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {//签名
            var signcount = 0;
            var data = tx.txraw.GetMessage();
            foreach (var key in this.keys)
            {
                if (key.prikey == null) continue;
                var addr = key.GetAddress();
                foreach (var k in tx.keyinfos)
                {
                    if (k.Value.type == KeyType.Simple)
                    {
                        if (k.Key == addr)//可以签一个
                        {
                            k.Value.signdata[0] = ThinNeo.Helper.Sign(data, key.prikey);
                            signcount++;
                        }
                    }
                    if (k.Value.type == KeyType.MultiSign)
                    {
                        for (var i = 0; i < k.Value.MultiSignKey.MKey_Pubkeys.Count; i++)
                        {
                            var pub = k.Value.MultiSignKey.MKey_Pubkeys[i];
                            var signaddr = ThinNeo.Helper.GetAddressFromPublicKey(pub);
                            if (signaddr == addr)//可以签一个
                            {
                                k.Value.signdata[i] = ThinNeo.Helper.Sign(data, key.prikey);
                                signcount++;
                            }
                        }

                    }
                }
            }
            if (signcount == 0)
            {
                MessageBox.Show("没找到可以签的");
            }
            UpdateTxUI();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if(tx.HasAllKeyInfo==false)
            {
                MessageBox.Show("簽名信息還不完整");
            }
            else
            {
                tx.FillRaw();
                var str =  ThinNeo.Helper.Bytes2HexString( tx.txraw.GetRawData());
                dialog_exportTX.ShowDialog(this, str);
            }
        }
        
        /**
         * 广播交易
         */
        private async void Button_Click_7(object sender, RoutedEventArgs e)
        {
            tx.FillRaw();
            var rawData = ThinNeo.Helper.Bytes2HexString(tx.txraw.GetRawData());
            byte[] data;
            var str = Helper.MakeRpcUrlPost(url, "sendrawtransaction", out data, new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(rawData) });
            var result =await Helper.HttpPost(str, data);
            var json = MyJson.Parse(result);
            MessageBox.Show(json.ToString());
            //if (json.AsDict()["result"].AsList()[0].AsDict()["endrawtransactionresult"].AsBool())
            //    MessageBox.Show(json.AsDict()["result"].AsList()[0].AsDict()["txid"].AsString());
            //else
            //    MessageBox.Show("交易失败");
        }

        private void radioIsMainnet_Checked(object sender, RoutedEventArgs e)
        {
            if (!(bool)this.radioIsMainnet.IsChecked)
            {
                url = "https://api.nel.group/api/testnet";
                //url = "http://localhost:20332";
            }
            else
            {
                url = "https://api.nel.group/api/mainnet";
            }
            Console.WriteLine(url);
        }
    }
}
