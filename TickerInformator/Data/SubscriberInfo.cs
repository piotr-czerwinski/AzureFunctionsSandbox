using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace  TickerInformator
{
    public class SubscriberInfo : TableEntity
    {
        public bool Active {get;set;}
        public int AlertTreshold { get; set; }
    }
}