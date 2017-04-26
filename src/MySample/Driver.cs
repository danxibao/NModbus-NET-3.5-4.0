using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;

using System.Collections.Generic;

namespace MySample
{
	/// <summary>
	/// Demonstration of NModbus
	/// </summary>
	public class Driver
	{
		static void Main(string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			try
			{
                ReadRegisters();
				//ModbusTcpMasterReadCoils();
				
				//ModbusTcpMasterReadInputs();
				//ModbusTcpMasterReadInputsFromModbusSlave();
				//StartModbusTcpSlave();
				//StartModbusUdpSlave();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			Console.ReadKey();
		}

        public static void ReadRegisters()
        {
            using (TcpClient client = new TcpClient("192.168.31.249", 502))
            {
                ushort[] r = { 101, 102, 103, 401 };
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);
                master.WriteMultipleRegisters(1, 200, new ushort[] { 123, 456 });
                r=master.ReadHoldingRegisters(1, 200, 10);

                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Input {0}={1}", 0 + i, r[i]);

            }
        }

        /// <summary>
        /// Simple Modbus TCP master read inputs example.
        /// </summary>
        public static void ModbusTcpMasterReadCoils()
        {
            using (TcpClient client = new TcpClient("192.168.31.100", 502))
            {

                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

                bool[] data = { true, false, true, false, true, true, false, true, false, true };
                master.WriteMultipleCoils(1, 0, data);
                bool[] data2 = { false, false, false, false };
                master.WriteMultipleCoils(1, 0, data2);
                string s;
                while (true)
                {
                    Console.Write("input startAddress:\n");
                    s = Console.ReadLine();
                    // read five input values
                    ushort startAddress = ushort.Parse(s);
                    Console.Write("input numInputs:\n");
                    s = Console.ReadLine();
                    ushort numInputs = ushort.Parse(s);


                    bool[] inputs = master.ReadCoils(1, startAddress, numInputs);


                    for (int i = 0; i < numInputs; i++)
                        Console.WriteLine("Input {0}={1}", startAddress + i, inputs[i] ? 1 : 0);

                }


            } 

            // output: 
            // Input 100=0
            // Input 101=0
            // Input 102=0
            // Input 103=0
            // Input 104=0
        }

		/// <summary>
		/// Simple Modbus TCP master read inputs example.
		/// </summary>
		public static void ModbusTcpMasterReadInputs()
		{
			using (TcpClient client = new TcpClient("192.168.31.100", 502))
			{
                
				ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

                string s;
                while (true)
                {
                    Console.Write("input startAddress:\n");
                    s = Console.ReadLine();
                    // read five input values
                    ushort startAddress = ushort.Parse(s);
                    Console.Write("input numInputs:\n");
                    s = Console.ReadLine();
                    ushort numInputs = ushort.Parse(s);

                    bool[] inputs = master.ReadInputs(1,startAddress, numInputs);


                    for (int i = 0; i < numInputs; i++)
                        Console.WriteLine("Input {0}={1}", startAddress + i, inputs[i] ? 1 : 0);

                }
                
                
			}

			// output: 
			// Input 100=0
			// Input 101=0
			// Input 102=0
			// Input 103=0
			// Input 104=0
		}

		/// <summary>
		/// Simple Modbus UDP master write coils example.
		/// </summary>
		public static void ModbusUdpMasterWriteCoils()
		{			
			using (UdpClient client = new UdpClient())
			{
				IPEndPoint endPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);
				client.Connect(endPoint);

				ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

				ushort startAddress = 1;

				// write three coils
				master.WriteMultipleCoils(startAddress, new bool[] { true, false, true });
			}
		}


		/// <summary>
		/// Simple Modbus TCP slave example.
		/// </summary>
		public static void StartModbusTcpSlave()
		{
			byte slaveId = 1;
			int port = 502;
			IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

			// create and start the TCP slave
			TcpListener slaveTcpListener = new TcpListener(address, port);
			slaveTcpListener.Start();

			ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);
			slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

			slave.Listen();

			// prevent the main thread from exiting
			Thread.Sleep(Timeout.Infinite);
		}

		/// <summary>
		/// Simple Modbus UDP slave example.
		/// </summary>
		public static void StartModbusUdpSlave()
		{
			using (UdpClient client = new UdpClient(502))
			{
				ModbusUdpSlave slave = ModbusUdpSlave.CreateUdp(client);
				slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

				slave.Listen();

				// prevent the main thread from exiting
				Thread.Sleep(Timeout.Infinite);
			}
		}

		/// <summary>
		/// Modbus TCP master and slave example.
		/// </summary>
		public static void ModbusTcpMasterReadInputsFromModbusSlave()
		{
			byte slaveId = 1;
			int port = 502;
			IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

			// create and start the TCP slave
			TcpListener slaveTcpListener = new TcpListener(address, port);
			slaveTcpListener.Start();
			ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);
			Thread slaveThread = new Thread(slave.Listen);
			slaveThread.Start();

			// create the master
			TcpClient masterTcpClient = new TcpClient(address.ToString(), port);
			ModbusIpMaster master = ModbusIpMaster.CreateIp(masterTcpClient);

			ushort numInputs = 5;
			ushort startAddress = 0;

			// read five register values
			ushort[] inputs = master.ReadInputRegisters(startAddress, numInputs);

			for (int i = 0; i < numInputs; i++)
				Console.WriteLine("Register {0}={1}", startAddress + i, inputs[i]);

			// clean up
			masterTcpClient.Close();
			slaveTcpListener.Stop();

			// output
			// Register 100=0
			// Register 101=0
			// Register 102=0
			// Register 103=0
			// Register 104=0
		}

	}
}
