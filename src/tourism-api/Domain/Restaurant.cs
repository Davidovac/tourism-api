﻿namespace tourism_api.Domain;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Status { get; set; } = "u pripremi";
    public User? Owner { get; set; }
    public int OwnerId { get; set; }
    public List<Meal> Meals { get; set; } = new List<Meal>();

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Description)
            && Capacity > 0
            && ImageUrls != null
            && ImageUrls.Count > 0
            && ImageUrls.All(url => !string.IsNullOrWhiteSpace(url));
    }
}
