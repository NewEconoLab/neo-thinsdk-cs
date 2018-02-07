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

namespace thinWallet
{
    /// <summary>
    /// Dialog_Input_password.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Witness_Edit : Window
    {
        public Dialog_Witness_Edit()
        {
            InitializeComponent();
        }
        public byte[] vscript;
        public byte[] iscript;
        void update()
        {
            this.textASM.Text = ThinNeo.Helper.Bytes2HexString(this.vscript);
            var scripthash = ThinNeo.Helper.GetScriptHashFromScript(this.vscript);
            this.info1.Text = ThinNeo.Helper.Bytes2HexString(scripthash);
            this.info2.Text = ThinNeo.Helper.GetAddressFromScriptHash(scripthash);
            MyJson.JsonNode_Array arr = new MyJson.JsonNode_Array();
            arr.AddArrayValue("(str)0214");
            arr.Add(new MyJson.JsonNode_Array());
            var sb = new StringBuilder();
            arr.ConvertToStringWithFormat(sb, 4);
            this.jsonParam.Text = sb.ToString();

            if (this.iscript != null)
            {
                this.asmList.Items.Clear();
                var ops = ThinNeo.Compiler.Avm2Asm.Trans(this.iscript);
                for (int i = 0; i < ops.Length; i++)
                {
                    this.asmList.Items.Add(ops[i]);
                }
                this.asmBinText.Text = ThinNeo.Helper.Bytes2HexString(this.iscript);
            }
        }
        public static byte[] ShowDialog(Window owner, ThinNeo.Witness witness)
        {
            var d = new Dialog_Witness_Edit();
            d.Owner = owner;
            d.vscript = witness.VerificationScript;
            d.iscript = witness.InvocationScript;
            d.update();
            if (d.ShowDialog() == true)
            {
                return d.iscript;
            }
            return null;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void textASM_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var json = MyJson.Parse(jsonParam.Text).AsList();
                jsonParam.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();
                var list = json.AsList();
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    sb.EmitParamJson(list[i]);
                }
                this.iscript = sb.ToArray();
                this.asmList.Items.Clear();
                var ops = ThinNeo.Compiler.Avm2Asm.Trans(this.iscript);
                for (int i = 0; i < ops.Length; i++)
                {
                    this.asmList.Items.Add(ops[i]);
                }
                this.asmBinText.Text = ThinNeo.Helper.Bytes2HexString(this.iscript);
            }
            catch
            {
                jsonParam.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }
    }
}
