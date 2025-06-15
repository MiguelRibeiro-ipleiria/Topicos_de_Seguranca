using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal class Cifragem
    {
        private AesCryptoServiceProvider aes;

        public string DeCifrarTexto(string txtCifradoB64)
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

        public string CifrarTexto(string TextoACifrar)
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
