﻿using System;
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
        {
            this.tx = dialog_importTX.ShowDialog(keys, this);
            UpdateTxUI();
        }
    }
}