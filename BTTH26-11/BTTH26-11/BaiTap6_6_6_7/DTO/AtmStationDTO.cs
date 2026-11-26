namespace DTO
{
    public class AtmStationDTO
    {
        public string StationId { get; set; }
        public string Address { get; set; }
        public decimal CurrentCash { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
    }
}