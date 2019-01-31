using Newtonsoft.Json.Linq;
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
        string api = "https://api.nel.group/api/testnet";

        const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        const string lockscript = "56c56b6c766b00527ac46c766b51527ac46c766b52527ac4616168184e656f2e426c6f636b636861696e2e4765744865696768746168184e656f2e426c6f636b636861696e2e4765744865616465726c766b53527ac46c766b00c36c766b53c36168174e656f2e4865616465722e47657454696d657374616d70a06c766b54527ac46c766b54c3640e00006c766b55527ac4621a006c766b51c36c766b52c3617cac6c766b55527ac46203006c766b55c3616c7566";
        // script hash  = d3cce84d0800172d09c88ccad61130611bd047a4
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static uint ToTimestamp(DateTime time)
        {
            return (uint)(time.ToUniversalTime() - unixEpoch).TotalSeconds;
        }
        public byte[] script_LockContract;
        public byte[] scripthash_LockContract;
        public string address_LockContract;
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

            //默认提取地址就是私钥地址
            txt_get_addr.Text = addr;
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
            label_timeutc.Text = datemix.ToUniversalTime().ToLongDateString() + " " + datemix.ToUniversalTime().ToLongTimeString();


            //生成脚本
            var timestamp = ToTimestamp(datemix);
            byte[] script;
            //on genbutton
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                sb.EmitPushBytes(pubkey); //sb.EmitPush(GetKey().PublicKey);

                sb.EmitPushNumber(timestamp);//sb.EmitPush(timestamp);

                //// Lock 2.0 in mainnet tx:4e84015258880ced0387f34842b1d96f605b9cc78b308e1f0d876933c2c9134b
                sb.EmitAppCall(hash); // script hash  = d3cce84d0800172d09c88ccad61130611bd047a4
                //return Contract.Create(new[] { ContractParameterType.Signature }, sb.ToArray());
                script = sb.ToArray();
            }


            var callscripthash = ThinNeo.Helper.GetScriptHashFromScript(script);
            var contractaddr = ThinNeo.Helper.GetAddressFromScriptHash(callscripthash);
            this.txt_contract.Text = ThinNeo.Helper.Bytes2HexString(script);
            this.txt_addrout.Text = contractaddr;


            //from addr
            this.txt_get_srcaddr.Text = contractaddr;

            address_LockContract = contractaddr;
            script_LockContract = script;
            scripthash_LockContract = callscripthash;
        }
        class Asset
        {
            public bool isnep5;
            public string assetID;
            public double balance;
            public string name;
            public int decimals = 0;
            public override string ToString()
            {
                return (isnep5 ? "<NEP5>" : "<UTXO>") + name + "=" + balance;
            }
        }
        bool hasGas;
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            listAsset.Items.Clear();
            {
                //fill utxo balance;
                var url = Demo.Helper.MakeRpcUrlPost(this.api, "getbalance", out byte[] data, new MyJson.JsonNode_ValueString(address_LockContract));
                var json = await Demo.Helper.HttpPost(url, data);
                var jsonb = Newtonsoft.Json.Linq.JObject.Parse(json);
                var result = jsonb["result"] as JArray;
                hasGas = false;
                if (result != null)
                {
                    foreach (JObject r in result)
                    {
                        var asset = new Asset();
                        asset.isnep5 = false;
                        asset.assetID = (string)r["asset"];
                        asset.balance = (double)r["balance"];
                        foreach (JObject nameitem in r["name"] as JArray)
                        {
                            asset.name = nameitem["name"].ToString();
                        }
                        if (asset.assetID == id_GAS)
                        {
                            asset.name = "::GAS";
                            hasGas = true;
                        }
                        this.listAsset.Items.Add(asset);
                        var url2 = Demo.Helper.MakeRpcUrlPost(this.api, "getasset", out byte[] data2, new MyJson.JsonNode_ValueString(asset.assetID));
                        var json2 = await Demo.Helper.HttpPost(url2, data2);
                        var jsonb2 = Newtonsoft.Json.Linq.JObject.Parse(json2);

                        asset.decimals = (int)jsonb2["result"][0]["precision"];
                    }
                }
            }
            {//fill nep5 balance.
                var url2 = Demo.Helper.MakeRpcUrlPost(this.api, "getallnep5assetofaddress", out byte[] data2, new MyJson.JsonNode_ValueString(address_LockContract));
                var json = await Demo.Helper.HttpPost(url2, data2);
                var jsonb = Newtonsoft.Json.Linq.JObject.Parse(json);
                var result = (jsonb["result"] as JArray);
                if (result != null)
                {
                    foreach (JObject item in result)
                    {
                        var asset = new Asset();
                        asset.isnep5 = true;
                        asset.assetID = (string)item["assetid"];
                        var urlsub = Demo.Helper.MakeRpcUrlPost(this.api, "getnep5asset", out byte[] datasub, new MyJson.JsonNode_ValueString(asset.assetID));
                        var jsonsub = await Demo.Helper.HttpPost(urlsub, datasub);
                        var jsonsubb = Newtonsoft.Json.Linq.JObject.Parse(jsonsub);
                        asset.name = (string)jsonsubb["result"][0]["name"];
                        asset.decimals = (int)jsonsubb["result"][0]["decimals"];

                        var urlb = Demo.Helper.MakeRpcUrlPost(this.api, "getnep5balanceofaddress", out byte[] databsub, new MyJson.JsonNode_ValueString(asset.assetID), new MyJson.JsonNode_ValueString(address_LockContract));
                        var jsonbsub = await Demo.Helper.HttpPost(urlb, databsub);
                        var jsonbsubb = Newtonsoft.Json.Linq.JObject.Parse(jsonbsub);
                        asset.balance = (double)jsonbsubb["result"][0]["nep5balance"];
                        this.listAsset.Items.Add(asset);

                    }
                }
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            byte[] rawdata = await MakeScript(true);

            if (rawdata == null)
                return;

            var txt = ThinNeo.Helper.Bytes2HexString(rawdata);
            this.txt_get_result.Text = txt;

            //and broadcast;
            {
                //ThinNeo.Transaction testde = new ThinNeo.Transaction();
                //testde.Deserialize(new System.IO.MemoryStream(tran.GetRawData()));

                byte[] postdata;
                var url = Demo.Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(txt));
                var resultstr = await Demo.Helper.HttpPost(url, postdata);

                this.txt_bc_result.Text = resultstr;
                MessageBox.Show("boardcast that=" + resultstr);
            }
        }
        public async Task<byte[]> MakeScript(bool sign)
        {
            var payaddr = label_addr.Text;
            var paygas = 0.001;
            var utxos = await Demo.Helper.GetUtxosToPay(api, payaddr, id_GAS, paygas);
            if (utxos.Count == 0)
            {
                MessageBox.Show("you do not have gas for sendraw.");
                return null;

            }
            var asset = listAsset.SelectedItem as Asset;
            if (asset == null || asset.isnep5 == false)
            {
                MessageBox.Show("this is not a nep5");
                return null;
            }
            var targetaddr = txt_get_addr.Text;
            var targetbalance = double.Parse(txt_get_balance.Text);
            for (var i = 0; i < asset.decimals; i++)
            {
                targetbalance *= 10;
            }


            if (sign)
            {
                var paywif = txt_wifGetter.Text;
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(paywif);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
                if (address != payaddr)
                {
                    MessageBox.Show("error wif for pubkey in lock contract.");
                    return null;
                }
            }
            //MakeTran
            ThinNeo.Transaction tran = null;
            {

                byte[] script = null;
                using (var sb = new ThinNeo.ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(addr)" + address_LockContract);//from
                    array.AddArrayValue("(addr)" + targetaddr);//to
                    array.AddArrayValue("(int)" + (ulong)targetbalance);//value
                    sb.EmitParamJson(array);//参数倒序入
                    sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)transfer"));//参数倒序入
                    ThinNeo.Hash160 shash = new ThinNeo.Hash160(asset.assetID);
                    sb.EmitAppCall(shash);//nep5脚本
                    script = sb.ToArray();
                }

                tran = Demo.Helper.makeTran(null, payaddr, new ThinNeo.Hash256(id_GAS), 0, (decimal)0.001, utxos, payaddr);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = script;
                tran.attributes = new ThinNeo.Attribute[1];
                tran.attributes[0] = new ThinNeo.Attribute();
                tran.attributes[0].usage = ThinNeo.TransactionAttributeUsage.Script;
                tran.attributes[0].data = scripthash_LockContract;
            }

            if (sign)
            {
                var paywif = txt_wifGetter.Text;
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(paywif);
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                ////sign 
                var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
                tran.AddWitness(signdata, pubkey, address);

                var sb2 = new ThinNeo.ScriptBuilder();
                sb2.EmitPushBytes(signdata);

                var iscript = sb2.ToArray();
                tran.AddWitnessScript(script_LockContract, iscript);
                return tran.GetRawData();

            }
            else
            {
                return tran.GetMessage();
            }

        }


        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            byte[] rawdata = await MakeScript(true);
            if (rawdata == null)
                return;

            var txt = ThinNeo.Helper.Bytes2HexString(rawdata);
            this.txt_get_result.Text = txt;

        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            byte[] rawdata = await MakeScript(false);
            if (rawdata == null)
                return;

            var txt = ThinNeo.Helper.Bytes2HexString(rawdata);
            this.txt_get_result.Text = txt;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(this.api.Contains("testnet"))
            {
                this.api = "https://api.nel.group/api/mainnet";
            }
            else
            {
                this.api = "https://api.nel.group/api/testnet";
            }
            this.txt_API.Text = api;
        }
    }
}
