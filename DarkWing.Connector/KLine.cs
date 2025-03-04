using System;
using DarkWing.Common.Util;

namespace DarkWing.Connector
{
    public class KLine
    {
        public long StartTime { get; init; }
        public long CloseTime { get; init; }

        public decimal Open { get; init; }
        public decimal Close { get; init; }
        public decimal Min { get; init; }
        public decimal Max { get; init; }

        internal DateTime StartTimeDateTime =>
            StartTime.ToDateTime();
        
        internal DateTime CloseTimeDateTime =>
            CloseTime.ToDateTime();

        public override string ToString()
        {
            return $"{StartTimeDateTime:g} - {CloseTimeDateTime:g}";
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}