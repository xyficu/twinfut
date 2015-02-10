using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Npgsql;
using System.Net.Sockets;


namespace twin_futs
{
    public partial class FrmObserve : Form
    {
        private List<OT> otList;
        private List<GRB> grbList;
        private List<ObsTar> obsTarList;
        FutPgsql futSqlUser;
        Thread updateDBTh;
        int updateFreq;
        System.Timers.Timer tmrUpdateListView;
        private FutMgrNet futMgr;
        FormFuts parent;
        List<ListViewItem> targetList;
        Thread obsThd;
        DeviceParams[] deviceParams;
        public FrmObserve(FormFuts _parent)
        {
            InitializeComponent();

            otList = _parent.otList;
            grbList = _parent.grbList;
            obsTarList = _parent.obsTarList;
            futSqlUser = _parent.futSqlUser;
            futMgr = _parent.futMgr;
            deviceParams = _parent.deviceParams;

            targetList = new List<ListViewItem>();

            //更新数据库线程
            updateDBTh = new Thread(new ThreadStart(UpdateDB));
            updateFreq = 1000;
            updateDBTh.IsBackground = true;
            updateDBTh.Start();


            tmrUpdateListView = new System.Timers.Timer(3000);
            tmrUpdateListView.Elapsed += new System.Timers.ElapsedEventHandler(DispOtFromDB);
            tmrUpdateListView.AutoReset = true;
            tmrUpdateListView.Enabled = true;  
            //listview格式
            listView1.Columns.Add("序号", 40, HorizontalAlignment.Left);
            listView1.Columns.Add("ID", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("RA", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("DEC", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("星等", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("观测状态", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("添加时间", 140, HorizontalAlignment.Left);
            listView1.FullRowSelect = true;
            listView1.View = View.Details;
            listView1.GridLines = true;
        }

        private void observe_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        //从数据库中将OT列表取出并显示
        private void DispOtFromDB(object o, System.Timers.ElapsedEventArgs e)
        {
            UpdateLV();
            
        }

        private delegate void DegeUpdateLV();
        private void UpdateLV()
        {
            if (this.listView1.InvokeRequired)
            {
                while (!this.listView1.IsHandleCreated)
                {
                    if (this.listView1.Disposing || this.listView1.IsDisposed)
                    {
                        return;
                    }
                }
                DegeUpdateLV d = new DegeUpdateLV(UpdateLV);
                this.listView1.Invoke(d);
            }
            else
            {
                try
                {
                    string cmdStr = "select * from ot order by add_time asc";
                    DataSet ds = futSqlUser.DatasetFill(cmdStr);

                    listView1.BeginUpdate();
                    for (; 0 < listView1.Items.Count; )
                    {
                        listView1.Items.RemoveAt(0);
                    }

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ListViewItem record = new ListViewItem((listView1.Items.Count + 1).ToString());
                        record.SubItems.Add(dr["id"].ToString());
                        record.SubItems.Add(dr["ra"].ToString());
                        record.SubItems.Add(dr["dec"].ToString());
                        record.SubItems.Add(dr["mag"].ToString());
                        record.SubItems.Add(int.Parse(dr["status"].ToString())==1?"已观测":"未观测");
                        record.SubItems.Add(dr["add_time"].ToString());
                        listView1.Items.Add(record);
                    }
                    listView1.EndUpdate();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        //更新listview
        //private delegate void DegeUpdateLV(ListViewItem record);
        //private void UpdateLV(ListViewItem record)
        //{
        //    if (this.listView1.InvokeRequired)
        //    {
        //        while (!this.listView1.IsHandleCreated)
        //        {
        //            if (this.listView1.Disposing||this.listView1.IsDisposed)
        //            {
        //                return;
        //            }
        //        }
        //        DegeUpdateLV d = new DegeUpdateLV(UpdateLV);
        //        this.listView1.Invoke(d, new object[] { record });
        //    }
        //    else
        //    {
        //        this.listView1.Items.Add(record);
        //    }
        //}

        //清空ListView
        //private delegate void DeleRemoveLVRec();
        //private void ClearLVRec()
        //{
        //    if (this.listView1.InvokeRequired)
        //    {
        //        while (!this.listView1.IsHandleCreated)
        //        {
        //            if (this.listView1.Disposing || this.listView1.IsDisposed)
        //            {
        //                return;
        //            }
        //        }
        //        DeleRemoveLVRec d = new DeleRemoveLVRec(ClearLVRec);
        //        this.listView1.Invoke(d);
        //    }
        //    else
        //    {
        //        for (; 0 < listView1.Items.Count; )
        //        {
        //            listView1.Items.RemoveAt(0);
        //        }
        //    }
        //}


        //将OT，GRB存入数据库
        //将已观测列表更新入数据库
        private void UpdateDB()
        {
            while (true)
            {
                //插入OT
                foreach (OT ot in otList)
                {
                    ot.addTime = GetLocalTime();
                    futSqlUser.AddOT(ot);
                }
                //更新OT状态
                foreach (ObsTar obsTar in obsTarList)
                {
                    futSqlUser.UpdateOTStat(obsTar.id, obsTar.type == 0 ? 1 : 0);
                }
                //插入GRB
                //更新GRB状态


                Thread.Sleep(updateFreq);
            }
            
            
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

        private void InitFut(int futId)
        {
            string fut= futId==0?"S1":"S2";
            Socket value;
            try
            {
                if (futMgr != null && futMgr.mDeviceConnections.TryGetValue(fut, out value))
                {
                    string msg = (futId == 0 ? "T1" : "T2") + ",INIT," + GetLocalTime();
                    byte[] buf = Encoding.ASCII.GetBytes(msg);
                    value.Send(buf);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            
        }

        private void buttonInitFuts_Click(object sender, EventArgs e)
        {
            InitFut(0);
            InitFut(1);
        }

        string ra = "", dec = "";
        int color = 0, id = 0;
        double time = 0.0;
        int amount = 1;
        string fileName = "";
        int futId;
        private void buttonStartObs_Click(object sender, EventArgs e)
        {
            futId = 0;

            //存储当前所有未观测目标
            targetList.Clear();
            foreach (ListViewItem item in listView1.Items)
            {
                string status = item.SubItems[5].Text;
                if ( status== "未观测")
                {
                    targetList.Add(item);
                }
            }

            obsThd = new Thread(new ThreadStart(ObsTarget));
            obsThd.IsBackground = true;
            obsThd.Start();
            
        }

        private void ObsTarget()
        {
            string deviceType = futId == 0 ? "T1" : "T2";
            try
            {

                foreach (ListViewItem item in targetList)
                {

                    //当转台忙的时候等待1秒
                    while (deviceParams[0].teleStat == 1)
                    {
                        Thread.Sleep(1000);
                    }
                    //从list中取出一个源，赋值给变量
                    id = int.Parse(item.SubItems[1].Text);
                    ra = item.SubItems[2].Text;
                    dec = item.SubItems[3].Text;
                    color = 9;
                    time = 5;
                    amount = 2;
                    Socket value;
                    fileName = id + "_" + ra + "_" + dec;

                    string msg = deviceType + ",OBS," + id.ToString() + "," + ra + "," + dec + "," +
                                 color.ToString() + "," + time.ToString() + "," + amount.ToString() + "," +
                                 fileName + "," + GetLocalTime();
                    byte[] buf = Encoding.ASCII.GetBytes(msg);
                    if (futMgr != null && futMgr.mDeviceConnections.TryGetValue(futId == 0 ? "S1" : "S2", out value))
                    {
                        value.Send(buf);
                    }
                    //等待命令发送望远镜状态变化
                    Thread.Sleep(1000);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
