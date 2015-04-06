//example of how to get time from a time server

using System;
using System.Windows.Forms;

namespace TimeServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SNTP SntpTime = new SNTP();
            SntpTime.TimeServer = "time.windows.com";
            SNTP.SNTPpacket time = SntpTime.SNTPRequest();

            //our recieved time stamp from the server, stored in a datetime variable
            DateTime rxTimeServer = time.ReceiveTimeStamp;

            //convert to unix time
            Int32 unixTimestamp = (Int32)(rxTimeServer.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            //time as rx from time server
            label1.Text = time.TransmitTimeStamp.ToString();

            // time from our unixtime stamp
            label4.Text = unixTimestamp.ToString();
            //Console.WriteLine(time.TransmitTimeStamp.ToString());
            //Console.WriteLine(time.RefId);
            //Console.WriteLine(time.Stratum.ToString());
        }
    }
}