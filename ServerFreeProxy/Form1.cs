using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ServerFreeProxy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            MyServer myServer = new MyServer();
            myServer.onReport += new MyServer.receiveReportDelegate(onGetReport);
            Thread th = new Thread(myServer.run);
            th.IsBackground = true;
            th.Start();
        }

        private void onGetReport(string report)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke((Action)delegate()
                {
                    string[] str = report.Split('|');
                    ListViewItem tmpItem = null; 
                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (str[0].Equals(item.SubItems[1].Text))
                        {
                            item.Remove();
                            break;
                        }
                    }
                    if (tmpItem == null)
                    {
                        tmpItem = new ListViewItem();
                        tmpItem.SubItems.Add(str[0]);
                        tmpItem.SubItems.Add(str[1]);
                        tmpItem.SubItems.Add(str[2]);
                        listView1.Items.Add(tmpItem);
                    }
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        listView1.Items[i].SubItems[0].Text = (i + 1).ToString();
                    }
                });
                
            }
        }

        class MyServer
        {
            private UdpClient _server;

            public MyServer()
            {
                _server = new UdpClient(3000);
            }

            public delegate void receiveReportDelegate(string report);
            public event receiveReportDelegate onReport;

            public void run()
            {
                while(true)
                {
                    IPEndPoint receivePoint = new IPEndPoint(IPAddress.Any, 3000);
                    byte[] recData = _server.Receive(ref receivePoint);
                    //Console.WriteLine(receivePoint.Address.ToString() + Encoding.ASCII.GetString(recData));
                    string report = Encoding.ASCII.GetString(recData);
                    if (report.Contains("[FreeProxy]"))
                    {
                        if (onReport != null)
                        {
                            onReport(receivePoint.Address.ToString() + "|" + DateTime.Now.ToString() + "|" + report.Replace("[FreeProxy]", ""));
                        }
                    }
                }
            }
        }

    }
}
