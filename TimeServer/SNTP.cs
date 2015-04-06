using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TimeServer
{
    internal class SNTP
    {
        public class SNTPpacket
        {
            public byte LI { get; set; }

            public byte VN { get; set; }

            public byte Mode { get; set; }

            public byte Stratum { get; set; }

            public byte Poll { get; set; }

            public byte Precision { get; set; }

            public Int32 RootDelay { get; set; }

            public Int32 RootDispersion { get; set; }

            public string RefId { get; set; }

            public DateTime RefTimeStamp { get; set; }

            public DateTime OriginateTimeStamp { get; set; }

            public DateTime ReceiveTimeStamp { get; set; }

            public DateTime TransmitTimeStamp { get; set; }

            public byte[] buffer { get; set; }

            public SNTPpacket()
            {
                buffer = new byte[48];
            }

            public void ConvertToBuffer()
            {
                buffer[0] = (byte)(((LI & 0x3) << 6) | ((VN & 0x3) << 3) | Mode & 0x07);
                buffer[1] = Stratum;
                buffer[2] = Poll;
                buffer[3] = Precision;
                Int64 SNTPTime = ConvertToSNTPTime(RefTimeStamp);
                LoadSNTPTime(SNTPTime, 24);
            }

            public void ConvertFromBuffer()
            {
                Mode = (byte)(buffer[0] & 0x7);

                VN = (byte)((buffer[0] >> 3) & 0x7);
                LI = (byte)((buffer[0] >> 6) & 0x3);
                Stratum = buffer[1];
                Poll = buffer[2];
                Precision = buffer[3];
                RootDelay = convertInt32(4);
                RootDispersion = convertInt32(8);
                RefId = Encoding.ASCII.GetString(buffer, 12, 4);
                RefTimeStamp = ConvertFromSNTPTime(16);
                OriginateTimeStamp = ConvertFromSNTPTime(24);
                ReceiveTimeStamp = ConvertFromSNTPTime(32);
                TransmitTimeStamp = ConvertFromSNTPTime(40);
            }

            private Int64 ConvertToSNTPTime(DateTime T)
            {
                return T.Subtract(new DateTime(1900, 1, 1)).Ticks / TimeSpan.TicksPerSecond;
            }

            private void LoadSNTPTime(Int64 T, int start)
            {
                byte[] temp = BitConverter.GetBytes(T);
                for (int i = 0; i < 4; i++)
                {
                    buffer[start + 3 - i] = temp[i];
                }
            }

            private Int32 convertInt32(int start)
            {
                byte[] temp = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    temp[i] = buffer[start + 3 - i];
                }
                return BitConverter.ToInt32(temp, 0);
            }

            private DateTime ConvertFromSNTPTime(int start)
            {
                byte[] tempHigh = new byte[4];
                byte[] tempLow = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    tempHigh[i] = buffer[start + 3 - i];
                    tempLow[i] = buffer[start + 4 + 3 - i];
                }

                UInt32 tempTime = BitConverter.ToUInt32(tempHigh, 0);
                UInt32 tempFrac = BitConverter.ToUInt32(tempLow, 0);
                double SNTPTime = tempTime + ((double)tempFrac) / ((double)UInt32.MaxValue + 1);
                return new DateTime(1900, 1, 1).AddSeconds(SNTPTime); ;
            }
        }

        public string TimeServer { get; set; }

        private UdpClient UDP = new UdpClient();

        public SNTPpacket SNTPRequest()
        {
            SNTPpacket request = new SNTPpacket();
            request.VN = 1;
            request.Mode = 3;
            request.RefTimeStamp = DateTime.UtcNow;
            request.ConvertToBuffer();
            UDP.Connect(TimeServer, 123);
            int count = UDP.Send(request.buffer, request.buffer.Length);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            SNTPpacket reply = new SNTPpacket();
            reply.buffer = UDP.Receive(ref remoteEP);
            reply.ConvertFromBuffer();

            return reply;
        }
    }
}