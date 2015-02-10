using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace twin_futs
{
    public class FutMgrNet
    {
        //save device connections
        public Dictionary<string, Socket> mDeviceConnections;
        //save device status
        private DeviceParams[] mDeviceParams;
        public Socket mServer;
        private Thread refreshThread;
        private int refreshFreq;
        ClientThread newClient;
        private List<OT> otList;
        private List<GRB> grbList;
        private List<ObsTar> obsTarList;
        public FutMgrNet(DeviceParams[] dev, List<OT> _otList, List<GRB> _grbList, List<ObsTar> _obsTarList)
        {
            otList = _otList;
            grbList = _grbList;
            obsTarList = _obsTarList;
            mDeviceConnections = new Dictionary<string, Socket>();
            mDeviceParams = dev;
            
            refreshFreq = 1000;
            refreshThread = new Thread(new ThreadStart(RefreshStatus));
            refreshThread.IsBackground = true;
            refreshThread.Start();
        }

        ~FutMgrNet()
        {
            

        }

        //获取local time
        private string GetLocalTime()
        {
            return DateTime.Now.Year.ToString() +
                        DateTime.Now.Month.ToString("d2") +
                        DateTime.Now.Day.ToString("d2") +
                        "T" +
                        DateTime.Now.Hour.ToString("d2") +
                        DateTime.Now.Minute.ToString("d2") +
                        DateTime.Now.Second.ToString("d2") +
                        "." +
                        DateTime.Now.Millisecond.ToString("d3");
        }


        /// <summary>
        /// 从连接字典中取出socket连接，如果存在，就向他请求STATUS
        /// </summary>
        private void RefreshStatus()
        {
            Socket value;
            string lt = "";
            string data = "";
            try
            {
                while (true)
                {
                    lt = GetLocalTime();
                    //更新S1状态
                    if (mDeviceConnections.TryGetValue("S1", out value))
                    {
                        data = "T1,STATUS," + lt;
                        Console.WriteLine(data);
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        mDeviceConnections["S1"].Send(msg);
                    }
                    //更新S2状态
                    if (mDeviceConnections.TryGetValue("S2", out value))
                    {
                        data = "T2,STATUS," + lt;
                        Console.WriteLine(data);
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        mDeviceConnections["S2"].Send(msg);
                    }
                    
                    System.Threading.Thread.Sleep(refreshFreq);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                char c = data[1];
                switch (c)
                {
                    case '1':
                        newClient.DeregisterDevice("RS1");
                        break;
                    case '2':
                        newClient.DeregisterDevice("RS2");
                        break;
                    default:
                        break;
                }
                RefreshStatus();
            }

        }

        public void StartServer()
        {
            try
            {
                //initial ip address
                //以后写成配置文件IP, Port
                IPAddress local = IPAddress.Parse(GetLocalIP());
                IPEndPoint iep = new IPEndPoint(local, 30002);
                mServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                    ProtocolType.Tcp);
                //bind ip end point to socket
                mServer.Bind(iep);
                //start listening at local port 30001
                mServer.Listen(10);
                Console.WriteLine("start listening at {0}:30002, wait for connecting...", local.ToString());
                while (true)
                {
                    //get client socket
                    Socket client = mServer.Accept();
                    //create message thread object
                    newClient = new ClientThread(client, mDeviceParams, mDeviceConnections, otList, grbList, obsTarList);
                    //pass client method to thread
                    Thread newthread = new Thread(new ThreadStart(newClient.ClientService));
                    //start thread message service
                    newthread.IsBackground = true;
                    newthread.Start();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private string GetLocalIP()
        {
            try
            {
                string hostName = Dns.GetHostName();
                Console.WriteLine("host name: " + hostName);
                IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
                foreach (IPAddress ip in ipEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                        
                    }
                }
                return "";
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("get local ip address error: ", ex.ToString());
                return "";
            }
        }




    }

    /// <summary>
    /// a tcp client service
    /// use to control client device via tcp connections
    /// </summary>
    public class ClientThread
    {
        //save device status
        private DeviceParams[] mDeviceParams;
        private Dictionary<string, Socket> mDeviceConnections;
        //member elements
        private static int mConnections = 0;
        private Socket mService;
        int i;
        List<OT> otList;
        List<GRB> grbList;
        List<ObsTar> obsTarList;
        //constructor
        public ClientThread(Socket clientSocket, DeviceParams[] deviceParams, Dictionary<string, Socket> deviceConnections, List<OT> _otList, List<GRB> _grbList, List<ObsTar> _obsTarList)
        {
            //save client socket
            this.mService = clientSocket;
            this.mDeviceParams = deviceParams;
            this.mDeviceConnections = deviceConnections;
            otList = _otList;
            grbList = _grbList;
            obsTarList = _obsTarList;
        }
        public void ClientService()
        {
            try
            {
                //directly use member data device status

                String data = null;
                byte[] bytes = new byte[1024];

                //if socket is not null, then connections plus 1, save connections
                if (mService != null)
                {
                    mConnections++;
                }
                Console.WriteLine("new client is set up: {0} connection(s)",
                    mConnections);

                while ((i = mService.Receive(bytes)) != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("data received: {0}", data);

                    //////////////////////////////////////////////////////////////////////////
                    //解析收到的字符串
                    //////////////////////////////////////////////////////////////////////////
                    //注册设备
                    RegisterDevice(data);

                    //注销设备
                    DeregisterDevice(data);

                    //解析指令
                    ResolveCmds(data);

                    //reply a message to client
                    data = "received: " + data;
                    //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    //mService.Send(msg);
                    //Console.WriteLine("data sent: {0}", data);
                }
                

                mService.Close();
                mConnections--;
                Console.WriteLine("client closed: {0} connection(s)", mConnections);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ClientService error: {0}", ex.ToString());
            }
            
        }

        //register device if first connect
        /// <summary>
        /// 注册设备，设备连接后会发送设备名字，以区别连接种类，后将其放入连接字典
        /// </summary>
        /// <param name="message"></param>
        private void RegisterDevice(string message)
        {
            string deviceName = message;
            if (deviceName == "S1" && !mDeviceConnections.ContainsKey("S1"))
            {
                mDeviceConnections.Add("S1", mService);
                Console.WriteLine("S1 is registered.");
            }
            else if (deviceName == "S2" && !mDeviceConnections.ContainsKey("S2"))
            {
                mDeviceConnections.Add("S2", mService);
                Console.WriteLine("S2 is registered.");
            }
        }

        //deregister device
        /// <summary>
        /// 注销设备，设备断开时会发送设备名称，此名称与注册时不同，如果收到则将连接从字典中移除
        /// </summary>
        /// <param name="message"></param>
        public void DeregisterDevice(string message)
        {
            if (message == "RS1" && mDeviceConnections.ContainsKey("S1"))
            {
                //mDeviceConnections["WHEEL"].Close();
                mDeviceConnections.Remove("S1");
                Console.WriteLine("S1 is deregistered.");
            }
            else if (message == "RS2" && mDeviceConnections.ContainsKey("S2"))
            {
                //mDeviceConnections["CCD"].Close();
                mDeviceConnections.Remove("S2");
                Console.WriteLine("S2 is deregistered.");
            }
            
        }

        private void SendMessage(string msg)
        {
            try
            {
                byte[] buf = Encoding.ASCII.GetBytes(msg);
                if (true == mService.Connected)
                {
                    mService.Send(buf);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ResolveCmds(string message)
        {
            try
            {
                //resolve message
                string deviceType, cmd;
                string pos, mov, res, lt;
                string ra = "", dec = "", az = "", alt = "", date = "", ut = "", st = "", movStat = "";
                string temp = "", coolerSwitch = "", acqStat = "", gain = "", binx = "", biny = "", imgPath = "";
                string fileName = "", shutter = "", expTime = "", amount = "", curNumb = "", acqProc = "", imgAmt = "";
                string targetRa = "", targetDec = "";
                string[] cmdList = message.Split(',');
                deviceType = cmdList[0];
                string reply = "";
                
                int futId = 0;

                if (cmdList.Length < 3)
                    return;
                cmd = cmdList[1];
                lt = cmdList.Last();

                //解析望远镜单元消息
                if (deviceType == "RT1" || deviceType == "RT2")
                {
                    if (deviceType=="RT1")
                    {
                        futId=0;
                    }
                    else if (deviceType=="RT2")
                    {
                        futId=1;
                    }
                    if (cmd == "STATUS")
                    {
                        
                        //if (cmdList.Length < 27)
                        //    return;
                        //mount status
                        mDeviceParams[futId].mountParams.ra = cmdList[2];
                        mDeviceParams[futId].mountParams.dec = cmdList[3];
                        mDeviceParams[futId].mountParams.targetRa = cmdList[4];
                        mDeviceParams[futId].mountParams.targetDec = cmdList[5];
                        mDeviceParams[futId].mountParams.az = cmdList[6];
                        mDeviceParams[futId].mountParams.alt = cmdList[7];
                        mDeviceParams[futId].mountParams.date = cmdList[8];
                        mDeviceParams[futId].mountParams.ut = cmdList[9];
                        mDeviceParams[futId].mountParams.st = cmdList[10];
                        mDeviceParams[futId].mountParams.stat = int.Parse(cmdList[11]);
                        //focuser status
                        mDeviceParams[futId].focuserParams.pos = double.Parse(cmdList[12]);
                        mDeviceParams[futId].focuserParams.isMoving = int.Parse(cmdList[13]);
                        mDeviceParams[futId].focuserParams.temp = double.Parse(cmdList[14]);
                        //ccd status
                        mDeviceParams[futId].ccdParams.temp = double.Parse(cmdList[15]);
                        mDeviceParams[futId].ccdParams.coolerSwitch = cmdList[16] == "1" ? true : false;
                        mDeviceParams[futId].ccdParams.acqStat = int.Parse(cmdList[17]);
                        mDeviceParams[futId].ccdParams.gain = int.Parse(cmdList[18]);
                        mDeviceParams[futId].ccdParams.binx = int.Parse(cmdList[19]);
                        mDeviceParams[futId].ccdParams.biny = int.Parse(cmdList[20]);
                        mDeviceParams[futId].ccdParams.imgPath = cmdList[21];
                        mDeviceParams[futId].ccdParams.acqProc = double.Parse(cmdList[22]);
                        mDeviceParams[futId].ccdParams.curNumb = int.Parse(cmdList[23]);
                        mDeviceParams[futId].ccdParams.imgAmt = int.Parse(cmdList[24]);
                        //wheel status
                        mDeviceParams[futId].wheelParams.curPos = int.Parse(cmdList[25]);
                        mDeviceParams[futId].wheelParams.movStatus = int.Parse(cmdList[26]);
                        //tele status
                        mDeviceParams[futId].teleStat = int.Parse(cmdList[27]);
                    }
                    else if (cmd == "OBS")
                    {
                        ObsTar obsTar = new ObsTar();
                        obsTar.id = int.Parse(cmdList[2]);
                        obsTar.ra = cmdList[3];
                        obsTar.dec = cmdList[4];
                        obsTar.color = cmdList[5];
                        obsTar.expTime = double.Parse(cmdList[6]);
                        obsTar.amount = int.Parse(cmdList[7]);
                        obsTar.fileName = cmdList[8];
                        res = cmdList[9];
                        lt = cmdList[10];
                        if (res == "0")
                        {
                            //type == 0表示源已经观测
                            obsTar.type = 0;
                        }
                        //加入已观测列表，准备更新数据库
                        obsTarList.Add(obsTar);
                        
                    }
                    else if (cmd == "INIT")
                    {
                        res = cmdList[2];
                        //更新整个的望远镜状态，1表示正在观测，0表示空闲
                        mDeviceParams[futId].teleStat = int.Parse(res);

                    }
                    else if (cmd == "HOUSEKEEPING")
                    {
                        //housekeeping策略
                        if (futId == 0)
                        {
                            //一号机策略
                        }
                        else if (futId == 1)
                        {
                            //二号机策略
                        }

                    }
                    else
                        return;

                }
                else if (deviceType == "Z")
                {
                    if (cmd == "OT")
                    {
                        //接收OT
                        if (cmdList[2] != "OT")
                            return;
                        if (cmdList.Length < 10)
                            return;
                        OT ot = new OT();
                        ot.id = cmdList[3];
                        ot.ra = cmdList[4];
                        ot.dec = cmdList[5];
                        ot.mag = cmdList[6];
                        ot.status = cmdList[7];
                        ot.endFlag = cmdList[8];
                        if (ot.endFlag != "END")
                        {
                            return;
                        }
                        lt = cmdList[9];
                        //加入OT列表，以便加入数据库
                        otList.Add(ot);
                        //send reply message
                        reply = "R" + string.Join(",", cmdList, 0, cmdList.Length - 1);
                        reply += "," + "0" + "," + lt;
                        SendMessage(reply);

                    }
                    else if (cmd == "GRB")
                    {
                        res = cmdList[cmdList.Length - 2];
                        //接收消息策略
                    }
                }
                

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("cmds error: {0}", ex.ToString());
            }

        }

    }
}
