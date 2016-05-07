using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Collections.Generic;

namespace tracert
{
    enum STATUS
    {
        WAIT = 0,       // Waiting
        TTL = 1,         // TTL expired
        FINISH = 2,     // Finish pinging
        TIMEOUT = 3, // Timed out
        UNKNOWN = 4    
    }

    class PingRun : IDisposable
    {
        Ping ping;
        public STATUS status  // Status of ICMP Packet
        {
            get;
            private set;
        }
        int n;  // TTL of this Ping run
        long startTime;  // Start Time
        long stopTime;  // Stop Time
        IPAddress currentIP;  // IP Address of Current Server
        PingOptions option;  // Ping Options
        byte[] data = new byte[0];  // ICMP Data

        public PingRun(int n)
        {
            this.n = n;
            status = STATUS.WAIT;
            ping = new Ping();
            ping.PingCompleted += Ping_PingCompleted;
            option = new PingOptions(n + 1, true);  // TTL = n + 1
        }

        public void Run(IPAddress ip, int timeout)
        {
            ping.SendAsync(ip, timeout, data, option, null);
            startTime = DateTime.Now.Ticks;
        }

        private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            stopTime = DateTime.Now.Ticks;
            if (e.Reply != null)
            {
                currentIP = e.Reply.Address;
                switch (e.Reply.Status)
                {
                    case IPStatus.Success:
                        status = STATUS.FINISH; break;
                    case IPStatus.TtlExpired:
                        status = STATUS.TTL; break;
                    case IPStatus.TimedOut:
                        status = STATUS.TIMEOUT; break;
                    default:
                        status = STATUS.UNKNOWN; break;
                }
            }
        }

        public void Dispose()
        {
            ping.Dispose();
        }

        public void Disp(out ResultSet result)
        {
            result = new ResultSet();
            if (status == STATUS.WAIT || status == STATUS.TIMEOUT)
            {
                /*
                Console.WriteLine("{0} {1} {2}", n + 1, "*", "*");
                */
                result.setFlag(false);
            }
            else
            {
                float nsec = stopTime - startTime;
                long msec = (long)nsec / 10000;
                /*
                Console.WriteLine("{0} {1} {2}", n + 1, msec, currentIP);
                */
                result.setFlag(true);
                result.setIP(currentIP.ToString());
                result.setTime(msec);
            }
        }
    }

    class PingAction : IDisposable
    {
        List<PingRun> array = new List<PingRun>();
        List<ResultSet> resultSet = new List<ResultSet>();
        const int Time_Wait_Other_Threads = 300;

        public PingAction(int hop)
        {
            for (int i = 0; i < hop; i++)
            {
                array.Add(new PingRun(i));
            }
        }

        public void Run(IPAddress ip, int timeout)
        {
            foreach (var p in array)
            {
                //Thread.Sleep(0);
                p.Run(ip, timeout);
            }
            while (!isFinish())
            {
                //Checks if it has finished
            }
        }

        private bool isFinish()
        {
            foreach (var p in array)  // Returns true when finished
            {
                if (p.status == STATUS.FINISH)
                {
                    Thread.Sleep(Time_Wait_Other_Threads);   // Slightly waits for other threads
                    return true;
                }
            }
            foreach (var p in array)  //Continues waiting with packets flagged WAIT left
            {
                if (p.status == STATUS.WAIT)
                {
                    return false;
                }
            }
            // Returns true with no packet flagged WAIT
            return true;
        }

        public void Dispose()
        {
            foreach (var p in array)
            {
                p.Dispose();
            }
        }

        public List<ResultSet> Disp()
        {
            foreach (var p in array)
            {
                ResultSet result = null;
                p.Disp(out result);
                resultSet.Add(result);
                if (p.status == STATUS.FINISH)
                    break;
            }
            /*
            Console.WriteLine("Finish!");
            */
            return resultSet;
        }
    }
}
