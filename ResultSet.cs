using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tracert
{
    class ResultSet
    {
        private string ip;
        private bool flag; // true: has response   false: no response
        private long time;

        public void setIP(string ip)
        {
            this.ip = ip;
        }
        public void setFlag(bool flag)
        {
            this.flag = flag;
        }
        public void setTime(long time)
        {
            this.time = time;
        }
        public string getIP()
        {
            return ip;
        }
        public bool getFlag()
        {
            return flag;
        }
        public long getTime()
        {
            return time;
        }
    }
}
