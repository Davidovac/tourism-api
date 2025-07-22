namespace tourism_api.Domain
{
    public class ToursStatsData
    {
        public List<TourStats> MostReserved { get; set; } = new List<TourStats>();
        public List<TourStats> LeastReserved { get; set; } = new List<TourStats>();
        public List<TourStats> MostFilled { get; set; } = new List<TourStats>();
        public List<TourStats> LeastFilled { get; set; } = new List<TourStats>();
    }
}