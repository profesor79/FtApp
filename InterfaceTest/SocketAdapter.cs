using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TXCommunication;

namespace InterfaceTest
{
    class SocketAdapter : IRfcommAdapter
    {
        
        private Socket _sender;
        public bool Opened { get; private set; }
        public bool DumpToConsole { get; set; }


        public void OpenConnection(string address)
        {
            // Data buffer for incoming data.
            var bytes = new byte[1024];

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.

                var ipAddress = IPAddress.Parse(address);
                var remoteEP = new IPEndPoint(ipAddress, 65000);

                // Create a TCP/IP  socket.
                 _sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {

                    if (_sender.Connected)
                    {
                        return;
                    }

                    _sender.Connect(remoteEP);
                    
                    Opened = true;

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public void CloseConnection()
        {
            if (Opened)
            {
                // Release the socket.
                _sender?.Shutdown(SocketShutdown.Both);
                _sender?.Close();
            }
        }

        public void Write(byte[] bytes)
        {
            if (DumpToConsole)
            {
                Console.WriteLine("Write:");
                for (int i = 0; i < bytes.Length; i++)
                {
                    Console.Write(BitConverter.ToString(new[] { bytes[i] }) + " ");

                    if ((i + 1) % 16 == 0)
                    {
                        Console.WriteLine();
                    }
                }
                Console.WriteLine();
            }

            // Send the data through the socket.
            _sender.Send(bytes);
            
        }


        public bool IsAvaliable(string address)
        {
            return true;
        }

        public byte[] Read(int count)
        {
            byte[] bytes = new byte[count];

            // Receive the response from the remote device.
            var bytesRec = _sender.Receive(bytes);
            Console.WriteLine("Echoed test = {0}",
                Encoding.ASCII.GetString(bytes, 0, bytesRec));


            if (!DumpToConsole) return bytes;

            Console.WriteLine("Read:");
            for (int i = 0; i < bytes.Length; i++)
            {
                Console.Write(BitConverter.ToString(new[] { bytes[i] }) + " ");

                if ((i + 1) % 16 == 0)
                {
                    Console.WriteLine();
                }
            }
            Console.WriteLine();

            return bytes;
        }

        public void Dispose()
        {
            _sender.Dispose();
        }
    }
}
