using EI.SI;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private Form1 form1Ref;
        bool hidepass = true;

        private const int PORT = 10000;

        private RSACryptoServiceProvider rsa;

        private string ChaveSimetrica;

        ProtocolSI protocolSI;
        NetworkStream networkStream;
        TcpClient client;


        public Form2()
        {
            InitializeComponent();
            rsa = new RSACryptoServiceProvider();
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, PORT);
            protocolSI = new ProtocolSI();
            client = new TcpClient();
            client.Connect(endpoint);
            networkStream = client.GetStream();

            PublicKey();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pass = textBoxPass.Text;
            string username = textBoxUser.Text;

            byte[] packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, username + "+" + pass);
            networkStream.Write(packet, 0, packet.Length);

            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                if(protocolSI.GetStringFromData() == "validado")
                {
                    MessageBox.Show("Logado Com Sucesso");
                }
                else if(protocolSI.GetStringFromData() == "erro")
                {
                    MessageBox.Show("Errado Com Sucesso");
                }
            }

        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (form1Ref != null)
            {
                form1Ref.Close(); 
            }
            this.Close(); 
            Application.Exit(); 
        }


        public void SetForm1Reference(Form1 form1)
        {
            form1Ref = form1;
        }

        private void HidePass_Click(object sender, EventArgs e)
        {
            if (hidepass == true)
            {
                textBoxPass.PasswordChar = '*';
                hidepass = false;
            }
            else
            {
                textBoxPass.PasswordChar = '\0';
                hidepass = true;
            }
        }

        private void Limpar_Dados_Click(object sender, EventArgs e)
        {
            textBoxPass.Text = string.Empty;
            textBoxUser.Text = string.Empty;
        }

        private void button_registro_Click(object sender, EventArgs e)
        {
            string password = textBoxPass.Text;
            string username = textBoxUser.Text;

            byte[] packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, username + "+" + password);
            networkStream.Write(packet, 0, packet.Length);
        }

        public void PublicKey()
        {
            string publickey = rsa.ToXmlString(false);

            byte[] packet = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, publickey);
            networkStream.Write(packet, 0, packet.Length);

            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                ChaveSimetrica = protocolSI.GetStringFromData();
            }
        }

    }
}
