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
using System.Windows.Shapes;

namespace thinWallet
{
    /// <summary>
    /// Window_funcTest.xaml 的交互逻辑
    /// </summary>
    public partial class Window_funcTest : Window
    {
        public Window_funcTest()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listOut.Items.Clear();
                var addr = ThinNeo.Helper.GetPublicKeyHashFromAddress(textAddress.Text);
                listOut.Items.Add("right hash");
                listOut.Items.Add("pubkeyhash=" + ThinNeo.Helper.Bytes2HexString(addr));
            }
            catch
            {
                try
                {
                    listOut.Items.Clear();
                    var addr = ThinNeo.Helper.GetPublicKeyHashFromAddress_WithoutCheck(textAddress.Text);
                    listOut.Items.Add("wrong hash");
                    listOut.Items.Add("pubkeyhash=" + ThinNeo.Helper.Bytes2HexString(addr));
                    var addr2 = ThinNeo.Helper.GetAddressFromScriptHash(addr);
                    listOut.Items.Add("wrong addr2=" + addr2);
                }
                catch
                {
                    listOut.Items.Clear();
                    listOut.Items.Add("wrong address,can not parse.");
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                listWifOut.Items.Clear();
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(textWif.Text);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var script = ThinNeo.Helper.GetScriptFromPublicKey(pubkey);
                var scripthash = ThinNeo.Helper.GetScriptHashFromScript(script);
                var address = ThinNeo.Helper.GetAddressFromScriptHash(scripthash);

                listWifOut.Items.Add("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
                listWifOut.Items.Add("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
                listWifOut.Items.Add("script=" + ThinNeo.Helper.Bytes2HexString(script));
                listWifOut.Items.Add("scripthash=" + ThinNeo.Helper.Bytes2HexString(scripthash));
                listWifOut.Items.Add("address=" + address);

            }
            catch
            {
                listWifOut.Items.Clear();
                listWifOut.Items.Add("wrong wif,can not parse.");

            }
        }
        byte[] lastprikey;
        byte[] lastpubkey;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                sign_listWifOut.Items.Clear();
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(sign_textWif.Text);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);


                sign_listWifOut.Items.Add("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
                sign_listWifOut.Items.Add("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
                sign_listWifOut.Items.Add("address=" + address);
                lastprikey = prikey;
                lastpubkey = pubkey;

            }
            catch
            {
                listWifOut.Items.Clear();
                listWifOut.Items.Add("wrong wif,can not parse.");

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                var msg = ThinNeo.Helper.HexString2Bytes(signdata.Text);
                var result = ThinNeo.Helper.Sign(msg, lastprikey);
                signresult.Text = ThinNeo.Helper.Bytes2HexString(result);
            }
            catch
            {
                signresult.Text = "<E>sign error";
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                var msg = ThinNeo.Helper.HexString2Bytes(signdata.Text);
                var result = ThinNeo.Helper.HexString2Bytes(signresult.Text);
                var b = ThinNeo.Helper.VerifySignature(msg, result, lastpubkey);
                MessageBox.Show("vertify=" + b);
            }
            catch
            {
                MessageBox.Show("vertify error");
            }

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                tonep2_listWifOut.Items.Clear();
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(tonep2_textWif.Text);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                var nep2 = ThinNeo.Helper.GetNep2FromPrivateKey(prikey, tonep2_textPass.Text);
                tonep2_listWifOut.Items.Add("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
                tonep2_listWifOut.Items.Add("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
                tonep2_listWifOut.Items.Add("address=" + address);
                tonep2_listWifOut.Items.Add("Nep2=" + nep2);

            }
            catch
            {
                tonep2_listWifOut.Items.Clear();
                tonep2_listWifOut.Items.Add("parse nep2 error");

            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                fromnep2_listOut.Items.Clear();
                var prikey = ThinNeo.Helper.GetPrivateKeyFromNEP2(fromnep2_textNep2.Text, fromnep2_textPass.Text);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                fromnep2_listOut.Items.Add("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
                fromnep2_listOut.Items.Add("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
                fromnep2_listOut.Items.Add("address=" + address);
            }
            catch
            {
                fromnep2_listOut.Items.Clear();
                fromnep2_listOut.Items.Add("parse nep2 error");
            }
        }
    }
}
