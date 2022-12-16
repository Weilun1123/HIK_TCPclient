using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace TCP_HIK_Client
{
    public enum Status
    {
        connectSuccess = 1,
        readSuccess,
        processSuccess,
        disconnect,
        TimeOut,
        connectError,
        readError,
        processError,
    }

    public struct DataElement
    {
      public string codeContent, codeType, central_position, codeScore;
    }

    public class TCPClient_HIK
    {
        private string IP_Add;
        private int PortNo;
        private TcpClient client;
        private NetworkStream netStream;//Provides the underlying stream of data for network access.
        private string data;

        private Status status;
        private int count_reconnect;

        public TCPClient_HIK()
        {
            this.IP_Add = "";
            this.PortNo = 0;
            this.count_reconnect = 0;
        }

        public TCPClient_HIK(string IP_Add, int PortNo, int count_reconnect = 10)
        {
            this.IP_Add = IP_Add;
            this.PortNo = PortNo;
            this.count_reconnect = count_reconnect;
        }

        ~TCPClient_HIK()
        {
            client.Close();
            Console.WriteLine("DISCONNECTED");
            Console.ReadKey();
        }

        public Status ConnectToHIK()
        {
            int i = 0;
            do
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(IP_Add, PortNo);
                    Console.WriteLine("Connect Sucessfully");
                    status = Status.connectSuccess;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    status = Status.connectError;
                }
                i++;
            } while (status != Status.connectSuccess && i < count_reconnect);

            return status;
        }

        public Status receivedata(ref DataElement[] barcode, ref int size,int bufferSize, int setTimeOut, char barcode_Delimiter, char barcodeData_Delimeter)
        {
            Status readstatus;
            Status reconnstatus = 0;
            Status prcessStatus;
            do
            {
                readstatus = x_receiveData(bufferSize, setTimeOut);
                if(readstatus == Status.readSuccess)
                {
                    prcessStatus = processData(ref barcode,ref size, barcode_Delimiter, barcodeData_Delimeter);
                    return prcessStatus;
                }
                else if(readstatus == Status.disconnect)
                {
                    Console.WriteLine("Disconnect\nTry To Reconnect");
                    reconnstatus = ConnectToHIK();
                    if(reconnstatus != Status.connectSuccess)
                    {
                        return reconnstatus;
                    }
                }
                else if (readstatus == Status.TimeOut)
                {
                    Console.WriteLine("Time Out");
                    return readstatus;
                }
                else
                {
                    return readstatus;
                }
            } while (readstatus != Status.readSuccess);
            return readstatus;
        }

        /*barcode_Delimiter to separate how many barcode scanned barcodeData_Delimter to separate the data member in each barcode*/
        private Status processData(ref DataElement[] barcode, ref int size, char barcode_Delimiter, char barcodeData_Delimeter)    
        {
            try
            {
                Console.WriteLine("Before processing :" + data);

                data = data.Replace("<p>", ""); //replace the <p> with empty
                data = data.Replace("</p>", "");//replace the </p> with empty

                Console.WriteLine("Remove <p> $ </p> :" + data);

                string[] result = data.Split(barcode_Delimiter);
                size = result.Length;
                barcode = new DataElement[size];
                for (int i = 0; i < size; i++)
                {
                    Console.WriteLine("Data Set {0}: {1}", i, result[i]);
                    string[] result2 = result[i].Split(barcodeData_Delimeter);

                    if (result2.Length > 4)
                    {
                        throw new Exception("Data Received Out Of Length");
                    }

                    barcode[i].codeContent = result2[0];
                    barcode[i].codeType = result2[1];
                    barcode[i].central_position = result2[2];
                    barcode[i].codeScore = result2[3];
                }
                return Status.processSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Status.processError;
            }
        }

        private Status x_receiveData(int bufferSize, int setTimeOut)
        {
            try
            {
                netStream = client.GetStream(); // returns a NetworkStream that you can use to send and receive data.
                if (netStream.CanRead) // if can read return true; else return false
                {
                    byte[] receiveBuffer = new byte[bufferSize];
                    int bytesReceived;

                    //while ((bytesReceived = netStream.Read(receiveBuffer, 0, receiveBuffer.Length)) > 0)
                    client.ReceiveTimeout = setTimeOut; //sets the amount of time a TcpClient will wait to receive data
                    bytesReceived = netStream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    if (bytesReceived == 0)
                    {
                        throw new Exception("No Data Received");
                    }
                    else
                    {
                        data = Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);
                    }
                }
                return Status.readSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex is IOException)
                {
                    if (client.Connected == false)
                    {
                        return Status.disconnect;
                    }
                    else
                    {
                        return Status.TimeOut;
                    }
                }
                return Status.readError;
            }
        }



    }
}
