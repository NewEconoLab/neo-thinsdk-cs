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
using thinWallet.dapp_plat;

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

            //自动刷一下utxo
            Button_Click_5(null, null);
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
            this.UpdateTranData();
            dapp_plat.LoadSimplePlugins();
            this.dapp_UpdatePluginsUI();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(3000);
            timer.Tick += (s, ee) =>
              {
                  this.update();
              };
            timer.Start();
        }
        async Task<ulong> api_getHeight()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var str = WWW.MakeRpcUrl(this.labelApi.Text, "getblockcount");
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsList();
            var height = ulong.Parse(json[0].AsDict()["blockcount"].ToString()) - 1;
            return height;
        }
        async Task<ulong> rpc_getHeight()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var str = WWW.MakeRpcUrl(this.labelRPC.Text, "getblockcount");
            var result = await WWW.Get(str);
            var json = MyJson.Parse(result).AsDict()["result"].ToString();
            var height = ulong.Parse(json) - 1;
            return height;
        }
        ulong apiHeight;
        ulong rpcHeight;
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

        byte[] lastScript;
        byte[] lastTranMessage;
        decimal? lastFee = null;
        void updateScript()
        {
            listCode.Items.Clear();
            if (lastScript != null)
            {
                var ops = ThinNeo.Compiler.Avm2Asm.Trans(lastScript);
                listCode.Items.Clear();
                foreach (var o in ops)
                {
                    listCode.Items.Add(o);
                }
            }
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var ss = Dialog_Script_Make.ShowDialog(this, this.labelRPC.Text);
            if (ss != null)
            {
                lastScript = ss;
                lastFee = null;
                labelFee.Text = "Fee:";
                updateScript();
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
        ulong lastCoinHeight = 0;
        bool initCoins = false;
        Tools.Asset myasset = null;


        decimal rpc_getNep5Balance(string nep5asset)
        {
            //make callscript
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
            var pubkeyhash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);

            var asset = ThinNeo.Helper.HexString2Bytes(nep5asset);
            ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();
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

            Tools.CoinTool.SaveNep5();
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
            input.From = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
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
            List<string> needWitness = new List<string>();

            //智能合約輸入，記錄
            Dictionary<string, byte[]> hadscript = new Dictionary<string, byte[]>();

            foreach (Tools.Input input in listInput.Items)
            {
                if (incoins.ContainsKey(input.Address) == false)
                {
                    incoins[input.Address] = new Dictionary<string, decimal>();
                }

                var incoin = incoins[input.Address];
                if (incoin.ContainsKey(input.Coin.assetID) == false)
                    incoin[input.Coin.assetID] = input.Coin.value;
                else
                    incoin[input.Coin.assetID] += input.Coin.value;

                if (needWitness.Contains(input.Address) == false)
                {
                    needWitness.Add(input.Address);
                }
                if (input.Script != null)
                {
                    hadscript[input.Address] = input.Script;
                }
            }

            var pubkey = this.privatekey != null ? ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey) : null;
            var address = pubkey != null ? ThinNeo.Helper.GetAddressFromPublicKey(pubkey) : null;
            {//处理见证人
                List<string> witness = new List<string>();
                foreach (ThinNeo.Witness w in listWitness.Items)
                {
                    witness.Add(w.Address);
                }


                foreach (var f in needWitness)
                {
                    if (witness.Contains(f) == false)
                    {
                        ThinNeo.Witness wit = new ThinNeo.Witness();
                        if (f == address)
                        {
                            wit.VerificationScript = ThinNeo.Helper.GetScriptFromPublicKey(pubkey);
                        }
                        else if (hadscript.ContainsKey(f))//自帶脚本
                        {
                            wit.VerificationScript = hadscript[f];
                        }
                        else
                        {
                            byte[] script = rpc_getScript(ThinNeo.Helper.GetPublicKeyHashFromAddress(f));
                            if (script != null)//并不知道试试是不是公開的智能合約 
                            {
                                wit.VerificationScript = script;
                            }
                        }
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
                        value += (decimal)output.Fix8;
                    }
                }
                decimal last = money.Value - value;
                if (last > 0)
                {
                    Tools.Output vout = new Tools.Output();
                    vout.assetID = money.Key;
                    vout.isTheChange = true;
                    vout.Target = incoins.First().Key;
                    vout.Fix8 = last;
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
            var b = Dialog_AddUtxo_FromSc.ShowDialog(this, this.labelApi.Text);
            if (b != null)
            {
                listInput.Items.Add(b);

                updateOutput();
            }
        }
        byte[] rpc_getScript(byte[] scripthash)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var url = this.labelRPC.Text;
            //url = "http://127.0.0.1:20332/";//本地测试

            var sid = ThinNeo.Helper.Bytes2HexString(scripthash);
            var str = WWW.MakeRpcUrl(url, "getcontractstate", new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(sid) });
            var result = WWW.GetWithDialog(this, str);
            if (result != null)
            {

                var json = MyJson.Parse(result);
                if (json.AsDict().ContainsKey("error"))
                    return null;
                var script = json.AsDict()["result"].AsDict()["script"].AsString();
                return ThinNeo.Helper.HexString2Bytes(script);
            }
            return null;
        }
        bool rpc_SendRaw(byte[] rawdata)
        {
            var sid = ThinNeo.Helper.Bytes2HexString(rawdata);

            var url = this.labelRPC.Text;
            byte[] data;
            var str = WWW.MakeRpcUrlPost(url, "sendrawtransaction", out data, new MyJson.IJsonNode[] { new MyJson.JsonNode_ValueString(sid) });
            var result = WWW.PostWithDialog(this, str, data);
            var json = MyJson.Parse(result);
            if (json.AsDict().ContainsKey("error"))
                return false;
            return json.AsDict()["result"].AsBool();
        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {//sign and broadcast 
            try
            {
                signAndBroadcast();
            }
            catch (Exception err)
            {
                MessageBox.Show("signAndBroadcast:" + err.Message);
            }
        }
        void UpdateTranData()
        {
            foreach (var his in Tools.CoinTool.TxHistory)
            {

                dataTran.Items.Add(new { txid = his.txid, time = his.time, type = his.type });
            }
        }
        void ClearTran()
        {
            this.listInput.Items.Clear();
            this.listOutput.Items.Clear();
            this.listWitness.Items.Clear();

            lastScript = null;
            lastFee = null;
            updateScript();
        }
        ThinNeo.Transaction GenTran()
        {
            if (this.listInput.Items.Count == 0)
            {
                MessageBox.Show("no input");
                return null;
            }
            if (this.listOutput.Items.Count == 0)
            {
                MessageBox.Show("no output");
                return null;
            }
            if (this.listWitness.Items.Count == 0)
            {
                MessageBox.Show("no witness");
                return null;
            }
            ThinNeo.Transaction trans = new ThinNeo.Transaction();
            trans.attributes = new ThinNeo.Attribute[0];
            if (tabCType.SelectedIndex == 0)
                trans.type = ThinNeo.TransactionType.ContractTransaction;
            else if (tabCType.SelectedIndex == 1)
            {
                if (lastScript == null)
                    throw new Exception("need script");
                if (lastFee.HasValue == false)
                    throw new Exception("need test script");

                trans.type = ThinNeo.TransactionType.InvocationTransaction;
                trans.extdata = new ThinNeo.InvokeTransData();
                (trans.extdata as ThinNeo.InvokeTransData).script = lastScript;
                (trans.extdata as ThinNeo.InvokeTransData).gas = lastFee.Value;
            }
            trans.inputs = new ThinNeo.TransactionInput[this.listInput.Items.Count];
            var _listOutput = new List<ThinNeo.TransactionOutput>();
            for (var i = 0; i < listInput.Items.Count; i++)
            {
                var item = listInput.Items[i] as Tools.Input;
                var input = new ThinNeo.TransactionInput();
                input.index = (ushort)item.Coin.fromN;
                var checkutxo = item.Coin.fromN + "_" + item.Coin.fromID;
                foreach (var v in Tools.CoinTool.UtxoHistory)
                {
                    if (v == checkutxo)
                    {
                        if (MessageBox.Show("use a input with spented money. Realy?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                            return null;
                    }
                }
                input.hash = ThinNeo.Helper.HexString2Bytes(item.Coin.fromID).Reverse().ToArray();//反转
                trans.inputs[i] = input;
            }
            for (var i = 0; i < listOutput.Items.Count; i++)
            {
                var item = listOutput.Items[i] as Tools.Output;
                if (string.IsNullOrEmpty(item.Target))
                {//扔钱
                    continue;
                }
                var output = new ThinNeo.TransactionOutput();
                output.assetId = ThinNeo.Helper.HexString2Bytes(item.assetID).Reverse().ToArray();//反转
                output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(item.Target);
                output.value = item.Fix8;
                _listOutput.Add(output);
            }
            trans.outputs = _listOutput.ToArray();

            return trans;
        }
        ThinNeo.Transaction signAndBroadcast()
        {

            ThinNeo.Transaction trans = GenTran();
            if (trans == null)
                return null;

            var msg = trans.GetMessage();
            trans.witnesses = new ThinNeo.Witness[this.listWitness.Items.Count];
            //检查签名

            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
            var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            for (var i = 0; i < listWitness.Items.Count; i++)
            {
                var item = listWitness.Items[i] as ThinNeo.Witness;
                var witness = new ThinNeo.Witness();
                witness.VerificationScript = item.VerificationScript;
                witness.InvocationScript = item.InvocationScript;
                if (witness.VerificationScript == null)
                {
                    throw new Exception("a VerificationScript is null");
                }
                if (item.Address != addr && witness.InvocationScript == null)//curkey 可以自動簽名
                {
                    throw new Exception("a InvocationScript is null");
                }
                //autoresign
                {
                    if (item.Address == addr)
                    {
                        var signdata = ThinNeo.Helper.Sign(msg, this.privatekey);
                        var sb = new ThinNeo.ScriptBuilder();
                        sb.EmitPushBytes(signdata);
                        witness.InvocationScript = sb.ToArray();
                    }
                }
                trans.witnesses[i] = witness;
            }

            var rawdata = trans.GetRawData();
            bool b = rpc_SendRaw(rawdata);
            if (b)
            {
                var str = Tools.CoinTool.RecordTran(trans);
                this.UpdateTranData();

                Tools.CoinTool.SaveRecord();
                MessageBox.Show("txid=" + str);
                ClearTran();
                return trans;
            }
            else
            {
                MessageBox.Show("transaction error");
                //ClearTran();
                return null;
            }
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            listTestScript.Items.Clear();

            try
            {
                var symbol = ThinNeo.Helper.Bytes2HexString(lastScript);
                byte[] data;
                var str = WWW.MakeRpcUrlPost(labelRPC.Text, "invokescript", out data, new MyJson.JsonNode_ValueString(symbol));
                var resultstr = WWW.PostWithDialog(this, str, data);
                var json = MyJson.Parse(resultstr).AsDict();
                var gas = json["result"].AsDict()["gas_consumed"].ToString();
                lastFee = decimal.Parse(gas);
                labelFee.Text = "Fee:" + lastFee;
                StringBuilder sb = new StringBuilder();
                json["result"].AsDict().ConvertToStringWithFormat(sb, 4);

                //移除撒GAS
                foreach (Tools.Output list in listOutput.Items)
                {
                    if (string.IsNullOrEmpty(list.Target) && Tools.CoinTool.id_GAS == list.assetID)
                    {
                        listOutput.Items.Remove(list);
                        updateOutput();
                        break;
                    }

                }
                if (lastFee > 10)
                {
                    Tools.Output newsb = new Tools.Output();
                    newsb.assetID = Tools.CoinTool.id_GAS;
                    newsb.isTheChange = false;
                    newsb.Target = "";
                    newsb.Fix8 = (lastFee.Value - (decimal)10.0);
                    listOutput.Items.Add(newsb);
                    updateOutput();
                }
                var lines = sb.ToString().Split('\n');
                foreach (var l in lines)
                {
                    listTestScript.Items.Add(l);
                }
            }
            catch (Exception err)
            {
                listTestScript.Items.Add("error:" + err.Message);
            }

        }

        //双击交易
        private void dataTran_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var obj = dataTran.SelectedItem as dynamic;
            if (obj == null)
                return;
            string txid = obj.txid;
            System.Diagnostics.Process.Start("http://be.nel.group/page/txInfo.html?txid=" + txid);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (labelAccount.Text == "nokey")
                return;
            var addr = labelAccount.Text.Split(' ')[0];
            System.Diagnostics.Process.Start("http://be.nel.group/page/address.html?addr=" + addr);

        }

        //发布智能合约
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            var ss = Dialog_Script_Publish.ShowDialog(this, this.labelRPC.Text);
            if (ss != null)
            {
                lastScript = ss;
                lastFee = null;
                labelFee.Text = "Fee:";
                updateScript();
            }
        }
        //load custom contract
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            var ss = Dialog_Script_Custom.ShowDialog(this, this.labelRPC.Text);
            if (ss != null)
            {
                lastScript = ss;
                lastFee = null;
                labelFee.Text = "Fee:";
                updateScript();
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {        //edit select witness
            var wit = listWitness.SelectedItem as ThinNeo.Witness;
            if (wit != null)
            {
                var b = Dialog_Witness_Edit.ShowDialog(this, wit);
                if (b != null)
                {
                    wit.InvocationScript = b;
                }
            }
            listWitness.InvalidateVisual();
        }
        //witness 和input 严格对应，这两个功能没任何意义
        //private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        //{        //add curkey witness

        //    var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
        //    var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
        //    foreach (ThinNeo.Witness k in listWitness.Items)
        //    {
        //        if (k.Address == address)
        //        {
        //            return;
        //        }
        //    }
        //    ThinNeo.Witness wit = new ThinNeo.Witness();
        //    wit.VerificationScript = ThinNeo.Helper.GetScriptFromPublicKey(pubkey);
        //    listWitness.Items.Add(wit);
        //}

        //private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        //{
        //    var wit = listWitness.SelectedItem as ThinNeo.Witness;
        //    if (wit != null)
        //    {
        //        listWitness.Items.Remove(wit);
        //    }
        //}
    }
}
