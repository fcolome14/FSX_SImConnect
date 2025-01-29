using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports; //Must use version 6.X.X instead of 8.X.X

namespace FSX_SImConnect_test
{
    public class ArduinoConnection
    {
        private SerialPort serialPort;

        public ArduinoConnection(string portName, int baudRate)
        {
            // Initialize serial port connection to Arduino
            serialPort = new SerialPort(portName, baudRate)
            {
                DtrEnable = true,
                RtsEnable = true,
                NewLine = "\r\n"
            };
        }

        public void Connect()
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    Console.WriteLine("Connected to Arduino.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to Arduino: " + ex.Message);
            }
        }

        public void SendData(string data)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    byte[] byteArray = Encoding.ASCII.GetBytes(data);
                    serialPort.Write(byteArray, 0, byteArray.Length);
                    // Optionally, you can convert the byte array to a string representation for logging
                    string dataString = Encoding.ASCII.GetString(byteArray);
                    Console.WriteLine("Data sent to Arduino: " + dataString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send data to Arduino: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                Console.WriteLine("Disconnected from Arduino.");
            }
        }
    }
}
