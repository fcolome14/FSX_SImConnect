using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace FSX_SimConnectClient
{
    class Program
    {
        private static SimConnect simconnect = null;
        private static SerialPort arduinoSerial;
        private static bool arduinoReady = true;
        const int WM_USER_SIMCONNECT = 0x0402;

        enum DATA_REQUESTS { REQUEST_1 }
        enum DATA_DEFINITION { AIRCRAFT_DATA }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct AircraftData
        {
            // Fixed size byte array for Title (SimConnect STRING256)
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Title;
            public double Latitude;
            public double Longitude;
            public double Altitude;
            public double Airspeed;     // Indicated airspeed in knots
            public double EngineFire;
            public double FuelQTY;
            public double MainRotorRPM;
            public double Generator;
            public double PitotHeat;
            public double HydraulicPressure;
            public double HydraulicQTY;
            public double OilPressure;
            public double OilTemperature;
            public double EngineTorque;
            public double FuelPressure;
            public double EngineRPM;
            public double Electrical;
        }

        static void Main(string[] args)
        {
            try
            {
                // Initialize SimConnect
                simconnect = new SimConnect("SimConnect Client", IntPtr.Zero, WM_USER_SIMCONNECT, null, 0);
                Console.WriteLine("Connected to FSX.");

                // Initialize Arduino SerialPort
                arduinoSerial = new SerialPort("COM3", 115200);
                arduinoSerial.DataReceived += ArduinoDataReceived; // Event for incoming Arduino data
                arduinoSerial.Open();
                Console.WriteLine("Connected to Arduino.");

                // Set up SimConnect data definitions and requests
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "INDICATED ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ENG ON FIRE:1", "bool", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "FUEL TOTAL QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ROTOR RPM PCT:1", "percent over 100", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ELECTRICAL BATTERY LOAD", "amps", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "PITOT HEAT", "bool", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ENG HYDRAULIC PRESSURE:1", "psf", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ENG HYDRAULIC QUANTITY:1", "percent over 100", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ENG OIL PRESSURE:1", "psf", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ENG OIL TEMPERATURE:1", "rankine", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ENG TORQUE PERCENT:1", "percent scaler 16k", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ENG FUEL PRESSURE:1", "psi scaler 16k", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "TURB ENG N2:1", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DATA_DEFINITION.AIRCRAFT_DATA, "ELECTRICAL MAIN BUS VOLTAGE", "volts", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.RegisterDataDefineStruct<AircraftData>(DATA_DEFINITION.AIRCRAFT_DATA);
                simconnect.RequestDataOnSimObject(DATA_REQUESTS.REQUEST_1, DATA_DEFINITION.AIRCRAFT_DATA, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                simconnect.OnRecvSimobjectData += SimConnect_OnReceiveSimObjectData;

                // Keep program running to listen for data
                while (true) simconnect.ReceiveMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection error: " + ex.Message);
            }
            finally
            {
                simconnect?.Dispose();
                arduinoSerial?.Close();
            }
        }

        private static void SimConnect_OnReceiveSimObjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (data.dwRequestID == (uint)DATA_REQUESTS.REQUEST_1)
            {
                AircraftData aircraftData = (AircraftData)data.dwData[0];
                string dataToSend = $"ASPD:{Math.Round(aircraftData.Airspeed, 2)}/FQTY:0/TRQ:0/FIRE:{aircraftData.EngineFire}/FUEL:{Math.Round(aircraftData.FuelQTY, 2)}" +
                    $"/MR:{Math.Round(aircraftData.MainRotorRPM * 400, 2)}/GEN:{Math.Round(aircraftData.Generator,2)}/PIT:{aircraftData.PitotHeat}" +
                    $"/HYDP:{Math.Round(aircraftData.HydraulicPressure, 2)}/HYDQ:{Math.Round(aircraftData.HydraulicQTY, 2)}/OILP:{Math.Round(aircraftData.OilPressure, 2)}" +
                    $"/OILT:{Math.Round(aircraftData.OilTemperature, 1)}/TRQ:{Math.Round((aircraftData.EngineTorque / 16384.0) * 100, 2)}" +
                    $"/FPR:{Math.Round((aircraftData.FuelPressure / 16384.0), 2)}/ENGRPM:{Math.Round(aircraftData.EngineRPM, 2)}/ALT:{Math.Round(aircraftData.Altitude, 2)}" +
                    $"/ELEC:{Math.Round(aircraftData.Electrical, 2)}\n";

                if (arduinoReady) // Send data only if Arduino is ready
                {
                    arduinoReady = false; // Reset readiness after sending
                    arduinoSerial.Write(dataToSend);
                    Console.WriteLine("Data sent to Arduino: " + dataToSend);
                }
            }
        }

        private static void ArduinoDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string message = arduinoSerial.ReadLine();
            Console.WriteLine("Message from Arduino: " + message);

            // Set arduinoReady flag if "READY" message is received
            if (message.Trim() == "READY")
            {
                arduinoReady = true;
                Console.WriteLine("Arduino is ready for next data.");
            }
        }
    }
}
