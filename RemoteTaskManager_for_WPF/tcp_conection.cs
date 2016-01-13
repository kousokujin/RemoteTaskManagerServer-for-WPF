using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager_for_WPF
{
    class tcp_conection
    {
        string IP_add;  //相手のIPアドレス
        int port;       //相手のポート番号
        public bool connection = false;

        System.Net.Sockets.TcpListener listener;
        System.Net.Sockets.TcpClient client;
        System.Net.Sockets.NetworkStream ns;

        public tcp_conection(string ip, int p)
        {
            IP_add = ip;
            port = p;
        }

        public void listen()
        {
            //System.Net.IPAddress ip = System.Net.IPAddress.Parse(IP_add);
            listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port);
            System.Console.WriteLine("Port:{0}", port);

            listener.Start();

            System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
            System.Console.WriteLine("connect");

            ns = client.GetStream();
            ns.ReadTimeout = 10000;
            ns.WriteTimeout = 10000;
            System.Console.WriteLine("connect2");

            IP_add = ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            //connection = true;
        }

        public string get_client_IP()
        {
            return IP_add;
        }

        public bool client_coneciton()
        {
            client = new System.Net.Sockets.TcpClient(IP_add, port);
            ns = client.GetStream();
            ns.ReadTimeout = 10000;
            ns.WriteTimeout = 10000;

            return true;
        }

        public int send(string mes)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes(mes + '\n');
            try
            {
                ns.Write(sendBytes, 0, sendBytes.Length);
            }
            catch (System.IO.IOException)
            {
                return 101;
                connection = false;
            }

            return 0;
        }

        public string recive()
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] resBytes = new byte[256];
            int resSize = 0;

            if (ns.DataAvailable)
            {

                do
                {
                    //データの一部を受信する
                    resSize = ns.Read(resBytes, 0, resBytes.Length);

                    //受信したデータを蓄積する
                    ms.Write(resBytes, 0, resSize);

                    // 受信を続ける
                } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');

                //受信したデータを文字列に変換
                string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                ms.Close();
                //末尾の\nを削除
                resMsg = resMsg.TrimEnd('\n');

                return resMsg;
            }
            else
            {
                return "0";
            }
        }

        public void disconnection()
        {
            if (ns != null)
            {
                ns.Close();
            }
            //client.Close();
            if (listener != null)
            {
                listener.Stop();
            }
            connection = false;
        }
    }
}
