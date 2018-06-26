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
    /// dialog_importTX.xaml 的交互逻辑
    /// </summary>
    public partial class dialog_importTX : Window
    {
        public dialog_importTX()
        {
            InitializeComponent();
        }
        Tx tx;
        IList<Key> keys;
        /// <summary>
        /// gen nep5 by 印玮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.InvocationTransaction;
            tran.version = 0;//0 or 1

            var addrfrom = "AMNFdmGuBrU1iaMbYd63L1zucYMdU9hvQU";
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + addrfrom);
                array.AddArrayValue("(addr)APjcXAa4M4A9gE3bt3zxFr93mzdH6YxGfo");
                array.AddArrayValue("(int)100");

                sb.EmitParamJson(array);
                sb.EmitPushString("transfer");
                var schash = new ThinNeo.Hash160("0x12329843449f29a66fb05974c2fb77713eb1689a");
                sb.EmitAppCall(schash);
                var extdata = new ThinNeo.InvokeTransData();
                tran.extdata = extdata;
                extdata.script = sb.ToArray();
                extdata.gas = 0;
            }
            tran.inputs = new ThinNeo.TransactionInput[0];
            tran.outputs = new ThinNeo.TransactionOutput[0];
            tran.attributes = new ThinNeo.Attribute[1];
            tran.attributes[0] = new ThinNeo.Attribute();
            tran.attributes[0].usage = ThinNeo.TransactionAttributeUsage.Script;
            var scripthashfrom = ThinNeo.Helper.GetPublicKeyHashFromAddress(addrfrom);

            tran.attributes[0].data = scripthashfrom;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                tran.SerializeUnsigned(ms);
                var hexstr = ThinNeo.Helper.Bytes2HexString( ms.ToArray());
                this.textTran.Text = hexstr;
            }
        }


        public static Tx ShowDialog(IList<Key> keys,Window owner)
        {
            var d = new dialog_importTX();
            d.tx = new Tx();
            d.Owner = owner;
            d.keys = keys;
            if (d.ShowDialog() == true)
            {
                return d.tx;
            }
            return null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.tx = null;
            this.DialogResult = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tx.FromString(keys,this.textTran.Text);
                this.DialogResult = true;
            }
            catch
            {
                MessageBox.Show("解析交易信息出错");
            }
        }
    }
}
