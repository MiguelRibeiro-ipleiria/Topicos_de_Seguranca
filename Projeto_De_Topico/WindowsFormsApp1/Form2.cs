using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private Form1 form1Ref;
        bool hidepass = true;

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();  
            Form1 form1 = new Form1(this); 
            form1.Show(); 
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
    }
}
