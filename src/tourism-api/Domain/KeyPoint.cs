namespace tourism_api.Domain;
public class KeyPoint
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool IsValid()
    {
        return Order > 0 && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Description)
            && !string.IsNullOrWhiteSpace(ImageUrl);
    }
}

