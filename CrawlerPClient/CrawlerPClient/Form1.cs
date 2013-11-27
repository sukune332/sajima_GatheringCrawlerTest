using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrawlerPClient
{
    public partial class Form1 : Form
    {
        private NetworkStream stream;
        TcpClient client;
        string ip;
        int port;

        public Form1()
        {
            InitializeComponent();
        }

        //Startボタン
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ip = textBox1.Text;
                port = int.Parse(textBox2.Text);
                client = new TcpClient(ip, port);
                stream = client.GetStream();
                byte[] tmp = Encoding.Unicode.GetBytes(textBox3.Text);
                stream.Write(tmp, 0, tmp.Length);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null)
            {
                client.Close();
            }
        }


    }
}
