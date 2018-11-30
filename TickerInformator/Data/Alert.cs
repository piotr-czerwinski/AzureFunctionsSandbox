using System;

namespace TickerInformator
{
    public class Alert
    {
        public String Addressee {get;set;}
        public decimal LastHourChange {get;set;}
        public decimal LastDayChange {get;set; }
        public decimal CurrentPrice { get; set; }
    }
}