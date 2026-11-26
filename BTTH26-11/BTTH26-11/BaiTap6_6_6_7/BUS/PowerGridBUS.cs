using System.Collections.Generic;
using DAL;
using DTO;

namespace BUS
{
    public class PowerGridBUS
    {
        private readonly PowerGridDAL _dal = new PowerGridDAL();

        public List<SubstationDTO> GetSubstations()
        {
            return _dal.GetAllSubstations();
        }

        public bool IsOverloaded(double loadPercent)
        {
            return loadPercent > 85.0;
        }

        public void RecordOverload(GridOverloadLogDTO log)
        {
            _dal.InsertOverloadLog(log);
        }
    }
}