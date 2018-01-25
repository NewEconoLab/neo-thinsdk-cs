using System.Windows;
using System.Windows.Controls;
using thinWallet.dapp_plat;

namespace thinWallet
{
    /// <summary>
    /// Window_thinwallet.xaml 的交互逻辑
    /// </summary>
    public partial class Window_thinwallet : Window
    {
        DApp_Plat DApp_Plat = new DApp_Plat();

        void UpdatePlugins()
        {
            foreach (var m in DApp_Plat.plugins)
            {
                var item = new System.Windows.Controls.TabItem();
                item.Header = m.Title;

                item.Tag = m;
                this.dappTab.Items.Add(item);

                var tab = new TabControl();
                item.Content = tab;
                tab.Background = null;

                foreach (var f in m.funcs)
                {
                    var tabItem = new TabItem();
                    tabItem.Header = f.name;
                    tab.Items.Add(tabItem);
                    UpdateFuncUI(tabItem, f);
                }


                //var text = new TextBlock();
                //text.Text= m.d
            }
        }
        System.Windows.Media.SolidColorBrush white
        {
            get
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));

            }
        }
        void UpdateFuncUI(TabItem tabitem, DApp_Func func)
        {
            var sviewer = new ScrollViewer();
            tabitem.Content = sviewer;
            var canvas = new Canvas();
            sviewer.Content = canvas;
            canvas.Background = null;

            var text = new TextBlock();
            text.Width = 500;
            text.Height = 100;
            canvas.Children.Add(text);
            Canvas.SetLeft(text, 0);
            Canvas.SetTop(text, 0);
            text.Text = func.desc;
            text.Foreground = white;

            var y = text.Height;
            foreach(var i in func.inputs)
            {
                var label = new TextBlock();
                label.Text = i.desc;
                label.Width = 200;
                label.Height = 32;
                canvas.Children.Add(label);
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, y);

                if (i.type=="string"||i.type=="address")
                {
                    TextBox tbox = new TextBox();
                    tbox.Width = 300;
                    tbox.Height = 20;
                    tbox.Text = i.value;
                    canvas.Children.Add(tbox);
                    Canvas.SetLeft(tbox, 200);
                    Canvas.SetTop(tbox, y);
                    y += 20;
                }
            }
        }
    }
}