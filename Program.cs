using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Remote_Cockpit
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static double RemoteStart = 0;
        static double RemoteThrottle = 0;
        static double RemoteBrake = 0;
        static double RemoteSteer = 0;
        static double RemoteGear = 0;
        public class ChassisCommand 
        {
            public double Throttle;
            public double Brake;
            public double Steer;
            public double Gear;
            public double Start;
        }

        //[STAThread]
        static void Main()
        {
            Thread RemoteThread = new Thread(RemoteCom);
            RemoteThread.IsBackground = true;
            RemoteThread.Start();

            Thread CokpitThread = new Thread(CokpitCom);
            CokpitThread.IsBackground = true;
            CokpitThread.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmPlayer());
        }

        static void CokpitCom()
        {
            LogitechGSDK.LogiSteeringShutdown();
            Thread.Sleep(1000);
            while (!LogitechGSDK.LogiSteeringInitialize(false)) ;
            while (true)
            {
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    LogitechGSDK.DIJOYSTATE2ENGINES rec;
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    double LogitechGear = 0;

                    if (LogitechGSDK.LogiButtonTriggered(0, 12)
                        || LogitechGSDK.LogiButtonIsPressed(0, 12)
                         || LogitechGSDK.LogiButtonTriggered(0, 14)
                          || LogitechGSDK.LogiButtonIsPressed(0, 14)
                           || LogitechGSDK.LogiButtonTriggered(0, 16)
                            || LogitechGSDK.LogiButtonIsPressed(0, 16))
                    {
                        LogitechGear = 1;
                    }

                    if (LogitechGSDK.LogiButtonTriggered(0, 13)
                        || LogitechGSDK.LogiButtonIsPressed(0, 13)
                         || LogitechGSDK.LogiButtonTriggered(0, 15)
                          || LogitechGSDK.LogiButtonIsPressed(0, 15)
                           || LogitechGSDK.LogiButtonTriggered(0, 17)
                            || LogitechGSDK.LogiButtonIsPressed(0, 17))
                    {
                        LogitechGear = -1;
                    }

                    if (LogitechGSDK.LogiButtonReleased(0, 12)
                       || LogitechGSDK.LogiButtonReleased(0, 13)
                        || LogitechGSDK.LogiButtonReleased(0, 14)
                         || LogitechGSDK.LogiButtonReleased(0, 15)
                          || LogitechGSDK.LogiButtonReleased(0, 16)
                           || LogitechGSDK.LogiButtonReleased(0, 17)
                            || LogitechGSDK.LogiButtonReleased(0, 18)
                             || LogitechGSDK.LogiButtonTriggered(0, 18)
                              || LogitechGSDK.LogiButtonIsPressed(0, 18))
                    {
                        LogitechGear = 0;
                    }

                    if (LogitechGSDK.LogiButtonTriggered(0, 4)
                        || LogitechGSDK.LogiButtonIsPressed(0, 4)
                         || LogitechGSDK.LogiButtonReleased(0, 4))
                    {
                        RemoteStart = 0;
                    }

                    if (LogitechGSDK.LogiButtonTriggered(0, 5)
                        || LogitechGSDK.LogiButtonIsPressed(0, 5)
                         || LogitechGSDK.LogiButtonReleased(0, 5))
                    {
                        RemoteStart = 1;
                    }

                    RemoteGear = LogitechGear;
                    RemoteSteer = rec.lX * 35 / 32768;
                    if (1 == RemoteStart)
                    {
                        if (1 == RemoteGear || -1 == RemoteGear)
                        {
                            RemoteBrake = 100 - 100 * (rec.lRz + 32768) / 65535;
                            if (RemoteBrake > 0)
                            {
                                RemoteThrottle = 0;
                            }
                            else
                            {
                                RemoteThrottle = 100 - 100 * (rec.lY + 32768) / 65535;
                                RemoteBrake = 0;
                            }
                        }
                        else
                        {
                            RemoteGear = 0;
                            RemoteBrake = 0;
                            RemoteThrottle = 0;
                        }
                    }
                    else
                    {
                        RemoteGear = 0;
                        RemoteBrake = 0;
                        RemoteThrottle = 0;
                    }
                    LogitechGSDK.LogiSteeringInitialize(false);
                }
            }
        }

        static void RemoteCom()
        {
            while (true)
            {
                try
                {
                    int port = 23456;
                    string address = "127.0.0.1";
                    IPAddress addr = IPAddress.Parse(address);
                    IPEndPoint ipe = new IPEndPoint(addr, port);
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(ipe);
                    socket.Listen(1);
                    Console.WriteLine("已启动监听，等待客户端连接。");
                    Socket clientSocket = socket.Accept();
                    IPEndPoint clientIp = (IPEndPoint)clientSocket.RemoteEndPoint;//获取远程终结点信息
                    if (clientSocket != null)
                        Console.WriteLine("成功与{0}的客户端建立联系", clientIp);
                    AutoResetEvent[] watchers = new AutoResetEvent[2];
                    watchers[0] = new AutoResetEvent(false);
                    watchers[1] = new AutoResetEvent(false);
                    new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                int recv;//记录客户端信息的长度
                                byte[] data = new byte[1024];
                                recv = clientSocket.Receive(data);//获取客户端传过来的信息
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR:{0}", ex.Message);
                                if (clientSocket != null) clientSocket.Close();
                                break;
                            }
                        }
                        watchers[0].Set();
                    }).Start();

                    new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                ChassisCommand chassisCmd = new ChassisCommand()
                                { Throttle = RemoteThrottle, Brake = RemoteBrake, Gear = RemoteGear, Start = RemoteStart, Steer = RemoteSteer };
                                if (clientSocket != null) clientSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(chassisCmd)));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR:{0}", ex.Message);
                                if (clientSocket != null) clientSocket.Close();
                                break;
                            }
                            Thread.Sleep(20);
                        }
                        watchers[1].Set();
                    }).Start();

                    WaitHandle.WaitAll(watchers);
                    Console.WriteLine("断开连接");
                    if (clientSocket != null) clientSocket.Close();
                    if (socket != null) socket.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR:{0}", ex.Message);
                    Console.WriteLine("断开连接");
                }
            }
        }
    }
}
