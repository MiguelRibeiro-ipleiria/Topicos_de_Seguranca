using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, PORT);
            client = new TcpClient();
            client.Connect(endpoint);
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

                ButaoCifrarMenu.Text = "";
                ButaoCifrarMenu.Image = WindowsFormsApp1.Properties.Resources.chatting;
                ButaoCifrarMenu.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleCenter;
                ButaoCifrarMenu.ImageSize = new Size(40, 40);

                guna2Button2.Size = new Size(41, 40);
                guna2Button2.Location = new Point(14, 349);

                guna2Button3.Size = new Size(41, 40);
                guna2Button3.Location = new Point(14, 29);

                ButaoCifrarMenu.Size = new Size(41, 40);
                ButaoCifrarMenu.Location = new Point(14, 90);

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

                ButaoCifrarMenu.Text = "     Cifrar";
                ButaoCifrarMenu.Image = WindowsFormsApp1.Properties.Resources.chatting;
                ButaoCifrarMenu.ImageAlign = (HorizontalAlignment)ContentAlignment.MiddleLeft;
                ButaoCifrarMenu.TextAlign = (HorizontalAlignment)ContentAlignment.MiddleRight;
                ButaoCifrarMenu.ImageSize = new Size(39, 39);

                guna2Button2.Size = new Size(146, 40);
                guna2Button2.Location = new Point(14, 349);

                guna2Button3.Size = new Size(155, 40);
                guna2Button3.Location = new Point(14, 29);

                ButaoCifrarMenu.Size = new Size(155, 40);
                ButaoCifrarMenu.Location = new Point(14, 90);

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

            textBoxInformacao.AppendText("Tu: " + msg + Environment.NewLine);




            //enviar mensagem
            byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, msg);
            networkStream.Write(packet, 0, packet.Length);

            ProtocolSICmdType cmd;
            do
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                cmd = protocolSI.GetCmdType();
            } while (cmd != ProtocolSICmdType.ACK);

            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                string response = protocolSI.GetStringFromData();
                textBoxInformacao.AppendText(response + Environment.NewLine);
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

        private void ButaoCifrarMenu_Click(object sender, EventArgs e)
        {
            var popup = new Form()
            {
                Width = 400,
                Height = 230,
                Text = "Cifragem",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // Label
            var messageLabel = new Label()
            {
                Text = "Indique a sua pública",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            // TextBox
            var txtChavePublica = new TextBox()
            {
                Multiline = true,
                Width = 350,
                Height = 60,
                Font = new Font("Segoe UI", 10),
            };
            txtChavePublica.Location = new Point((popup.ClientSize.Width - txtChavePublica.Width) / 2, messageLabel.Bottom + 10);

            var buttonPanel = new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Location = new Point((popup.ClientSize.Width - 220) / 2, txtChavePublica.Bottom + 10)
            };

            var confirmButton = new Button()
            {
                Text = "Confirmar",
                Width = 90,
                Height = 35,
                DialogResult = DialogResult.OK
            };

            var cancelButton = new Button()
            {
                Text = "Cancelar",
                Width = 90,
                Height = 35,
                DialogResult = DialogResult.Cancel
            };

            if (popup.ShowDialog() == DialogResult.OK)
            {
                //executar função de cifragem
            }

            buttonPanel.Controls.Add(confirmButton);
            buttonPanel.Controls.Add(cancelButton);
            popup.Controls.Add(messageLabel);
            popup.Controls.Add(txtChavePublica);
            popup.Controls.Add(buttonPanel);
            popup.AcceptButton = confirmButton;
            popup.CancelButton = cancelButton;

            popup.ShowDialog();

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            var popup = new Form()
            {
                Width = 400,
                Height = 230,
                Text = "Connection Settings",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // Label
            var messageLabel = new Label()
            {
                Text = "A sua PORT:",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            // TextBox
            var txtChavePublica = new TextBox()
            {
                Multiline = false,
                Width = 350,
                Height = 30,
                Font = new Font("Segoe UI", 10),
                Text = PORT.ToString()
            };
            txtChavePublica.Location = new Point((popup.ClientSize.Width - txtChavePublica.Width) / 2, messageLabel.Bottom + 10);

            var buttonPanel = new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Location = new Point((popup.ClientSize.Width - 220) / 2, txtChavePublica.Bottom + 10)
            };
            var cancelButton = new Button()
            {
                Text = "OK",
                Width = 90,
                Height = 35,
                DialogResult = DialogResult.Cancel
            };


            buttonPanel.Controls.Add(cancelButton);
            popup.Controls.Add(messageLabel);
            popup.Controls.Add(txtChavePublica);
            popup.Controls.Add(buttonPanel);
            popup.CancelButton = cancelButton;

            popup.ShowDialog();
        }
    }
}
