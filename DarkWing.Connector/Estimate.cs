using System;
using DarkWing.Common.Util;

namespace DarkWing.Connector
{
    public struct Estimate
    {
        public long Time {  get; set; }
        public decimal Value {  get; set; }

        internal DateTime DateTime =>
            Time.ToDateTime();
    }
}
