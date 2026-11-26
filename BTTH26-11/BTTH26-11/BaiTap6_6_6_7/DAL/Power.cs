using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class PowerGridDAL
    {
        public List<SubstationDTO> GetAllSubstations()
        {
            var list = new List<SubstationDTO>();
            string query = "SELECT SubstationId, SubstationName FROM Substation";

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SubstationDTO
                        {
                            SubstationId = reader["SubstationId"].ToString(),
                            SubstationName = reader["SubstationName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public void InsertOverloadLog(GridOverloadLogDTO log)
        {
            string query = "INSERT INTO GridOverloadLog (SubstationName, RecordedTime, VoltageKV, LoadPercent, AlertMessage) VALUES (@Name, @Time, @Voltage, @Load, @Alert)";
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", log.SubstationName);
                cmd.Parameters.AddWithValue("@Time", log.RecordedTime);
                cmd.Parameters.AddWithValue("@Voltage", log.VoltageKV);
                cmd.Parameters.AddWithValue("@Load", log.LoadPercent);
                cmd.Parameters.AddWithValue("@Alert", log.AlertMessage);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}