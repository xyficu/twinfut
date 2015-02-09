using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace twin_futs
{
    public partial class FormFuts : Form
    {
        // 自定义望远镜状态字
        /** 望远镜状态字
         * 0  : 初始状态, 断开连接
         * 1  : 静止
         * 2  : 搜索零点中
         * 3  : 搜索零点成功
         * 4  : 指向中
         * 5  : 跟踪中
         * 6  : 轴控制中
         * 7  : 复位中
         * 8  : 复位
         **/
        private const int TS_NONE = 0;
        private const int TS_Stopped = 1;
        private const int TS_Homing = 2;
        private const int TS_Homed = 3;
        private const int TS_Slewing = 4;
        private const int TS_Tracking = 5;
        private const int TS_Moving = 6;
        private const int TS_Parking = 7;
        private const int TS_Parked = 8;

        FutPgsql pgUser;
        FutMgrNet futMgr;
        DeviceParams[] deviceParams;
        Thread futMgrThread;
        private List<OT> otList;
        private List<GRB> grbList;

        public FormFuts()
        {
            InitializeComponent();

            pgUser = new FutPgsql();
            otList = new List<OT>();
            grbList = new List<GRB>();

            deviceParams = new DeviceParams[]{new DeviceParams(), new DeviceParams()};
            futMgr = new FutMgrNet(deviceParams, otList, grbList);

            //启动总控网络信息收发
            futMgrThread = new Thread(new ThreadStart(futMgr.StartServer));
            futMgrThread.IsBackground = true;
            futMgrThread.Start();


            //
            timerUpdateStatus.Interval = 100;
            timerUpdateStatus.Enabled = true;
        }

        private void timerUpdateStatus_Tick(object sender, EventArgs e)
        {
            UpdateFutStatusS1();
            UpdateFutStatusS2();
        }
        
        private void UpdateFutStatusS1()
        {
            if (futMgr!=null && futMgr.mDeviceConnections.ContainsKey("S1"))
            {
                labelS1NetStat.ForeColor = Color.Green;
                labelS1NetStat.Text = "一号机网络已连接";
            }
            else
            {
                labelS1NetStat.ForeColor = Color.Red;
                labelS1NetStat.Text = "一号机网络未连接";
            }
            //转台
            textBoxS1MRa.Text = deviceParams[0].mountParams.ra;
            textBoxS1MDec.Text = deviceParams[0].mountParams.dec;
            textBoxS1MAz.Text = deviceParams[0].mountParams.az;
            textBoxS1MAlt.Text = deviceParams[0].mountParams.alt;
            textBoxS1MDate.Text = deviceParams[0].mountParams.date;
            textBoxS1MUt.Text = deviceParams[0].mountParams.ut;
            textBoxS1MSt.Text = deviceParams[0].mountParams.st;
            //textBoxS1MMovStat.Text = deviceParams[0].mountParams.stat.ToString();
            switch (deviceParams[0].mountParams.stat)
            {
                case TS_Stopped:
                    textBoxS1MMovStat.Text = "Stopped";
                    break;
                case TS_Homing:
                    textBoxS1MMovStat.Text = "Homing...";
                    break;
                case TS_Homed:
                    textBoxS1MMovStat.Text = "Homed";
                    break;
                case TS_Slewing:
                    textBoxS1MMovStat.Text = "Slewing...";
                    break;
                case TS_Tracking:
                    textBoxS1MMovStat.Text = "Tracking...";
                    break;
                case TS_Parking:
                    textBoxS1MMovStat.Text = "Parking...";
                    break;
                case TS_Parked:
                    textBoxS1MMovStat.Text = "Parked";
                    break;
                default:
                    break;
            }
            //调焦器
            textBoxS1FCurPos.Text = deviceParams[0].focuserParams.pos.ToString();
            textBoxS1FCurTemp.Text = deviceParams[0].focuserParams.temp.ToString();
            if (deviceParams[0].focuserParams.isMoving==1)
            {
                textBoxS1FMovStat.ForeColor = Color.Red;
                textBoxS1FMovStat.Text = "moving...";
            }
            else
            {
                textBoxS1FMovStat.ForeColor = Color.Black;
                textBoxS1FMovStat.Text = "stopped";
            }
            
            //CCD
            textBoxS1CAmt.Text = deviceParams[0].ccdParams.imgAmt.ToString();
            textBoxS1CImgPath.Text = deviceParams[0].ccdParams.imgPath;
            textBoxS1CCurNum.Text = deviceParams[0].ccdParams.curNumb.ToString();
            textBoxS1CCurTemp.Text = deviceParams[0].ccdParams.temp.ToString();
            if (deviceParams[0].ccdParams.coolerSwitch == true)
            {
                textBoxS1CCoolerStat.ForeColor = Color.Red;
                textBoxS1CCoolerStat.Text = "ON";
            }
            else
            {
                textBoxS1CCoolerStat.ForeColor = Color.Black;
                textBoxS1CCoolerStat.Text = "OFF";
            }
            if (deviceParams[0].ccdParams.acqStat==1)
            {
                textBoxS1CAcqStat.ForeColor = Color.Red;
                textBoxS1CAcqStat.Text = "acquiring...";
            }
            else
            {
                textBoxS1CAcqStat.ForeColor = Color.Black;
                textBoxS1CAcqStat.Text = "stopped";
            }
            progressBarS1C.Value = (int)deviceParams[0].ccdParams.acqProc > 100 ? 100 : (int)deviceParams[0].ccdParams.acqProc;
            //滤光片转轮
            textBoxS1WCurPos.Text = (deviceParams[0].wheelParams.curPos + 1).ToString();
            if (deviceParams[0].wheelParams.movStatus==1)
            {
                textBoxS1WMovStat.ForeColor = Color.Red;
                textBoxS1WMovStat.Text = "moving...";
            }
            else
            {
                textBoxS1WMovStat.ForeColor = Color.Black;
                textBoxS1WMovStat.Text = "stopped";
            }
            textBoxS1WCurColor.Text = (deviceParams[0].wheelParams.curPos + 1).ToString();
        }

        private void UpdateFutStatusS2()
        {
            if (futMgr != null && futMgr.mDeviceConnections.ContainsKey("S2"))
            {
                labelS2NetStat.ForeColor = Color.Green;
                labelS2NetStat.Text = "二号机网络已连接";
            }
            else
            {
                labelS2NetStat.ForeColor = Color.Red;
                labelS2NetStat.Text = "二号机网络未连接";
            }
            //转台
            textBoxS2MRa.Text = deviceParams[1].mountParams.ra;
            textBoxS2MDec.Text = deviceParams[1].mountParams.dec;
            textBoxS2MAz.Text = deviceParams[1].mountParams.az;
            textBoxS2MAlt.Text = deviceParams[1].mountParams.alt;
            textBoxS2MDate.Text = deviceParams[1].mountParams.date;
            textBoxS2MUt.Text = deviceParams[1].mountParams.ut;
            textBoxS2MSt.Text = deviceParams[1].mountParams.st;
            //textBoxS2MMovStat.Text = deviceParams[1].mountParams.stat.ToString();
            switch (deviceParams[1].mountParams.stat)
            {
                case TS_Stopped:
                    textBoxS2MMovStat.Text = "Stopped";
                    break;
                case TS_Homing:
                    textBoxS2MMovStat.Text = "Homing...";
                    break;
                case TS_Homed:
                    textBoxS2MMovStat.Text = "Homed";
                    break;
                case TS_Slewing:
                    textBoxS2MMovStat.Text = "Slewing...";
                    break;
                case TS_Tracking:
                    textBoxS2MMovStat.Text = "Tracking...";
                    break;
                case TS_Parking:
                    textBoxS2MMovStat.Text = "Parking...";
                    break;
                case TS_Parked:
                    textBoxS2MMovStat.Text = "Parked";
                    break;
                default:
                    break;
            }
            //调焦器
            textBoxS2FCurPos.Text = deviceParams[1].focuserParams.pos.ToString();
            textBoxS2FCurTemp.Text = deviceParams[1].focuserParams.temp.ToString();
            if (deviceParams[1].focuserParams.isMoving == 1)
            {
                textBoxS2FMovStat.ForeColor = Color.Red;
                textBoxS2FMovStat.Text = "moving...";
            }
            else
            {
                textBoxS2FMovStat.ForeColor = Color.Black;
                textBoxS2FMovStat.Text = "stopped";
            }

            //CCD
            textBoxS2CAmt.Text = deviceParams[1].ccdParams.imgAmt.ToString();
            textBoxS2CImgPath.Text = deviceParams[1].ccdParams.imgPath;
            textBoxS2CCurNum.Text = deviceParams[1].ccdParams.curNumb.ToString();
            textBoxS2CCurTemp.Text = deviceParams[1].ccdParams.temp.ToString();
            if (deviceParams[1].ccdParams.coolerSwitch == true)
            {
                textBoxS2CCoolerStat.ForeColor = Color.Red;
                textBoxS2CCoolerStat.Text = "ON";
            }
            else
            {
                textBoxS2CCoolerStat.ForeColor = Color.Black;
                textBoxS2CCoolerStat.Text = "OFF";
            }
            if (deviceParams[1].ccdParams.acqStat == 1)
            {
                textBoxS2CAcqStat.ForeColor = Color.Red;
                textBoxS2CAcqStat.Text = "acquiring...";
            }
            else
            {
                textBoxS2CAcqStat.ForeColor = Color.Black;
                textBoxS2CAcqStat.Text = "stopped";
            }
            progressBarS2C.Value = (int)deviceParams[1].ccdParams.acqProc > 100 ? 100 : (int)deviceParams[1].ccdParams.acqProc;
            //滤光片转轮
            textBoxS2WCurPos.Text = (deviceParams[1].wheelParams.curPos + 1).ToString();
            if (deviceParams[1].wheelParams.movStatus == 1)
            {
                textBoxS2WMovStat.ForeColor = Color.Red;
                textBoxS2WMovStat.Text = "moving...";
            }
            else
            {
                textBoxS2WMovStat.ForeColor = Color.Black;
                textBoxS2WMovStat.Text = "stopped";
            }
            textBoxS2WCurColor.Text = (deviceParams[1].wheelParams.curPos + 1).ToString();
        }
    }
}
