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
        string url;
        /// <summary>
        /// gen nep5 by 印玮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int gas = int.Parse(this.textConsume.Text);
                var tran = new ThinNeo.Transaction();
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                if (gas > 0)//0 or 1
                    tran.version = 1;
                else
                    tran.version = 0;

                var json = MyJson.Parse(this.textParams.Text).AsList();
                using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
                {
                    var list = json.AsList();
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        sb.EmitParamJson(list[i]);
                    }

                    var schash = new ThinNeo.Hash160(this.textContractHash.Text);
                    sb.EmitAppCall(schash);
                    var extdata = new ThinNeo.InvokeTransData();
                    tran.extdata = extdata;
                    extdata.script = sb.ToArray();
                    extdata.gas = gas;
                    if (gas > 0)
                    {
                        Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(url, this.textAddr.Text);
                        tran = Helper.makeTran(tran, dir["0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7"], null, new ThinNeo.Hash256("0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7"), extdata.gas);
                    }
                    else
                    {
                        tran.inputs = new ThinNeo.TransactionInput[0];
                        tran.outputs = new ThinNeo.TransactionOutput[0];
                    }
                }

                tran.attributes = new ThinNeo.Attribute[1];
                tran.attributes[0] = new ThinNeo.Attribute();
                tran.attributes[0].usage = ThinNeo.TransactionAttributeUsage.Script;
                var scripthashfrom = ThinNeo.Helper.GetPublicKeyHashFromAddress(this.textAddr.Text);

                tran.attributes[0].data = scripthashfrom;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    tran.SerializeUnsigned(ms);
                    var hexstr = ThinNeo.Helper.Bytes2HexString(ms.ToArray());
                    this.textTran.Text = hexstr;
                }
            }
            catch
            {
                MessageBox.Show("请填写正确的数据");
            }
        }

        public static ThinNeo.Transaction makeTran(List<Utxo> utxos, string targetaddr, ThinNeo.Hash256 assetid, decimal sendcount)
        {
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            var scraddr = "";
            utxos.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });
            decimal count = decimal.Zero;
            List<ThinNeo.TransactionInput> list_inputs = new List<ThinNeo.TransactionInput>();
            for (var i = 0; i < utxos.Count; i++)
            {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                scraddr = utxos[i].addr;
                if (count >= sendcount)
                {
                    break;
                }
            }
            tran.inputs = list_inputs.ToArray();
            if (count >= sendcount)//输入大于等于输出
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();
                //输出
                if (sendcount > decimal.Zero && targetaddr != null)
                {
                    ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                    output.assetId = assetid;
                    output.value = sendcount;
                    output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(targetaddr);
                    list_outputs.Add(output);
                }

                //找零
                var change = count - sendcount;
                if (change > decimal.Zero)
                {
                    ThinNeo.TransactionOutput outputchange = new ThinNeo.TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(scraddr);
                    outputchange.value = change;
                    outputchange.assetId = assetid;
                    list_outputs.Add(outputchange);

                }
                tran.outputs = list_outputs.ToArray();
            }
            else
            {
                throw new Exception("no enough money.");
            }
            return tran;
        }


        public static Tx ShowDialog(IList<Key> keys,string url,Window owner)
        {
            var d = new dialog_importTX();
            d.tx = new Tx();
            d.Owner = owner;
            d.keys = keys;
            d.url = url;
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

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var sid = this.textContractHash.Text;
            try
            {
                ThinNeo.Helper.HexString2Bytes(sid);
            }
            catch
            {
                MessageBox.Show("请输入正确的合约hash");
                return;
            }

            var str = Helper.MakeRpcUrl(url, "getcontractstate", new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(sid) });
            var result = await Helper.HttpGet(str);
            if (result == null)
            {
                MessageBox.Show("合约不存在");
                return;
            }
            var json = MyJson.Parse(result).AsDict()["result"].AsList()[0];
            var param = json.AsDict()["parameters"];
            var array = new MyJson.JsonNode_Array();
            foreach (var p in param.AsList())
            {
                if (p.AsString() == "String")
                {
                    array.Add(new MyJson.JsonNode_ValueString("(str)textParam"));
                }
                if (p.AsString() == "Array")
                {
                    array.Add(new MyJson.JsonNode_Array());
                }
                if (p.AsString() == "Boolean")
                {
                    array.Add(new MyJson.JsonNode_ValueNumber(false));
                }
                if (p.AsString() == "Bytes")
                {
                    array.Add(new MyJson.JsonNode_ValueString("(hex)0x00"));
                }
                if (p.AsString() == "UINT160")
                {
                    array.Add(new MyJson.JsonNode_ValueString("(hexbig)0xaa020304050607080910bb020304050607080910"));
                }
                if (p.AsString() == "UINT256")
                {
                    array.Add(new MyJson.JsonNode_ValueString("(hexbig)0xaa020304050607080910bb020304050607080910cc020304050607080910dd02"));
                }
                if (p.AsString() == "BigInteger")
                {
                    array.Add(new MyJson.JsonNode_ValueString("(int)735200"));
                }
            }
            StringBuilder sb = new StringBuilder();
            array.ConvertToStringWithFormat(sb, 4);
            this.textParams.Text = sb.ToString();
        }
    }
}
