using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotVal
{
    class Client
    {
        public long ClientId;
        public decimal Interval;
        public decimal CurrentInterval;
        public bool IsUSD;
        public bool IsEUR;
        public bool IsRUB;
        public bool IsBTC;
        public string word;
        public Client(long ClientId)
        {
            this.ClientId = ClientId;
            this.Interval = 60000;
            this.CurrentInterval = 60000;
            this.IsUSD=true;
            this.IsEUR = true;
            this.IsRUB = true;
            this.IsBTC = true;
            this.word = "Hello";
        }

        public Client(long ClientId, decimal Interval)
        {
            this.ClientId = ClientId;
            this.Interval = Interval;
            this.CurrentInterval = Interval;
            this.IsUSD = true;
            this.IsEUR = true;
            this.IsRUB = true;
            this.IsBTC = true;
            this.word = "Hello";
        }


        public void ChangeInterval(decimal Interval)
        {
            this.Interval = Interval;
            this.CurrentInterval = Interval;
        }

        public void PingInterval()
        {
            CurrentInterval = CurrentInterval - 10000;
        }
        public void ResetI()
        {
            CurrentInterval = Interval;
        }
    }
}
