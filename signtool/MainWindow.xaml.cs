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
                    try
                    {
                        return "M" + MKey_NeedCount + "/" + MKey_Pubkeys.Count + ":" + GetMultiSignAddress();
                    }
                    catch
                    {
                        return "M<error>";
                    }
                }
            }
            public byte[] GetMultiContract()
            {
                if (!(1 <= this.MKey_NeedCount && MKey_NeedCount <= MKey_Pubkeys.Count && MKey_Pubkeys.Count <= 1024))
                    throw new ArgumentException();
                using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
                {
                    sb.EmitPushNumber(MKey_NeedCount);
                    foreach (var pkey in this.MKey_Pubkeys)
                    {
                        sb.EmitPushBytes(pkey);
                    }
                    sb.EmitPushNumber(MKey_Pubkeys.Count);
                    sb.Emit(ThinNeo.VM.OpCode.CHECKMULTISIG);
                    return sb.ToArray();
                }
            }
            public string GetMultiSignAddress()
            {
                if (this.multisignkey == false)
                {
                    return "<not a multisign key>";
                }
                else
                {//计算多签地址
                    var contract = GetMultiContract();
                    var scripthash = ThinNeo.Helper.GetScriptHashFromScript(contract);
                    var address = ThinNeo.Helper.GetAddressFromScriptHash(scripthash);
                    return address;
                }
            }
            public void AddPubkey(byte[] pubkey)
            {
                foreach (var k in this.MKey_Pubkeys)
                {
                    var s1 = ThinNeo.Helper.Bytes2HexString(k);
                    var s2 = ThinNeo.Helper.Bytes2HexString(pubkey);
                    if (s1 == s2)
                        return;
                }
                this.MKey_Pubkeys.Add(pubkey);
                this.MKey_Pubkeys.Sort((a, b) =>
                {
                    var pa = ThinNeo.Cryptography.ECC.ECPoint.DecodePoint(a, ThinNeo.Cryptography.ECC.ECCurve.Secp256r1);
                    var pb = ThinNeo.Cryptography.ECC.ECPoint.DecodePoint(b, ThinNeo.Cryptography.ECC.ECCurve.Secp256r1);
                    return pa.CompareTo(pb);
                });
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
                if(k.multisignkey)
                {
                    foreach(var subpubkey in k.MKey_Pubkeys)
                    {
                        var address = ThinNeo.Helper.GetAddressFromPublicKey(subpubkey);
                        TreeViewItem pubkey = new TreeViewItem();
                        pubkey.Header = address;
                        item.Items.Add(pubkey);
                    }
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
    }
}
