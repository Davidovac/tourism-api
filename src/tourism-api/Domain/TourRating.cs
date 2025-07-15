namespace tourism_api.Domain;

public class TourRating
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }

    public bool IsValid()
    {
        if (TourId <= 0 || UserId <= 0 || Rating < 1 || Rating > 5)
        {
            return false;
        }
        return true;
    }
}