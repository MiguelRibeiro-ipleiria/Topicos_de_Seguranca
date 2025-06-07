using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EI.SI;
namespace Server
{
    class Program
    {
        private const int PORT = 10000;
        private string publickey;
        private static int clientes_counter = 0;
        public static List<ClientHandler> clientes = new List<ClientHandler>();
        public static readonly object lockObj = new object();

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
            TcpListener listener = new TcpListener(endPoint);

            listener.Start();
            Console.WriteLine("The server is READY!!");
            int clientes_counter = 0;


   
           


            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                clientes_counter++;
                Console.WriteLine("Client {0} connected", clientes_counter);
                ClientHandler clientHandler = new ClientHandler(client, clientes_counter);

                lock (lockObj)
                {
                    clientes.Add(clientHandler);
                }


                clientHandler.Handle();
            }
        }
    }

    class ClientHandler
    {
        private TcpClient client;
        private int clientID;

        public ClientHandler(TcpClient client, int clientID)
        {
            this.client = client;
            this.clientID = clientID;
        }

        public void Handle()
        {
            Thread thread = new Thread(threadHandler);
            thread.Start();

        }

        private void threadHandler()
        {
            NetworkStream networkStream = this.client.GetStream();
            ProtocolSI protocoloSI = new ProtocolSI();

            while (protocoloSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                int bytesRead = networkStream.Read(protocoloSI.Buffer, 0, protocoloSI.Buffer.Length);
                byte[] ack;
                switch (protocoloSI.GetCmdType())
                {
                    case ProtocolSICmdType.DATA:
                        //ESCREVER MENSAGEM DO CLIENTE
                        string mensagemRecebida = protocoloSI.GetStringFromData();
                        Console.WriteLine("Client " + clientID + ": " + mensagemRecebida);

                        ack = protocoloSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);

                        //RETORNAR A MENSAGEM AO CLIENTE
                        lock (Program.lockObj)
                        {
                            foreach (var clientes in Program.clientes)
                            {
                                if (clientes != this)
                                {
                                    clientes.MandarMensagem("Cliente " + clientID + ": " + mensagemRecebida);
                                }
                            }
                        }

                        break;
                    // CASO O CLIENTE ENVIO EOT (FIM DE TRANSMISSAO)
                    case ProtocolSICmdType.EOT:
                        Console.WriteLine("Ending Thread from Client {0}", clientID);
                        ack = protocoloSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;

                    case ProtocolSICmdType.PUBLIC_KEY:

                        string publickey = protocoloSI.GetStringFromData();
                        Console.WriteLine("PUBLICKEY :"+ publickey);

                        string pk = GerarChavePrivada(publickey);

                        MandarMensagem(pk);


                    break;

                    case ProtocolSICmdType.USER_OPTION_1:

                        /*//registro
                        string publickey = protocoloSI.GetStringFromData();
                        Console.WriteLine("PUBLICKEY :" + publickey);

                        string pk = GerarChavePrivada(publickey);

                        MandarMensagem(pk);*/


                    break;
                }


            }
            networkStream.Close();
            client.Close();

        }
        private string GerarChavePrivada(string pass)
        {
            byte[] salt = new byte[] { 0, 1, 0, 8, 2, 9, 9, 7 };

            Rfc2898DeriveBytes pwGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //Generate key
            byte[] key = pwGen.GetBytes(16);

            //Converter a chave para BASE64
            string pass64 = Convert.ToBase64String(key);

            return pass64;
        }

        private void MandarMensagem(string mensagemenviada)
        {
            try
            {
                ProtocolSI protocolSI = new ProtocolSI();
                NetworkStream ns = client.GetStream();
                byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, mensagemenviada);
                ns.Write(packet, 0, packet.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar para cliente " + clientID + ": " + ex.Message);
            }
        }

        private void Register(string username, byte[] saltedPasswordHash, byte[] salt)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\migue\source\repos\PROJETO_OFICIAL_DE_TOPICOS_DE_SEGURANCA\Topicos_de_Seguranca\Projeto_De_Topico\WindowsFormsApp1\Database1.mdf';Integrated Security=True");

                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração dos parâmetros do comando SQL
                SqlParameter paramUsername = new SqlParameter("@username", username);
                SqlParameter paramPassHash = new SqlParameter("@saltedPasswordHash", saltedPasswordHash);
                SqlParameter paramSalt = new SqlParameter("@salt", salt);

                // Declaração do comando SQL
                String sql = "INSERT INTO Users (Username, SaltedPasswordHash, Salt) VALUES (@username,@saltedPasswordHash,@salt)";

                // Prepara comando SQL para ser executado na Base de Dados
                SqlCommand cmd = new SqlCommand(sql, conn);

                // Introduzir valores aos parâmentros registados no comando SQL
                cmd.Parameters.Add(paramUsername);
                cmd.Parameters.Add(paramPassHash);
                cmd.Parameters.Add(paramSalt);

                // Executar comando SQL
                int lines = cmd.ExecuteNonQuery();

                // Fechar ligação
                conn.Close();
                if (lines == 0)
                {
                    // Se forem devolvidas 0 linhas alteradas então o não foi executado com sucesso
                    throw new Exception("Error while inserting an user");
                }
                //MessageBox.Show("USER INSERIDO COM SUCESSO!");
            }
            catch (Exception e)
            {
                throw new Exception("Error while inserting an user:" + e.Message);
            }
        }

        private bool VerifyLogin(string username, string password)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\migue\source\repos\PROJETO_OFICIAL_DE_TOPICOS_DE_SEGURANCA\Topicos_de_Seguranca\Projeto_De_Topico\WindowsFormsApp1\Database1.mdf';Integrated Security=True");

                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração do comando SQL
                String sql = "SELECT * FROM Users WHERE Username = @username";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;

                // Declaração dos parâmetros do comando SQL
                SqlParameter param = new SqlParameter("@username", username);

                // Introduzir valor ao parâmentro registado no comando SQL
                cmd.Parameters.Add(param);

                // Associar ligação à Base de Dados ao comando a ser executado
                cmd.Connection = conn;

                // Executar comando SQL
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    throw new Exception("Error while trying to access an user");
                }

                // Ler resultado da pesquisa
                reader.Read();

                // Obter Hash (password + salt)
                byte[] saltedPasswordHashStored = (byte[])reader["SaltedPasswordHash"];

                // Obter salt
                byte[] saltStored = (byte[])reader["Salt"];

                conn.Close();

                //TODO: verificar se a password na base de dados 
                //byte[] hash = GenerateSaltedHash(password, saltStored);

                //return saltedPasswordHashStored.SequenceEqual(hash);

                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                //MessageBox.Show("An error occurred: " + e.Message);
                return false;
            }
        }


    }
}
