namespace tourism_api.Domain
{
    public class RestaurantReservation
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string MealTime { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Restaurant? Restaurant { get; set; }
        public User? User { get; set; }
    }
}
