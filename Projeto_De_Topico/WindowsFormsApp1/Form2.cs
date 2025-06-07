using EI.SI;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private Form1 form1Ref;
        bool hidepass = true;

        private const int PORT = 10000;

        private RSACryptoServiceProvider rsa;
        private AesCryptoServiceProvider aes;


        private string ChaveSimetrica;

        ProtocolSI protocolSI;
        NetworkStream networkStream;
        TcpClient client;


        public Form2()
        {
            InitializeComponent();
            label_ErroLogin.Visible = false;

            rsa = new RSACryptoServiceProvider();
            AesCryptoServiceProvider aes;

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
                    label_ErroLogin.Visible = false;
                }
                else if(protocolSI.GetStringFromData() == "erro")
                {
                    label_ErroLogin.Visible = true;
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

        private string CifrarTexto(string TextoACifrar)
        {
            //Texto ara guardar o texto decifrado em Bytes
            byte[] txtDecifrado = Encoding.UTF8.GetBytes(TextoACifrar);

            //Texto ara guardar o cifrado em bytes
            byte[] txtCifrado;

            //Reservar espaço na memoria para colocar o texto e cifrá-lo
            MemoryStream ms = new MemoryStream();
            //Inicializa o sistema de cifragem (Write)
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            //Cifrar os dados
            cs.Write(txtDecifrado, 0, txtDecifrado.Length);
            cs.Close();

            //Guardar os dados cifrado que estão na memória
            txtCifrado = ms.ToArray();

            //Converter os dados para base64 (texto)
            string txtCifradoB64 = Convert.ToBase64String(txtCifrado);

            //Devolver os bytes em base64
            return txtCifradoB64;

        }

    }
}
