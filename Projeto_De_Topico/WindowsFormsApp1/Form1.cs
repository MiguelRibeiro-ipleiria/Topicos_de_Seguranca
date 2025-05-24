using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EI.SI;

using Guna.UI2.WinForms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        bool lateraloff = true;
        private Form2 form2Ref;

        private const int PORT = 10000;
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        TcpClient client;

        public Form1(Form2 form2)
        {
            InitializeComponent();
            lateraloff = true;
            lateral_control();
            form2Ref = form2;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            client = new TcpClient();
            client.Connect(endPoint);
            networkStream = client.GetStream();
            protocolSI = new ProtocolSI();
        }

        public void lateral_control()
        {
            if (lateraloff == true)
            {
                panel_lateral.Size = new Size(70, 403);
                panel_lateral.Location = new Point(-2, 52);

                guna2CustomGradientPanel2.Size = new Size(684, 57);
                guna2CustomGradientPanel2.Location = new Point(66, 49);

                label_nome_cliente.Location = new Point(522, 22);
                pictureBox2.Location = new Point(616, 7);

                guna2CustomGradientPanel1.Size = new Size(602, 37);
                guna2CustomGradientPanel1.Location = new Point(74, 404);

                guna2Button2.Text = "";
                guna2Button2.Image = WindowsFormsApp1.Properties.Resources.icons8_logout_48;
                guna2Button2.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleCenter;
                guna2Button2.ImageSize = new Size(40, 40);

                guna2Button3.Text = "";
                guna2Button3.Image = WindowsFormsApp1.Properties.Resources.connect;
                guna2Button2.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleCenter;
                guna2Button3.ImageSize = new Size(40, 40);

                guna2Button4.Text = "";
                guna2Button4.Image = WindowsFormsApp1.Properties.Resources.chatting;
                guna2Button4.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleCenter;
                guna2Button4.ImageSize = new Size(40, 40);

                guna2Button2.Size = new Size(41, 40);
                guna2Button2.Location = new Point(14, 349);

                guna2Button3.Size = new Size(41, 40);
                guna2Button3.Location = new Point(14, 29);

                guna2Button4.Size = new Size(41, 40);
                guna2Button4.Location = new Point(14, 90);

                lateraloff = false;
            }
            else
            {
                panel_lateral.Size = new Size(191, 403);
                panel_lateral.Location = new Point(-2, 52);

                guna2CustomGradientPanel2.Size = new Size(561, 57);
                guna2CustomGradientPanel2.Location = new Point(189, 49);

                label_nome_cliente.Location = new Point(410, 22);
                pictureBox2.Location = new Point(504, 8);

                guna2CustomGradientPanel1.Size = new Size(481, 37);
                guna2CustomGradientPanel1.Location = new Point(195, 404);

                guna2Button2.Text = "   Logout";
                guna2Button2.Image = WindowsFormsApp1.Properties.Resources.icons8_logout_48;
                guna2Button2.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleLeft;
                guna2Button2.TextAlign = (HorizontalAlignment)ContentAlignment.MiddleRight;
                guna2Button2.ImageSize = new Size(40, 40);

                guna2Button3.Text = "    Connection";
                guna2Button3.Image = WindowsFormsApp1.Properties.Resources.connect;
                guna2Button3.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleLeft;
                guna2Button3.TextAlign = (HorizontalAlignment)ContentAlignment.MiddleRight;
                guna2Button3.ImageSize = new Size(39, 39);

                guna2Button4.Text = "     Cifrar";
                guna2Button4.Image = WindowsFormsApp1.Properties.Resources.chatting;
                guna2Button4.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleLeft;
                guna2Button4.TextAlign = (HorizontalAlignment)ContentAlignment.MiddleRight;
                guna2Button4.ImageSize = new Size(39, 39);

                guna2Button2.Size = new Size(146, 40);
                guna2Button2.Location = new Point(14, 349);

                guna2Button3.Size = new Size(155, 40);
                guna2Button3.Location = new Point(14, 29);

                guna2Button4.Size = new Size(155, 40);
                guna2Button4.Location = new Point(14, 90);

                lateraloff = true;
            }
        }


        private void pictureBox3_Click(object sender, EventArgs e)
        {
            lateral_control();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CloseClient();
            Form2 novoForm = new Form2();
            novoForm.Show();
            Close();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            CloseClient();
            Form2 novoForm = new Form2();
            novoForm.Show();
            Close();
        }

        private void guna2ButtonEnviar_Click(object sender, EventArgs e)
        {
            string msg = textboxchat.Text;
            textboxchat.Clear();
            byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, msg);
            networkStream.Write(packet, 0, packet.Length);

            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                if (bytesRead > 0)
                {
                    if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
                    {
                        string resposta = protocolSI.GetStringFromData();
                        textBoxInformacao.AppendText(resposta + Environment.NewLine);
                    }
                }


            }

        }

        private void CloseClient()
        {
            //Vou enviar o EOT para o servidor
            byte[] EOT = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(EOT, 0, EOT.Length);
            //LER O ACK
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            networkStream.Close();
            client.Close();
        }
    }
}
