using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_HIK_Client;

namespace testing
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPClient_HIK client = new TCPClient_HIK("127.0.0.1",54000,5);
            DataElement[] data = { };
            Status var, statusreceive;

            var = client.ConnectToHIK();
            if (var == Status.connectSuccess)
            {
                do
                {
                    int size = 0;
                    statusreceive = client.receivedata(ref data, ref size, 512, 5000,'+','$');
                    if (statusreceive != Status.processSuccess)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Number Of Barcode received :" + size);
                        for (int i = 0; i < size; i++)
                        {
                            Console.WriteLine(data[i].codeContent);
                            Console.WriteLine(data[i].codeType);
                            Console.WriteLine(data[i].central_position);
                            Console.WriteLine(data[i].codeScore);
                        }
                    }
                } while (statusreceive == Status.processSuccess);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.ReadKey();
        }
    }
}
