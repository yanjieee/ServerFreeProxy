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
using System.IO;

namespace ServerFreeProxy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listView1.Items.Clear();
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
                    saveListView2HtmlTable(listView1);
                });
                
            }
        }

        private void saveListView2HtmlTable(ListView listview)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("<html><head><title>Reports{0}</title></head><body><table border=\"1\"><thead>", DateTime.Now.ToString()));
            foreach (ColumnHeader ch in listview.Columns)
            {
                sb.Append(string.Format("<th> {0} </th>", ch.Text));
            }
            sb.Append("</thead>");
            foreach (ListViewItem item in listview.Items)
            {
                sb.Append("<tr>");
                foreach(System.Windows.Forms.ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    sb.Append("<td>");
                    sb.Append(subitem.Text);
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("<tbody></body>");
            FileStream fs = new FileStream("c:\\inetpub\\wwwroot\\a.html", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            sw.Write(sb.ToString());
            sw.Close();
            fs.Close();
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
