using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
namespace CrawlerPWebServer
{
    public partial class Form1 : Form
    {
        IPAddress[] adrList;

        public Form1()
        {
            InitializeComponent();
            // スレッドからテキストボックスをアクセスすることを指定
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        //============
        // メンバー変数
        private System.Net.Sockets.TcpListener server;  // リスナー(接続待ちや受信等を行なうｵﾌﾞｼﾞｪｸﾄ)
        private System.Threading.Thread ListeningCallbackThread;   // 接続待ちスレッド
        private volatile bool SLTAlive;  // 接続待ちスレッド終了指示フラグ(volatile が指定されていることに注意)

        //============
        // フォーム起動時イベント
        private void Form1_Load(object sender, EventArgs e)
        {
            // スレッド終了指示フラグを未終了に初期化
            SLTAlive = false;
        }

        //============
        // フォーム閉鎖時イベント
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // ｻｰﾊﾞｱﾌﾟﾘを終了するにもかかわらず、接続待ちｽﾚｯﾄﾞを終了していない場合の処理
            if (SLTAlive)
            {
                // スレッド終了指示フラグを終了に設定
                SLTAlive = false;
                // 接続要求受け入れの終了
                server.Stop();
                // 念のためスレッドをnull設定
                ListeningCallbackThread = null;
            }

        }

        //============
        // 接続待ち開始ボタンのクリックイベント
        private void button1_Click(object sender, EventArgs e)
        {
            if (!SLTAlive)  // まだ接続待ちｽﾚｯﾄﾞを生成していない場合
            {
                // 接続待ち用スレッドを作成
                ListeningCallbackThread = new System.Threading.Thread(ListeningCallback);
                // 接続待ち用スレッドを開始
                ListeningCallbackThread.Start();
                // スレッド終了指示フラグを未終了に設定
                SLTAlive = true;

                // 追加
                string hostname = Dns.GetHostName();
                // ホスト名からIPアドレスを取得する
                adrList = Dns.GetHostAddresses(hostname);
            }
        }

        //============
        // 接続待ち終了ボタンのクリックイベント
        private void button2_Click(object sender, EventArgs e)
        {
            if (SLTAlive)  // 接続待ちｽﾚｯﾄﾞが作成されていて使える場合
            {
                if (server != null)
                {
                    // 接続要求受け入れの終了
                    server.Stop();
                }
                // スレッド終了指示フラグを終了に設定
                SLTAlive = false;
                richTextBox1.AppendText("サーバー終了" + "\n");
            }
        }

        //============
        // 接続待ちスレッド用メソッド
        private void ListeningCallback()
        {
            // リスナー(接続要求受け入れ待機)を生成
            server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse("172.16.30.215"), 9000);
            // *** server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, 9000);
            // 接続要求受け入れ開始
            server.Start();
            richTextBox1.AppendText("サーバー開始" + "\n");
            try
            {
                // 受信の受付を行なうための無限ループ
                while (SLTAlive)    // ｽﾚｯﾄﾞ終了指示ﾌﾗｸﾞでの終了指示がある場合はﾙｰﾌﾟ終了
                {
                    // 受信接続キュー内で、接続待ちがあるか判断
                    if (server.Pending() == true)
                    {
                        // クライアントからの接続を受け付ける
                        System.Net.Sockets.TcpClient ClientSocket = server.AcceptTcpClient(); // TCPｸﾗｲｱﾝﾄ
                        // 通信ストリームの取得
                        System.Net.Sockets.NetworkStream stream = ClientSocket.GetStream();
                        // クライアントからの電文の受信
                        byte[] ReceiveData = new byte[2000];
                        int DataLength = stream.Read(ReceiveData, 0, ReceiveData.Length);   // 電文の列長
                        string str = Encoding.Unicode.GetString(ReceiveData, 0, DataLength);
                        richTextBox1.AppendText(str+"\n");    // 受信データ
                        // 返信電文をクライアントへ送信
                        byte[] SendBuffer = Encoding.Unicode.GetBytes("本ｻｰﾊﾞｰの御利用ありがとう御座います。");
                        stream.Write(SendBuffer, 0, SendBuffer.Length);
                        stream.Flush(); // フラッシュ(強制書き出し)
                        // TCPｸﾗｲｱﾝﾄをｸﾛｰｽﾞ
                        ClientSocket.Close();
                    }
                    // 短時間だけ待機
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("サーバー終了" + "\n");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SLTAlive)  // 接続待ちｽﾚｯﾄﾞが作成されていて使える場合
            {
                if (server != null)
                {
                    // 接続要求受け入れの終了
                    server.Stop();
                }
                // スレッド終了指示フラグを終了に設定
                SLTAlive = false;
            }
        }



    }

}