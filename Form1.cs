using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace tracert
{
    public partial class Form1 : Form
    {
        int hop;
        int timeout;
        string target;
        string targetIP;

        string dataPath = "qqwry.dat";
        Tracert tracert;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tracert = new Tracert();
        }

        private List<ResultSet> tracerouce(IPAddress ip, int hop, int timeout)
        {
            var task = Task.Run(() =>
            {
                return tracert.Run(ip, hop, timeout);
            });
            return task.Result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button3.Enabled = false;
            label5.Text = "Ready.";
            routeList.Items.Clear();

            hop = int.Parse(textBox3.Text);
            timeout = int.Parse(textBox2.Text);
            //Get domain/IP from the text
            target = textBox1.Text;
            target = target.Trim();
            if (target.IndexOf("://") >= 0)
                target = target.Substring(target.IndexOf("://") + 3);
            if (target.IndexOf("/") >= 0)
                target = target.Substring(0, target.IndexOf("/"));
            if (target.IndexOf(":") >= 0)
                target = target.Substring(0, target.IndexOf(":"));
            //  -------------------------


            IPAddress ipaddress = GetIPAddr.GetIPAddress(target);
            if(ipaddress == null)
            {
                MessageBox.Show("Invalid Hostname or IP Address!", "Error");
                button1.Enabled = true;
                return;
            }
            targetIP = ipaddress.ToString();
            label5.Text = targetIP;

            List<ResultSet> resultSet = tracerouce(ipaddress, hop, timeout);
            int n = 0;
            string ip;
            long time;
            string location;
            foreach (var rs in resultSet)
            {
                n++;
                ListViewItem item = routeList.Items.Add(n.ToString());
                if (rs.getFlag())
                {
                    ip = rs.getIP();
                    time = rs.getTime();
                    location = IPAddrQry.IPLocate(dataPath, ip);

                    item.SubItems.Add(time.ToString());
                    item.SubItems.Add(ip);
                    item.SubItems.Add(location);
                }
                else
                {
                    item.SubItems.Add("*");
                    item.SubItems.Add("*");
                    item.SubItems.Add("*");
                }
            }
            button1.Enabled = true;
            button3.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PressEnter(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    if (DialogResult.OK == MessageBox.Show("Are you sure to exit?", "RouceTrace", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                    {
                        Close();
                    }
                    break;
                case Keys.Enter:
                    if (button2.Focused == false && button3.Focused == false)
                    {
                        button1_Click(sender, e);
                        return;
                    }
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (StreamWriter file = new StreamWriter("TraceRoute.txt"))
            {
                file.WriteLine("tracert  {0} [{1}] :", target, targetIP);
                foreach (ListViewItem item in routeList.Items)
                {
                    ListViewItem.ListViewSubItem subitem = item.SubItems[1];
                    if (subitem.Text == "*")
                    {
                        file.WriteLine(" {0,3}\t{1} \t{2, -25} \t{3}", item.Text, "*", "*", "Request timed out.");
                    }
                    else
                    {
                        file.WriteLine(" {0,3}\t{1} \t{2, -25} \t{3}", item.Text, item.SubItems[1].Text, item.SubItems[2].Text, item.SubItems[3].Text);
                    }
                }
                file.WriteLine();
                file.WriteLine(" ---- Generated with tracert developed by Makazeu ----");
            }

            ProcessStartInfo start = new ProcessStartInfo("notepad.exe");
            start.Arguments = "TraceRoute.txt";
            Process process = Process.Start(start);
            process.Close();
        }

    }
}
