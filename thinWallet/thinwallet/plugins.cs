using System.Windows;
using System.Windows.Controls;
using thinWallet.dapp_plat;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace thinWallet
{
    /// <summary>
    /// Window_thinwallet.xaml 的交互逻辑
    /// </summary>
    public partial class Window_thinwallet : Window
    {
        DApp_Plat dapp_plat = new DApp_Plat();
        bool dapp_Init = false;
        public class DappValue
        {
            public string value;
            public bool error = false;
        }
        Dictionary<string, DappValue> dapp_values = new Dictionary<string, DappValue>();
        //change dapp function
        private void dappfuncs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var plugin = dappfuncs.Tag as DApp_SimplePlugin;
            var func = (dappfuncs.SelectedItem as TabItem).Tag as DApp_Func;
            var items = (((dappfuncs.SelectedItem as TabItem).Content as ScrollViewer).Content as Canvas).Children;
            foreach (var input in func.inputs)
            {
                if (string.IsNullOrEmpty(input.id) == false)
                {
                    if (dapp_values.ContainsKey(input.id) == false)
                    {
                        dapp_values[input.id] = new DappValue();
                        dapp_values[input.id].value = new MyJson.JsonNode_ValueString(input.value).ToString();
                        dapp_values[input.id].error = false;
                    }
                }
            }
            foreach (FrameworkElement ui in items)
            {
                if (ui.Tag is DApp_Input)
                {
                    var input = ui.Tag as DApp_Input;
                    try
                    {
                        var value = dapp_getValue(ui, input.type);
                        this.dapp_values[input.id].value = value;
                        this.dapp_values[input.id].error = false;
                    }
                    catch (Exception err)
                    {
                        this.dapp_values[input.id].value = err.Message;
                        this.dapp_values[input.id].error = true;
                    }
                }
            }
            if (func.call.type == DApp_Call.Type.sendrawtransaction)
            {
                btnMakeTran.Visibility = Visibility.Visible;
            }
            else
            {
                btnMakeTran.Visibility = Visibility.Hidden;
            }
            dapp_updateValuesUI();
        }

        //UI execute pressed
        private void Execute_Dapp_Function(object sender, RoutedEventArgs e)
        {
            var plugin = dappfuncs.Tag as DApp_SimplePlugin;
            if (plugin == null)
                return;
            var func = (dappfuncs.SelectedItem as TabItem).Tag as DApp_Func;
            if (func.call.type == DApp_Call.Type.getstorage)
            {
                dapp_getStorage(func);
            }
            else if (func.call.type == DApp_Call.Type.invokescript)
            {
                dapp_invokeScript(func);
            }
            else if (func.call.type == DApp_Call.Type.sendrawtransaction)
            {
                dapp_sendrawtransaction(func);
            }
        }
        private void Execute_Dapp_Function_GenOnly(object sender, RoutedEventArgs e)
        {
            var plugin = dappfuncs.Tag as DApp_SimplePlugin;
            if (plugin == null)
                return;
            var func = (dappfuncs.SelectedItem as TabItem).Tag as DApp_Func;
            if (func.call.type == DApp_Call.Type.sendrawtransaction)
            {
                dapp_sendrawtransaction(func, true);
                tabMain.SelectedIndex = 0;
            }

        }
        private void dapp_getStorage(DApp_Func func)
        {
            try
            {
                var json = func.call.scriptparam;
                var scripthash = dapp_getCallParam(json[0].AsString());
                var key = dapp_getCallParam(json[1].AsString());
                var result = rpc_getStorage(scripthash, key);
                if (result == null)
                    this.dapp_result_raw.Text = "(null)";
                else
                    this.dapp_result_raw.Text = ThinNeo.Helper.Bytes2HexString(result);

                this.dapp_result.Items.Clear();
                if (func.results.Length > 0)
                {
                    var outvalue = "";
                    try
                    {
                        outvalue = dapp_getResultValue(func.results[0].type, result);
                    }
                    catch (Exception err)
                    {
                        outvalue = "err:" + err.Message;
                    }
                    this.dapp_result.Items.Add(func.results[0].desc + "=" + outvalue);
                }
            }
            catch (Exception err)
            {
                this.dapp_result_raw.Text = "error=" + err.Message + "\r\n" + err.StackTrace;
            }
        }
        void dapp_EmitParam(ThinNeo.ScriptBuilder sb, MyJson.IJsonNode param)
        {
            if (param is MyJson.JsonNode_ValueNumber)//bool 或小整数
            {
                sb.EmitParamJson(param);
            }
            else if (param is MyJson.JsonNode_Array)
            {
                var list = param.AsList();
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    dapp_EmitParam(sb, list[i]);
                }
                sb.EmitPushNumber(param.AsList().Count);
                sb.Emit(ThinNeo.VM.OpCode.PACK);
            }
            else if (param is MyJson.JsonNode_ValueString)//复杂格式
            {
                var str = param.AsString();
                var bytes = dapp_getCallParam(str);
                sb.EmitPushBytes(bytes);
            }
            else
            {
                throw new Exception("should not pass a {}");
            }
        }
        private void dapp_invokeScript(DApp_Func func)
        {
            try
            {
                var hash = dapp_getCallParam(func.call.scriptcall);
                var scrb = new ThinNeo.ScriptBuilder();
                var jsonps = func.call.scriptparam;
                for (var i = jsonps.Length - 1; i >= 0; i--)
                {
                    dapp_EmitParam(scrb, jsonps[i]);
                }
                scrb.EmitAppCall(hash);


                var callstr = ThinNeo.Helper.Bytes2HexString(scrb.ToArray());
                var str = WWW.MakeRpcUrl(labelRPC.Text, "invokescript", new MyJson.JsonNode_ValueString(callstr));
                var result = WWW.GetWithDialog(this, str);

                this.dapp_result.Items.Clear();


                if (result == null)
                    this.dapp_result_raw.Text = "(null)";
                else
                {
                    var json = MyJson.Parse(result).AsDict();
                    if (json.ContainsKey("error"))
                    {
                        this.dapp_result_raw.Text = json["error"].ToString();

                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        json["result"].AsDict().ConvertToStringWithFormat(sb, 4);
                        this.dapp_result_raw.Text = sb.ToString();
                        var gas = json["result"].AsDict()["gas_consumed"].ToString();
                        this.dapp_result.Items.Add("Fee:" + gas);
                        var state = json["result"].AsDict()["state"].ToString();
                        this.dapp_result.Items.Add("State:" + state);
                        var stack = json["result"].AsDict()["stack"].AsList();
                        this.dapp_result.Items.Add("StackCount=" + stack.Count);
                        foreach (var s in stack)
                        {
                            this.dapp_result.Items.Add(s.ToString());
                        }

                    }
                }


            }
            catch (Exception err)
            {
                this.dapp_result_raw.Text = "error=" + err.Message + "\r\n" + err.StackTrace;
            }
        }

        private void dapp_sendrawtransaction(DApp_Func func, bool onlyMakeTran = false)
        {
            try
            {
                dapp_result.Items.Clear();
                dapp_result_raw.Text = "";
                //fill script
                if (string.IsNullOrEmpty(func.call.scriptcall))
                {
                    this.lastScript = null;
                    this.tabCType.SelectedIndex = 0;
                    this.updateScript();
                    lastFee = 0;
                    labelFee.Text = "Fee:" + lastFee;
                }
                else
                {
                    var hash = dapp_getCallParam(func.call.scriptcall);
                    var scrb = new ThinNeo.ScriptBuilder();
                    var jsonps = func.call.scriptparam;
                    for (var i = jsonps.Length - 1; i >= 0; i--)
                    {
                        dapp_EmitParam(scrb, jsonps[i]);
                    }
                    scrb.EmitAppCall(hash);
                    this.lastScript = scrb.ToArray();
                    this.tabCType.SelectedIndex = 1;
                    this.updateScript();
                    lastFee = (decimal)func.call.scriptfee;
                    labelFee.Text = "Fee:" + lastFee;
                }
                //fill input
                this.listInput.Items.Clear();
                foreach (var coin in func.call.coins)
                {
                    var hash = dapp_getCallParam(coin.scripthash);
                    var value = coin.value;
                    tx_fillInputs(hash, coin.asset, value);
                }
                //fill output
                this.updateOutput();

                //生成交易,拼签名
                var tran = this.GenTran();
                if (tran == null)
                    return;
                this.lastTranMessage = tran.GetMessage();

                //处理鉴证
                foreach (var coin in func.call.witnesses)
                {
                    byte[] vscript = dapp_getCallParam(coin.vscript);
                    var hash = ThinNeo.Helper.GetScriptHashFromScript(vscript);
                    var addr = ThinNeo.Helper.GetAddressFromScriptHash(hash);
                    ThinNeo.Witness wit = null;
                    foreach (ThinNeo.Witness w in listWitness.Items)
                    {
                        if (w.Address == addr)
                        {
                            wit = w;
                            break;
                        }
                    }
                    if (wit == null)
                    {
                        wit = new ThinNeo.Witness();
                        wit.VerificationScript = vscript;
                        listWitness.Items.Add(wit);
                    }
                    ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();
                    for (var i = coin.iscript.Length - 1; i >= 0; i--)
                    {
                        dapp_EmitParam(sb, coin.iscript[i]);
                    }
                    wit.InvocationScript = sb.ToArray();

                }
                if (onlyMakeTran)
                {
                    return;
                }
                var ttran = this.signAndBroadcast();
                if (tran != null)
                {
                    this.dapp_result_raw.Text = "sendtran:" + ThinNeo.Helper.Bytes2HexString(ttran.GetHash());
                }
            }
            catch (Exception err)
            {
                this.dapp_result_raw.Text = "error=" + err.Message + "\r\n" + err.StackTrace;
            }
        }
        void tx_fillInputs(byte[] hash, string asset, ThinNeo.Fixed8 count)
        {
            var assetid = "";
            foreach (var item in Tools.CoinTool.assetUTXO)
            {
                if (item.Value == asset || item.Key == asset)
                {
                    assetid = item.Key;
                    break;
                }
            }
            var address = ThinNeo.Helper.GetAddressFromScriptHash(hash);
            var thispk = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
            var thisaddr = ThinNeo.Helper.GetAddressFromPublicKey(thispk);
            if (address == thisaddr)
            {
                var b = tx_fillThis(assetid, count);
                if (b == false)
                    throw new Exception("not have enough coin.");
            }


        }
        bool tx_fillThis(string assetid, ThinNeo.Fixed8 count)
        {
            if (assetid == "")
                throw new Exception("refresh utxo first.");

            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
            var hash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            foreach (var coins in this.myasset.allcoins)
            {
                if (coins.AssetID == assetid)
                {
                    coins.coins.Sort((a, b) =>
                    {
                        return Math.Sign(a.value - b.value);
                    });
                    decimal want = count;
                    decimal inputv = 0;
                    foreach (var c in coins.coins)
                    {
                        Tools.Input input = new Tools.Input();
                        input.Coin = c;
                        input.From = hash;
                        this.listInput.Items.Add(input);
                        inputv += c.value;
                        if (inputv > want)
                            break;
                    }
                    if (inputv < want)
                        return false;
                    else
                        return true;
                }
            }
            return false;
        }
        byte[] rpc_getStorage(byte[] scripthash, byte[] key)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var url = this.labelRPC.Text;
            var shstr = ThinNeo.Helper.Bytes2HexString(scripthash.Reverse().ToArray());
            var keystr = ThinNeo.Helper.Bytes2HexString(key);
            var str = WWW.MakeRpcUrl(url, "getstorage", new MyJson.JsonNode_ValueString(shstr), new MyJson.JsonNode_ValueString(keystr));
            var result = WWW.GetWithDialog(this, str);
            if (result != null)
            {

                var json = MyJson.Parse(result);
                if (json.AsDict().ContainsKey("error"))
                    return null;
                var script = json.AsDict()["result"].AsString();
                return ThinNeo.Helper.HexString2Bytes(script);
            }
            return null;
        }

        byte[] dapp_getCallParam(string text)
        {
            var str = text;
            if (text.Contains("%"))
            {
                var ss = text.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length != 2)
                {
                    throw new Exception("not a vaild text:" + text);
                }

                str = ss[0] + dapp_getRefValues(ss[1]);
            }
            var bytes = ThinNeo.ScriptBuilder.GetParamBytes(str);
            return bytes;
        }
        string dapp_getRefValues(string info)
        {
            var plugin = dappfuncs.Tag as DApp_SimplePlugin;
            var func = (dappfuncs.SelectedItem as TabItem).Tag as DApp_Func;

            info = info.Replace(" ", "");
            var pointstr = info.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (pointstr[0] == "consts")
            {
                if (plugin.consts.ContainsKey(pointstr[1]))
                {
                    return plugin.consts[pointstr[1]];
                }
                else
                {
                    throw new Exception("not have const:" + info);
                }
            }
            else if (pointstr[0] == "inputs")
            {
                if (this.dapp_values.ContainsKey(pointstr[1]))
                {
                    if (this.dapp_values[pointstr[1]].error == false)
                    {
                        return this.dapp_values[pointstr[1]].value;
                    }
                    else
                    {
                        throw new Exception("value is in error:" + info);
                    }
                }
                else
                {
                    throw new Exception("not have inputs:" + info);
                }
            }
            else if (pointstr[0] == "keyinfo")
            {
                if (this.privatekey == null)
                {
                    throw new Exception("not load key.");
                }
                if (pointstr[1] == "pubkey")
                {
                    var pkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
                    return ThinNeo.Helper.Bytes2HexString(pkey);
                }
                else if (pointstr[1] == "script")
                {
                    var pkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
                    var hs = ThinNeo.Helper.GetScriptFromPublicKey(pkey);
                    return ThinNeo.Helper.Bytes2HexString(hs);
                }
                else if (pointstr[1] == "scripthash")
                {
                    var pkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(this.privatekey);
                    var hs = ThinNeo.Helper.GetScriptHashFromPublicKey(pkey);
                    return ThinNeo.Helper.Bytes2HexString(hs);
                }
                else if (pointstr[1] == "signdata")
                {
                    var data = ThinNeo.Helper.Sign(this.lastTranMessage, this.privatekey);
                    return ThinNeo.Helper.Bytes2HexString(data);

                }
            }

            throw new Exception("not support it:" + info);

        }
        string dapp_getResultValue(string type, byte[] result)
        {
            if (type == "string")
            {
                return System.Text.Encoding.UTF8.GetString(result);
            }
            throw new Exception("not support.");
        }
        string dapp_getValue(FrameworkElement ui, string type)
        {
            if (type == "string" || type == "str")
            {
                if ((ui is TextBlock))
                    return (ui as TextBlock).Text;
                if ((ui is TextBox))
                    return (ui as TextBox).Text;
            }
            else if (type == "address")
            {
                var str = "";
                if ((ui is TextBlock))
                    str = (ui as TextBlock).Text;
                else if ((ui is TextBox))
                    str = (ui as TextBox).Text;
                else
                    throw new Exception("not support");
                var hash = ThinNeo.Helper.GetPublicKeyHashFromAddress(str);
                return ThinNeo.Helper.GetAddressFromScriptHash(hash);
            }
            else
            {
                throw new Exception("not support type");
            }
            throw new Exception("not parsed value");
        }
        void dapp_updateValuesUI()
        {
            this.listDappValue.Items.Clear();
            foreach (var v in dapp_values)
            {
                var item = new ListBoxItem();
                item.Content = v.Key + "=" + v.Value.value;
                if (v.Value.error)
                {
                    item.Foreground = red;
                }
                this.listDappValue.Items.Add(item);
            }
        }

        void dapp_UpdatePluginsUI()
        {
            if (!dapp_Init)
            {
                dapp_Init = true;
                this.comboDApp.SelectionChanged += (s, e) =>
                  {
                      dapp_values.Clear();
                      var plugin = (this.comboDApp.SelectedItem as ComboBoxItem).Content as DApp_SimplePlugin;
                      if (plugin != null)
                      {
                          dappfuncs.Items.Clear();
                          dappfuncs.Tag = plugin;
                          foreach (var f in plugin.funcs)
                          {
                              var tabItem = new TabItem();
                              tabItem.Header = f.name;
                              tabItem.Tag = f;
                              dappfuncs.Items.Add(tabItem);
                              dapp_UpdateFuncUI(tabItem, f);
                          }
                      }
                  };
            }
            foreach (var m in dapp_plat.plugins)
            {
                var item = new ComboBoxItem();
                item.Content = m;
                this.comboDApp.Items.Add(item);
            }
        }
        System.Windows.Media.SolidColorBrush white
        {
            get
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));

            }
        }
        System.Windows.Media.SolidColorBrush red
        {
            get
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));

            }
        }
        void dapp_UpdateFuncUI(TabItem tabitem, DApp_Func func)
        {
            var sviewer = new ScrollViewer();
            tabitem.Content = sviewer;
            var canvas = new Canvas();
            sviewer.Content = canvas;
            canvas.Background = null;

            var text = new TextBlock();
            text.Width = 500;
            text.Height = 32;
            canvas.Children.Add(text);
            Canvas.SetLeft(text, 0);
            Canvas.SetTop(text, 0);
            text.Text = func.desc;
            text.Foreground = white;

            var y = text.Height;
            foreach (var i in func.inputs)
            {
                var label = new TextBlock();
                label.Text = i.desc;
                label.Width = 200;
                label.Height = 32;
                canvas.Children.Add(label);
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, y);

                if (i.type == "string" || i.type == "address")
                {
                    TextBox tbox = new TextBox();
                    tbox.Tag = i;
                    tbox.Width = 300;
                    tbox.Height = 20;
                    tbox.Text = i.value;
                    canvas.Children.Add(tbox);
                    tbox.TextChanged += dapp_FuncValue_Text_Changed;
                    Canvas.SetLeft(tbox, 200);
                    Canvas.SetTop(tbox, y);
                    y += 20;
                }
            }
        }
        void dapp_FuncValue_Text_Changed(object sender, TextChangedEventArgs e)
        {
            var input = (sender as FrameworkElement).Tag as DApp_Input;
            try
            {
                var value = dapp_getValue(sender as FrameworkElement, input.type);
                this.dapp_values[input.id].value = value;
                this.dapp_values[input.id].error = false;
            }
            catch (Exception err)
            {
                this.dapp_values[input.id].value = err.Message;
                this.dapp_values[input.id].error = true;
            }
            dapp_updateValuesUI();
        }
    }
}