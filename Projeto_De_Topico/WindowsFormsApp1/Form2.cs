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

        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;
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

        private static byte[] GenerateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;
        }
        private static byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pass = textBoxPass.Text;
            string username = textBoxUser.Text;

            /*if (VerifyLogin(username, pass))
            {
                MessageBox.Show("USER VÁLIDO");
                this.Hide();
                Form1 form1 = new Form1(this);
                form1.Show();
            }
            else
            {
                MessageBox.Show("AUTENTICAÇÃO ERRADA");
            }*/

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
            string passoword = textBoxPass.Text;
            string username = textBoxUser.Text;

            byte[] packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, username+passoword);
            networkStream.Write(packet, 0, packet.Length);


            byte[] salt = GenerateSalt(SALTSIZE);
            byte[] hash = GenerateSaltedHash(passoword, salt);

            //Register(username, hash, salt);
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
