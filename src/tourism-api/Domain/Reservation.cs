namespace tourism_api.Domain
{
    public class Reservation
    {
        public int Id { get; set; }
        public int GuestsCount { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public Tour? Tour { get; set; }
    }
}
