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
using System.Net;

namespace RemoteTaskManager_for_WPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private tcp_conection tcp;
        private performance per;
        bool tick_switch = false;

        int port = 0;

        private async void runlisten() //listenを非同期で開始するメソッド
        {
            await Task.Run(() => listen());
        }

        private void listen()
        {
            status_label.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        status_label.Content = "サーバ待機";
                        statusbar.Background = getBrushColor(202, 81, 0, 255);
                        BorderBrush = getBrushColor(202, 81, 0, 255);
                    })
                );

            tcp.listen();
            string str;
            str = string.Format("{0},{1},0", per.cpu_count(), per.max_mem());
            System.Console.WriteLine(str);

            tcp.send(str);

            bool ok = false;

            while (!ok)
            {
                if (tcp.recive() == "OK")
                {
                    ok = true;
                    tcp.connection = true;  //tcp.connectionを違うインスタンスからいじるのはよくない気がする、どうにかしたいね
                    System.Console.WriteLine("準備完了");
                }
            }

            status_label.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        status_label.Content = "接続完了";
                        statusbar.Background = getBrushColor(104, 33, 122, 255);
                        BorderBrush = getBrushColor(104, 33, 122, 255);
                        client_label.Content = tcp.get_client_IP();
                        client_label.Foreground = getBrushColor(45,133,193,255);
                    })
                );
        }

        private void tick()
        {
            while(tick_switch)
            {
                int mem = per.get_use_mem();
                int cpu = per.get_all_cpu();

                CPU_label.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            CPU_label.Content = string.Format("{0}%",cpu);
                            mem_label.Content = string.Format("{0}MB", mem);
                        })
                        );

                if (tcp != null)
                {
                    if (tcp.connection) //クライアントが接続していたら
                    {
                        string sendstr;
                        //tcp.send(sendstr);
                        //System.Console.WriteLine("send:{0}",sendstr);

                        sendstr = string.Format("{0},{1}", mem, cpu);
                        //sendstr = string.Format("{0},{1}", mem, 90);
                        for (int i = 0; i < per.cpu_count(); i++)
                        {
                            string str = string.Format(",{0}", per.get_thread_cpu(i));
                            sendstr += str;
                        }

                        sendstr += ",0";
                        int result = tcp.send(sendstr);
                        System.Console.WriteLine("send:{0}", sendstr);

                        if (result == 101)
                        {
                            tcp.disconnection();
                            
                            /*
                            start_server.Dispatcher.BeginInvoke(
                                 new Action(() =>
                                 {
                                     start_server.IsEnabled = true;
                                     status_label.Content = "未接続";
                                     statusbar.Background = getBrushColor(45, 133, 193, 255);
                                     BorderBrush = getBrushColor(45,133,193, 255);
                                 })
                                );
                            */ 
                            //↑はクライアントが切断したらまたサーバ開始ボタン押さないといけない

                            //↓はクライアントが切断したらlistenを自動的に開始して何もしなくてもクライアントが接続できるようにしてる
                            
                            status_label.Dispatcher.BeginInvoke(
                                new Action(()=>
                                {
                                    status_label.Content = "サーバ待機";
                                    client_label.Foreground = getBrushColor(255,123,123,255);
                                    client_label.Content = "クライアントなし";

                                    //statusbar.Background = getBrushColor(202, 81, 0, 255);
                                    //BorderBrush = getBrushColor(202, 81, 0, 255);
                                })
                                );
                            tcp = new tcp_conection("0,0,0,0", port);
                            runlisten();　//await Task.Run(() => listen());ってしたかった
                            

                        }
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private async void start_tick() //tickメソッドを非同期で開始するメソッド
        {
            await Task.Run(() => tick());
        }

        private Brush getBrushColor(byte R,byte G,byte B,byte A)    //色
        {
            Color color = Color.FromArgb(A,R,G,B);
            Brush output = new SolidColorBrush(color);　//Color型の色を作ってBrushに変換するという方法しか思いつかなかった。もっといい方法だれか教えて。

            return output;
        }

        private string get_ip() //IPアドレス取得
        {
            string ipaddress = "";
            IPHostEntry ipentry = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipaddress = ip.ToString();
                    break;
                }
            }
            return ipaddress;
        }

        //この下からイベント

        public MainWindow()
        {
            InitializeComponent();
            per = new performance();
            max_memlabel.Content = string.Format("{0}MB", per.max_mem());
            IP_label.Content = string.Format("IPアドレス:{0}", get_ip());

            tick_switch = true;
            start_tick();
        }

        private async void start_server_Click(object sender, RoutedEventArgs e) //サーバ開始ボタン
        {
            start_server.IsEnabled = false;

            /*
            if(tcp == null)
            {
                int port = int.Parse(port_textbox.Text);
                tcp = new tcp_conection("0,0,0,0", port);
                status_label.Content = "サーバ待機";
                await Task.Run(() => listen());

            }
            */

            port = int.Parse(port_textbox.Text);    //数字以外の数が入ってきたら例外になるので例外処理書かないと
            tcp = new tcp_conection("0,0,0,0", port);   //サーバ側なので第一引数はどうでもいい
            status_label.Content = "サーバ待機";
            port_textbox.IsEnabled = false;
            await Task.Run(() => listen()); //runlisten()メソッドでもいいのでは？
        }

        private void closebutton_Click(object sender, RoutedEventArgs e)
        {
            if(tcp != null)
            {
                //tcp.disconnection();
            }
            this.Close();
        }

        private void disconnect_button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
