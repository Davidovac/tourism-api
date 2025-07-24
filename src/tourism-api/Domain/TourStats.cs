namespace tourism_api.Domain
{
    public class TourStats
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MaxGuests { get; set; }
        public int? ReservationsSum { get; set; }
        public double ReservationRate { get; set; }
    }
}
