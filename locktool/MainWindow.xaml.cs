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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace locktool
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
        const string lockscript = "56c56b6c766b00527ac46c766b51527ac46c766b52527ac4616168184e656f2e426c6f636b636861696e2e4765744865696768746168184e656f2e426c6f636b636861696e2e4765744865616465726c766b53527ac46c766b00c36c766b53c36168174e656f2e4865616465722e47657454696d657374616d70a06c766b54527ac46c766b54c3640e00006c766b55527ac4621a006c766b51c36c766b52c3617cac6c766b55527ac46203006c766b55c3616c7566";
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static uint ToTimestamp(DateTime time)
        {
            return (uint)(time.ToUniversalTime() - unixEpoch).TotalSeconds;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //显示lock合约地址
            var lockbin = ThinNeo.Helper.HexString2Bytes(lockscript);
            var hash = ThinNeo.Helper.GetScriptHashFromScript(lockbin);
            var hashstr = hash.ToString();
            //addr d3cce84d0800172d09c88ccad61130611bd047a4
            label_lockscript.Text = hashstr;


            //显示公钥-》地址
            var pubkey = ThinNeo.Helper.HexString2Bytes(this.txt_pubkey.Text);

            var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            label_addr.Text = addr;

            //显示日期
            var date = datepicker.SelectedDate;
            if (date == null)
                date = datepicker.DisplayDate;
            var hour = int.Parse(txt_time.Text.Split(':')[0]);
            var datemix = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day,
                hour,
                date.Value.Minute,
                date.Value.Second);
            label_time.Text = datemix.ToLongDateString() + " " + datemix.ToLongTimeString();
            label_timeutc.Text = datemix.ToLongDateString() + " " + datemix.ToLongTimeString();


            //生成脚本
            var timestamp = ToTimestamp(datemix);
            byte[] script;
            //on genbutton
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                sb.EmitPushBytes(pubkey); //sb.EmitPush(GetKey().PublicKey);

                sb.EmitPushNumber(timestamp);//sb.EmitPush(timestamp);

                //// Lock 2.0 in mainnet tx:4e84015258880ced0387f34842b1d96f605b9cc78b308e1f0d876933c2c9134b
                sb.EmitAppCall(hash);
                //return Contract.Create(new[] { ContractParameterType.Signature }, sb.ToArray());
                script = sb.ToArray();
            }

            
            var callscripthash = ThinNeo.Helper.GetScriptHashFromScript(script);
            var contractaddr = ThinNeo.Helper.GetAddressFromScriptHash(callscripthash);
            this.txt_contract.Text = ThinNeo.Helper.Bytes2HexString(script);
            this.txt_addrout.Text = contractaddr;
        }
    }
}
