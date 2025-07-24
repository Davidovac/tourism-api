namespace tourism_api.Domain;

public class Tour
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime? DateTime { get; set; }
    public int MaxGuests { get; set; }
    public string? Status { get; set; } = "u pripremi";
    public User? Guide { get; set; }
    public int GuideId { get; set; }
    public List<KeyPoint> KeyPoints { get; set; } = new List<KeyPoint>();
    public List<TourRating> Ratings { get; set; } = new List<TourRating>();

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Description) && MaxGuests > 0;
    }
}
