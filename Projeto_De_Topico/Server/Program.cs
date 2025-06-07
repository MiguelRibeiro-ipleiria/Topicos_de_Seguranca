using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;
        private AesCryptoServiceProvider aes;
        private string pk;
        private string iv;

        public ClientHandler(TcpClient client, int clientID)
        {
            this.client = client;
            this.clientID = clientID;
            this.aes = new AesCryptoServiceProvider();
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

                        pk = GerarChavePrivada(publickey);
                        iv = GerarIV(publickey);
                        MandarMensagem(pk + "||" + iv);

                        break;

                    case ProtocolSICmdType.USER_OPTION_1:

                        //registro
                        string RegistroUserANDPass = protocoloSI.GetStringFromData();

                        //Obter a chave e o IV
                        string keyB64 = pk;
                        string ivB64 = iv;

                        aes.Key = Convert.FromBase64String(keyB64);
                        aes.IV = Convert.FromBase64String(ivB64);

                        Console.WriteLine(RegistroUserANDPass);

                        string RegistroDecifrado = DeCifrarTexto(RegistroUserANDPass);
                        string[] ArrayRegistro = RegistroDecifrado.Split('+');

                        string username = ArrayRegistro[0];
                        string password = ArrayRegistro[1];
                        byte[] salt = GenerateSalt(SALTSIZE);
                        byte[] hash = GenerateSaltedHash(password, salt);

                        Console.WriteLine(username);
                        Console.WriteLine(password);

                        Register(username, hash, salt);

                        break;


                    case ProtocolSICmdType.USER_OPTION_2:

                        //registro
                        string LoginUserANDPass = protocoloSI.GetStringFromData();
                        string[] ArrayLogin = LoginUserANDPass.Split('+');

                        string user = ArrayLogin[0];
                        string pass = ArrayLogin[1];
                                    
                        if (VerifyLogin(user, pass))
                        {
                            Console.WriteLine("Logado");
                            MandarMensagem("validado");
                        }
                        else
                        {
                            Console.WriteLine("Erro");
                            MandarMensagem("erro");
                        }


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

        private String GerarIV(string pass)
        {
            byte[] salt = new byte[] { 7, 8, 8, 8, 2, 5, 9, 5 };
            Rfc2898DeriveBytes pwgGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //Gerar um key
            byte[] iv = pwgGen.GetBytes(16);
            //Converter para base64
            string ivB64 = Convert.ToBase64String(iv);
            //devolver
            return ivB64;
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
                conn = new SqlConnection();
                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\migue\source\repos\Projeto_pasta_erros_clones\Pasta1\Topicos_de_Seguranca\Projeto_De_Topico\WindowsFormsApp1\Database1.mdf';Integrated Security=True");
                conn.Open();

                // ✅ Verifica se o utilizador já existe
                string checkUserSql = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                SqlCommand checkCmd = new SqlCommand(checkUserSql, conn);
                checkCmd.Parameters.AddWithValue("@username", username);

                int userExists = (int)checkCmd.ExecuteScalar();
                if (userExists > 0)
                {
                    Console.WriteLine("Utilizador já existe.");
                    MandarMensagem("erro: user já existe");
                    return;
                }

                // Se não existe, insere
                string sql = "INSERT INTO Users (Username, SaltedPasswordHash, Salt) VALUES (@username,@saltedPasswordHash,@salt)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@saltedPasswordHash", saltedPasswordHash);
                cmd.Parameters.AddWithValue("@salt", salt);

                int lines = cmd.ExecuteNonQuery();
                conn.Close();

                if (lines == 0)
                {
                    throw new Exception("Error while inserting user");
                }

                MandarMensagem("user inserido com sucesso");
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao inserir utilizador: " + e.Message);
            }
        }


        private bool VerifyLogin(string username, string password)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\migue\source\repos\Projeto_pasta_erros_clones\Pasta1\Topicos_de_Seguranca\Projeto_De_Topico\WindowsFormsApp1\Database1.mdf';Integrated Security=True");

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
                byte[] hash = GenerateSaltedHash(password, saltStored);

                return saltedPasswordHashStored.SequenceEqual(hash);

                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                //MessageBox.Show("An error occurred: " + e.Message);
                return false;
            }

        }

        private string DeCifrarTexto(string txtCifradoB64)
        {
            //Texto ara guardar o texto cifrado em Bytes
            byte[] txtCifrado = Convert.FromBase64String(txtCifradoB64);

            //Reservar espaço na memoria para colocar o texto e decifrá-lo
            MemoryStream ms = new MemoryStream(txtCifrado);
            //Inicializa o sistema de decifragem (Read)
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);

            //Variavel para guardar o texto decifrado em bytes
            byte[] txtDecifrado = new byte[ms.Length];

            //Variavel para ter o numero de bytes decifrado
            int bytesLidos = 0;

            //Decifrar os dados
            bytesLidos = cs.Read(txtDecifrado, 0, txtDecifrado.Length);
            cs.Close();

            //Converter para texto
            string txtDecifradoemTexto = Encoding.UTF8.GetString(txtDecifrado, 0, bytesLidos);

            //Devolver o texto decifrado
            return txtDecifradoemTexto;
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


    }
}
