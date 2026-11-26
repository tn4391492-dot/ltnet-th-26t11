using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DAL;
using DTO;

namespace BUS
{
    public class AtmBUS
    {
        private readonly AtmDAL _dal = new AtmDAL();

        public List<AtmStationDTO> GetAllStations()
        {
            return _dal.GetAllStations();
        }

        public bool SaveStation(AtmStationDTO station, bool isNew)
        {
            if (string.IsNullOrWhiteSpace(station.StationId) || string.IsNullOrWhiteSpace(station.Address))
            {
                throw new ArgumentException("Mã trạm và địa chỉ không được để trống.");
            }
            if (station.CurrentCash < 0)
            {
                throw new ArgumentException("Số tiền tồn quỹ không hợp lệ.");
            }
            return isNew ? _dal.InsertStation(station) : _dal.UpdateStation(station);
        }

        public List<ReconciliationResultDTO> GetReconciliationLogs()
        {
            return _dal.GetReconciliationLogs();
        }

        public async Task<ReconciliationResultDTO> RunEndOfDayReconciliationAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                const int totalSteps = 50;
                const int totalTransactions = 250000;
                var random = new Random();
                int discrepancyCount = 0;

                for (int step = 1; step <= totalSteps; step++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(100, cancellationToken);

                    if (random.Next(0, 100) < 6)
                    {
                        discrepancyCount++;
                    }

                    int percent = (step * 100) / totalSteps;
                    progress?.Report(percent);
                }

                var result = new ReconciliationResultDTO
                {
                    ReconcileTime = DateTime.Now,
                    TotalTransactions = totalTransactions,
                    MatchedCount = totalTransactions - discrepancyCount,
                    DiscrepancyCount = discrepancyCount,
                    ResultStatus = discrepancyCount == 0 ? "Khớp 100%" : $"Chênh lệch {discrepancyCount} GD"
                };

                _dal.InsertReconciliationLog(result);
                return result;
            }, cancellationToken);
        }
    }
}