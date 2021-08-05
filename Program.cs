using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;

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

        [STAThread]
        static void Main()
        {
            Thread Rt = new Thread(CokpitCom);
            Rt.Start();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmPlayer());
        }

        static void CokpitCom()
        {
            LogitechGSDK.LogiSteeringShutdown();
            Thread.Sleep(1000);
            LogitechGSDK.LogiSteeringInitialize(false);
            Thread.Sleep(1000);
            System.Timers.Timer timer = new System.Timers.Timer(150);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(RemoteCom);
            timer.AutoReset = true;
            timer.Start();
        }

        private static void RemoteCom(object src, ElapsedEventArgs e)
        {
            if (LogitechGSDK.LogiUpdate()&& LogitechGSDK.LogiIsConnected(0))
            {
                LogitechGSDK.DIJOYSTATE2ENGINES rec;
                rec = LogitechGSDK.LogiGetStateUnity(0);
                double LogitechGear = 0;
                
                if (LogitechGSDK.LogiButtonTriggered(0, 12)
                    ||LogitechGSDK.LogiButtonIsPressed(0, 12)
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
                RemoteSteer = rec.lX  * 45 / 32768;
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
                
                Debug.WriteLine("Steering:" + RemoteSteer);
                Debug.WriteLine("Throttle:" + RemoteThrottle);
                Debug.WriteLine("Brake:" + RemoteBrake);
                Debug.WriteLine("Gear:" + RemoteGear);
                Debug.WriteLine("Start:" + RemoteStart);
            }
        }
    }
}
