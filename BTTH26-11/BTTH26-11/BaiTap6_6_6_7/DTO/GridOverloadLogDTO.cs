using System;

namespace DTO
{
    public class GridOverloadLogDTO
    {
        public int LogId { get; set; }
        public string SubstationName { get; set; }
        public DateTime RecordedTime { get; set; }
        public double VoltageKV { get; set; }
        public double LoadPercent { get; set; }
        public string AlertMessage { get; set; }
    }
}