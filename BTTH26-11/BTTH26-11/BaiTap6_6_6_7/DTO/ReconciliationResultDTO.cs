using System;

namespace DTO
{
    public class ReconciliationResultDTO
    {
        public int LogId { get; set; }
        public DateTime ReconcileTime { get; set; }
        public int TotalTransactions { get; set; }
        public int MatchedCount { get; set; }
        public int DiscrepancyCount { get; set; }
        public string ResultStatus { get; set; }
    }
}