using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EI.SI;
namespace Server
{
    class Program
    {
        private const int PORT = 10000;
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
                }


            }
            networkStream.Close();
            client.Close();

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


    }
}
