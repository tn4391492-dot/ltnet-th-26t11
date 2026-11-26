using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class AtmDAL
    {
        public List<AtmStationDTO> GetAllStations()
        {
            var list = new List<AtmStationDTO>();
            string query = "SELECT StationId, Address, CurrentCash, Status, ISNULL(ImagePath, '') AS ImagePath FROM AtmStation";

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AtmStationDTO
                        {
                            StationId = reader["StationId"].ToString(),
                            Address = reader["Address"].ToString(),
                            CurrentCash = Convert.ToDecimal(reader["CurrentCash"]),
                            Status = reader["Status"].ToString(),
                            ImagePath = reader["ImagePath"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public bool InsertStation(AtmStationDTO station)
        {
            string query = "INSERT INTO AtmStation (StationId, Address, CurrentCash, Status, ImagePath) VALUES (@Id, @Address, @Cash, @Status, @ImagePath)";
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", station.StationId);
                cmd.Parameters.AddWithValue("@Address", station.Address);
                cmd.Parameters.AddWithValue("@Cash", station.CurrentCash);
                cmd.Parameters.AddWithValue("@Status", station.Status);
                cmd.Parameters.AddWithValue("@ImagePath", station.ImagePath ?? string.Empty);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateStation(AtmStationDTO station)
        {
            string query = "UPDATE AtmStation SET Address = @Address, CurrentCash = @Cash, Status = @Status, ImagePath = @ImagePath WHERE StationId = @Id";
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", station.StationId);
                cmd.Parameters.AddWithValue("@Address", station.Address);
                cmd.Parameters.AddWithValue("@Cash", station.CurrentCash);
                cmd.Parameters.AddWithValue("@Status", station.Status);
                cmd.Parameters.AddWithValue("@ImagePath", station.ImagePath ?? string.Empty);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public void InsertReconciliationLog(ReconciliationResultDTO log)
        {
            string query = "INSERT INTO AtmReconciliationLog (ReconcileTime, TotalTransactions, MatchedCount, DiscrepancyCount, ResultStatus) VALUES (@Time, @Total, @Matched, @Discrepancy, @Status)";
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Time", log.ReconcileTime);
                cmd.Parameters.AddWithValue("@Total", log.TotalTransactions);
                cmd.Parameters.AddWithValue("@Matched", log.MatchedCount);
                cmd.Parameters.AddWithValue("@Discrepancy", log.DiscrepancyCount);
                cmd.Parameters.AddWithValue("@Status", log.ResultStatus);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<ReconciliationResultDTO> GetReconciliationLogs()
        {
            var list = new List<ReconciliationResultDTO>();
            string query = "SELECT TOP 20 LogId, ReconcileTime, TotalTransactions, MatchedCount, DiscrepancyCount, ResultStatus FROM AtmReconciliationLog ORDER BY LogId DESC";

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ReconciliationResultDTO
                        {
                            LogId = Convert.ToInt32(reader["LogId"]),
                            ReconcileTime = Convert.ToDateTime(reader["ReconcileTime"]),
                            TotalTransactions = Convert.ToInt32(reader["TotalTransactions"]),
                            MatchedCount = Convert.ToInt32(reader["MatchedCount"]),
                            DiscrepancyCount = Convert.ToInt32(reader["DiscrepancyCount"]),
                            ResultStatus = reader["ResultStatus"].ToString()
                        });
                    }
                }
            }
            return list;
        }
    }
}