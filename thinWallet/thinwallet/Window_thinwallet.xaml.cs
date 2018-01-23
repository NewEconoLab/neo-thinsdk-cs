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
            Tools.CoinTool.Load();


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
            var result = await WWW.Get(str);
            var json = MyJson.Parse(result).AsDict()["result"];
            var height = json.AsInt() - 1;
            return height;
        }
        int apiHeight;
        int rpcHeight;
        async void update()
        {
            try
            {
                var height = await api_getHeight();
                apiHeight = height;
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
                rpcHeight = height;
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
                foreach (var o in ops)
                {
                    listCode.Items.Add(o);
                }
            }
        }
        MyJson.IJsonNode api_getUTXO()
        {
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
            var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);


            System.Net.WebClient wc = new System.Net.WebClient();
            var str = WWW.MakeRpcUrl(this.labelApi.Text, "getutxo", new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(address) });
            var result = WWW.GetWithDialog(this, str);
            var json = MyJson.Parse(result).AsDict()["result"];
            return json;
        }
        MyJson.IJsonNode api_getAllAssets()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var str = WWW.MakeRpcUrl(this.labelApi.Text, "getallasset");
            var result = WWW.GetWithDialog(this, str);
            var json = MyJson.Parse(result).AsDict()["result"];
            return json;
        }
        //getUtxo
        int lastCoinHeight = -1;
        bool initCoins = false;
        Tools.Asset myasset = null;


        decimal rpc_getNep5Balance(string nep5asset)
        {
            //make callscript
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
            var pubkeyhash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var asset = ThinNeo.Helper.HexString2Bytes(nep5asset);
            Neo.ScriptBuilder sb = new Neo.ScriptBuilder();
            sb.EmitPushBytes(pubkeyhash);
            sb.EmitPushNumber(1);
            sb.Emit(ThinNeo.VM.OpCode.PACK);
            sb.EmitPushString("balanceOf"); //name//totalSupply//symbol//decimals
            sb.EmitAppCall(asset.Reverse().ToArray());

            var nep5 = Tools.CoinTool.assetNep5[nep5asset];

            var symbol = ThinNeo.Helper.Bytes2HexString(sb.ToArray());
            var str = WWW.MakeRpcUrl(labelRPC.Text, "invokescript", new MyJson.JsonNode_ValueString(symbol));
            var resultstr = WWW.GetWithDialog(this, str);
            var json = MyJson.Parse(resultstr).AsDict()["result"].AsDict();
            if (json["state"].AsString().Contains("HALT") == false)
                throw new Exception("error state");
            var value = json["stack"].AsList()[0].AsDict();
            decimal outvalue = 0;
            if (value["type"].AsString() == "Integer")
            {
                outvalue = decimal.Parse(value["value"].AsString());
            }
            else
            {
                var bts = ThinNeo.Helper.HexString2Bytes(value["value"].AsString());
                var bi = new System.Numerics.BigInteger(bts);
                outvalue = (decimal)bi;
            }
            for (var i = 0; i < nep5.decimals; i++)
            {
                outvalue /= 10;
            }
            return outvalue;
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (lastCoinHeight == apiHeight)//高度一样就别刷了
                return;
            try
            {
                if (initCoins == false)
                {
                    var json = api_getAllAssets();
                    Tools.CoinTool.Load();
                    Tools.CoinTool.ParseUtxoAsset(json.AsList());
                    initCoins = true;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("InitCoins:" + err.Message);
            }
            try
            {

                var json = api_getUTXO().AsList();
                myasset = new Tools.Asset();
                myasset.ParseUTXO(json);
                foreach (var nep5 in Tools.CoinTool.assetNep5)
                {
                    var dec = rpc_getNep5Balance(nep5.Key);
                    if (dec > 0)
                    {
                        var nep5coin = new Tools.CoinType(nep5.Key, true);
                        nep5coin.Value = dec;
                        myasset.allcoins.Add(nep5coin);
                    }
                }
                lastCoinHeight = apiHeight;
                infoAssetHeight.Text = "Refresh:" + lastCoinHeight;
                //fill item
                treeCoins.Items.Clear();
                foreach (var cointype in myasset.allcoins)
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Tag = cointype;
                    if (cointype.NEP5)
                    {
                        item.Header = "(NEP5)" + Tools.CoinTool.GetName(cointype.AssetID) + ":" + cointype.Value;
                    }
                    else
                    {
                        item.Header = "(UTXO)" + Tools.CoinTool.GetName(cointype.AssetID) + ":" + cointype.Value;
                        foreach (var coin in cointype.coins)
                        {
                            TreeViewItem itemUtxo = new TreeViewItem();
                            itemUtxo.Tag = coin;
                            itemUtxo.Header = coin.value + " <== " + coin.fromID + "[" + coin.fromN + "]";
                            item.Items.Add(itemUtxo);
                        }
                    }
                    treeCoins.Items.Add(item);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("GetBalance:" + err.Message);
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Dialog_Config_Nep5.ShowDialog(this, this.labelRPC.Text);

            Tools.CoinTool.Save();
        }

        private void treeCoins_MouseMove(object sender, MouseEventArgs e)
        {
            TreeViewItem item = treeCoins.SelectedItem as TreeViewItem;
            if (item == null)
                return;
            var coin = item.Tag as Tools.UTXOCoin;
            if (coin == null)
                return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, coin, DragDropEffects.Copy);
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {

        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            Tools.UTXOCoin coin = e.Data.GetData(typeof(Tools.UTXOCoin)) as Tools.UTXOCoin;
            foreach (Tools.Input itemcoin in listInput.Items)
            {
                if (itemcoin.Coin.fromID == coin.fromID && itemcoin.Coin.fromN == coin.fromN)
                {
                    MessageBox.Show("already have this input.");
                    return;
                }
            }
            var input = new Tools.Input();
            input.Coin = coin.Clone();
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
            input.From = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            input.IsSmartContract = false;
            listInput.Items.Add(input);
            updateOutput();
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            Tools.Input coin = listInput.SelectedItem as Tools.Input;
            if (coin == null)
                return;
            listInput.Items.Remove(coin);
            updateOutput();
        }
        void _updateOutput()
        {
            Dictionary<string, Dictionary<string, decimal>> incoins = new Dictionary<string, Dictionary<string, decimal>>();
            Dictionary<string, bool> witnesses = new Dictionary<string, bool>();
            foreach (Tools.Input input in listInput.Items)
            {
                if (incoins.ContainsKey(input.From) == false)
                {
                    incoins[input.From] = new Dictionary<string, decimal>();
                }
                witnesses[input.From] = input.IsSmartContract;

                var incoin = incoins[input.From];
                if (incoin.ContainsKey(input.Coin.assetID) == false)
                    incoin[input.Coin.assetID] = input.Coin.value;
                else
                    incoin[input.Coin.assetID] += input.Coin.value;
            }

            {//处理见证人
                List<string> witness = new List<string>();
                List<Tools.Witnees> needdelw = new List<Tools.Witnees>();

                foreach (Tools.Witnees w in listWitness.Items)
                {                        
                    //无用鉴证人清除
                    if (w.IsSmartContract == false && witnesses.ContainsKey(w.address) == false)
                    {
                        needdelw.Add(w);
                    }
                    else
                    {
                        witness.Add(w.address);
                    }
                }
                foreach(var d in needdelw)
                {
                    listWitness.Items.Remove(d);
                }

                foreach(var f in witnesses.Keys)
                {
                    if(witness.Contains(f)==false)
                    {
                        Tools.Witnees wit = new Tools.Witnees();
                        wit.IsSmartContract = witnesses[f];
                        wit.address = f;
                        wit.iscript = null;
                        listWitness.Items.Add(wit);
                    }
                }
            }
            //去除无法完成的输出
            List<Tools.Output> needdel = new List<Tools.Output>();
            foreach (Tools.Output output in listOutput.Items)
            {
                bool bCon = false;
                foreach (var incoin in incoins)
                {
                    if (incoin.Value.ContainsKey(output.assetID))
                    {
                        bCon = true;
                        break;
                    }
                }
                if (bCon == false)
                    needdel.Add(output);
            }
            foreach (var del in needdel)
            {
                listOutput.Items.Remove(del);
            }

            //交换合约(utxo来自多个人)，不自动找零
            if (incoins.Count > 1)
            {
                return;
            }

            //清除找零
            needdel.Clear();
            foreach (Tools.Output output in listOutput.Items)
            {
                if (output.isTheChange)
                    needdel.Add(output);
            }
            foreach (var del in needdel)
            {
                listOutput.Items.Remove(del);
            }


            var moneys = incoins.First().Value;
            foreach (var money in moneys)
            {
                decimal value = 0;
                foreach (Tools.Output output in listOutput.Items)
                {
                    if (output.assetID == money.Key)
                    {
                        value += (decimal)output.Fix8 / (decimal)100000000.0;
                    }
                }
                decimal last = money.Value - value;
                if (last > 0)
                {
                    Tools.Output vout = new Tools.Output();
                    vout.assetID = money.Key;
                    vout.isTheChange = true;
                    vout.Target = incoins.First().Key;
                    vout.Fix8 = new System.Numerics.BigInteger(last * 100000000);
                    listOutput.Items.Add(vout);
                }
                else if (last < 0)
                {
                    throw new Exception("too mush output money.");
                }
            }
        }
        void updateOutput()
        {
            try
            {
                listOutput.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                _updateOutput();
            }
            catch
            {
                listOutput.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }

        //delete output
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Tools.Output output = listOutput.SelectedItem as Tools.Output;
            if (output == null)
                return;
            if (output.isTheChange)
                return;
            listOutput.Items.Remove(output);
            updateOutput();
        }
        //增加输出
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            List<string> assets = new List<string>();
            //弹出输出对话框
            foreach (Tools.Input i in this.listInput.Items)
            {
                if (assets.Contains(i.Coin.assetID) == false)
                    assets.Add(i.Coin.assetID);
            }
            var output = Dialog_Transfer_Target.ShowDialog(this, assets.ToArray());
            if (output != null)
            {
                listOutput.Items.Add(output);
            }
            updateOutput();

        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //从智能合约添加输入，弹出对话框
        }
    }
}
