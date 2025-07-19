using Microsoft.Data.Sqlite;
using tourism_api.Domain;
using System.Globalization;

namespace tourism_api.Repositories
{
    public class RestaurantReservationRepository
    {

        private readonly string _connectionString;

        public RestaurantReservationRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString:SQLiteConnection"];
        }


        public (bool isAvailable, int? maxAvailable) CheckReservationAvailability(int restaurantId, DateTime date, string mealTime, int numberOfPeople)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string capacityQuery = "SELECT Capacity FROM Restaurants WHERE Id = @RestaurantId";
                using SqliteCommand capacityCommand = new SqliteCommand(capacityQuery, connection);
                capacityCommand.Parameters.AddWithValue("@RestaurantId", restaurantId);
                int capacity = Convert.ToInt32(capacityCommand.ExecuteScalar());

                string reservedQuery = @"
                SELECT COALESCE(SUM(NumberOfPeople), 0) 
                FROM RestaurantReservations 
                WHERE RestaurantId = @RestaurantId
                AND Date = @Date 
                AND MealTime = @MealTime";

                using SqliteCommand reservedCommand = new SqliteCommand(reservedQuery, connection);
                reservedCommand.Parameters.AddWithValue("@RestaurantId", restaurantId);
                reservedCommand.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                reservedCommand.Parameters.AddWithValue("@MealTime", mealTime);

                int reservedSeats = Convert.ToInt32(reservedCommand.ExecuteScalar());
                int availableSeats = capacity - reservedSeats;

                return (availableSeats >= numberOfPeople, availableSeats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking reservation availability: {ex.Message}");
                return (false, null);
            }
        }

        public RestaurantReservation Create(RestaurantReservation reservation)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                INSERT INTO RestaurantReservations (RestaurantId, UserId, Date, MealTime, NumberOfPeople, CreatedAt)
                VALUES (@RestaurantId, @UserId, @Date, @MealTime, @NumberOfPeople, @CreatedAt);
                SELECT last_insert_rowid();";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@RestaurantId", reservation.RestaurantId);
                command.Parameters.AddWithValue("@UserId", reservation.UserId);
                command.Parameters.AddWithValue("@Date", reservation.Date.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@MealTime", reservation.MealTime);
                command.Parameters.AddWithValue("@NumberOfPeople", reservation.NumberOfPeople);
                command.Parameters.AddWithValue("@CreatedAt", reservation.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

                reservation.Id = Convert.ToInt32(command.ExecuteScalar());
                return reservation;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating reservation: {ex.Message}");
                throw;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = "DELETE FROM RestaurantReservations WHERE Id = @Id";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting reservation: {ex.Message}");
                throw;
            }
        }

        public RestaurantReservation? GetById(int id)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                SELECT r.*, u.Username, rest.Name AS RestaurantName
                FROM RestaurantReservations r
                INNER JOIN Users u ON r.UserId = u.Id
                INNER JOIN Restaurants rest ON r.RestaurantId = rest.Id
                WHERE r.Id = @Id";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new RestaurantReservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        RestaurantId = Convert.ToInt32(reader["RestaurantId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Date = DateTime.Parse(reader["Date"].ToString()),
                        MealTime = reader["MealTime"].ToString(),
                        NumberOfPeople = Convert.ToInt32(reader["NumberOfPeople"]),
                        CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                        User = new User { Username = reader["Username"].ToString() },
                        Restaurant = new Restaurant { Name = reader["RestaurantName"].ToString() }
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reservation: {ex.Message}");
                throw;
            }
        }

        public List<RestaurantReservation> GetByRestaurant(int restaurantId, DateTime? date = null)
        {
            List<RestaurantReservation> reservations = new List<RestaurantReservation>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                SELECT r.*, u.Username, rest.Name AS RestaurantName
                FROM RestaurantReservations r
                INNER JOIN Users u ON r.UserId = u.Id
                INNER JOIN Restaurants rest ON r.RestaurantId = rest.Id
                WHERE r.RestaurantId = @RestaurantId";

                if (date.HasValue)
                {
                    query += " AND r.Date = @Date";
                }

                query += " ORDER BY r.Date, r.MealTime";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@RestaurantId", restaurantId);

                if (date.HasValue)
                {
                    command.Parameters.AddWithValue("@Date", date.Value.ToString("yyyy-MM-dd"));
                }

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    RestaurantReservation reservation = new RestaurantReservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        RestaurantId = Convert.ToInt32(reader["RestaurantId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Date = DateTime.Parse(reader["Date"].ToString()),
                        MealTime = reader["MealTime"].ToString(),
                        NumberOfPeople = Convert.ToInt32(reader["NumberOfPeople"]),
                        CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                        User = new User { Username = reader["Username"].ToString() },
                        Restaurant = new Restaurant { Name = reader["RestaurantName"].ToString() }
                    };
                    reservations.Add(reservation);
                }

                return reservations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reservations: {ex.Message}");
                throw;
            }
        }

        public List<RestaurantReservation> GetByUser(int userId)
        {
            List<RestaurantReservation> reservations = new List<RestaurantReservation>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                SELECT r.*, u.Username, rest.Name AS RestaurantName
                FROM RestaurantReservations r
                INNER JOIN Users u ON r.UserId = u.Id
                INNER JOIN Restaurants rest ON r.RestaurantId = rest.Id
                WHERE r.UserId = @UserId
                ORDER BY r.Date DESC, r.MealTime";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    RestaurantReservation reservation = new RestaurantReservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        RestaurantId = Convert.ToInt32(reader["RestaurantId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Date = DateTime.Parse(reader["Date"].ToString()),
                        MealTime = reader["MealTime"].ToString(),
                        NumberOfPeople = Convert.ToInt32(reader["NumberOfPeople"]),
                        CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                        User = new User { Username = reader["Username"].ToString() },
                        Restaurant = new Restaurant { Name = reader["RestaurantName"].ToString() }
                    };
                    reservations.Add(reservation);
                }

                return reservations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user reservations: {ex.Message}");
                throw;
            }
        }

        public int GetTotalReservationsThisYear(int restaurantId, int ownerId)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                DateTime startOfYear = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime endOfYear = new DateTime(DateTime.Now.Year, 12, 31);

                string query = @"
                SELECT COUNT(*)
                FROM RestaurantReservations rr
                INNER JOIN Restaurants r ON rr.RestaurantId = r.Id
                WHERE rr.RestaurantId = @RestaurantId
                AND r.OwnerId = @OwnerId
                AND rr.Date >= @StartOfYear AND rr.Date <= @EndOfYear";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@RestaurantId", restaurantId);
                command.Parameters.AddWithValue("@OwnerId", ownerId);
                command.Parameters.AddWithValue("@StartOfYear", startOfYear.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@EndOfYear", endOfYear.ToString("yyyy-MM-dd"));

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting total reservations: {ex.Message}");
                throw;
            }
        }

        public Dictionary<string, double> GetMonthlyPercentage(int restaurantId, int ownerId, int capacity)
        {
            Dictionary<int, double> monthlyPercentages = new Dictionary<int, double>();

            try
            {
                using (SqliteConnection connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    DateTime now = DateTime.Now;
                    DateTime startOfYear = new DateTime(now.Year, 1, 1);
                    DateTime endOfYear = new DateTime(now.Year, 12, 31);

                    string query = @"
                    SELECT rr.Date, rr.NumberOfPeople
                    FROM RestaurantReservations rr
                    INNER JOIN Restaurants r ON rr.RestaurantId = r.Id
                    WHERE rr.RestaurantId = @RestaurantId
                    AND r.OwnerId = @OwnerId
                    AND rr.Date >= @StartOfYear AND rr.Date <= @EndOfYear";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RestaurantId", restaurantId);
                        command.Parameters.AddWithValue("@OwnerId", ownerId);
                        command.Parameters.AddWithValue("@StartOfYear", startOfYear.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@EndOfYear", endOfYear.ToString("yyyy-MM-dd"));

                        Dictionary<int, int> guestCounts = new Dictionary<int, int>();

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime date = reader.GetDateTime(0);
                                int guests = reader.GetInt32(1);
                                int month = date.Month;

                                if (guestCounts.ContainsKey(month))
                                {
                                    guestCounts[month] += guests;
                                }
                                else
                                {
                                    guestCounts[month] = guests;
                                }
                            }
                        }

                        for (int month = 1; month <= 12; month++)
                        {
                            int guests = guestCounts.ContainsKey(month) ? guestCounts[month] : 0;
                            int daysInMonth = DateTime.DaysInMonth(now.Year, month);
                            int maxCapacity = daysInMonth * 3 * capacity; // 3 obroka dnevno

                            double percent = maxCapacity > 0 ? (double)guests / maxCapacity * 100.0 : 0;
                            monthlyPercentages[month] = Math.Round(percent, 2);
                        }
                    }
                }

                // Pretvaranje brojeva meseca u nazive meseci (srpski, latinica)
                Dictionary<string, double> resultWithMonthNames = new Dictionary<string, double>();
                CultureInfo culture = new CultureInfo("sr-Latn");

                foreach (KeyValuePair<int, double> kvp in monthlyPercentages)
                {
                    string monthName = culture.DateTimeFormat.GetMonthName(kvp.Key);
                    resultWithMonthNames[monthName] = kvp.Value;
                }

                return resultWithMonthNames;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting monthly reservations count: {ex.Message}");
                throw;
            }
        }


        public Dictionary<int, int> GetTotalReservationsPerRestaurant(int ownerId)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                DateTime startOfYear = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime endOfYear = new DateTime(DateTime.Now.Year, 12, 31);

                string query = @"
                SELECT rr.RestaurantId, COUNT(*) AS ReservationCount
                FROM RestaurantReservations rr
                INNER JOIN Restaurants r ON rr.RestaurantId = r.Id
                WHERE r.OwnerId = @OwnerId
                AND rr.Date >= @StartOfYear AND rr.Date <= @EndOfYear
                GROUP BY rr.RestaurantId";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@OwnerId", ownerId);
                command.Parameters.AddWithValue("@StartOfYear", startOfYear.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@EndOfYear", endOfYear.ToString("yyyy-MM-dd"));

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int restaurantId = Convert.ToInt32(reader["RestaurantId"]);
                    int count = Convert.ToInt32(reader["ReservationCount"]);

                    result[restaurantId] = count;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reservation counts: {ex.Message}");
                throw;
            }
        }


        public Dictionary<string, int> GetMonthlyReservationCounts(int restaurantId, int ownerId)
        {
            Dictionary<int, int> monthlyCounts = new Dictionary<int, int>();

            try
            {
                using (SqliteConnection connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    DateTime now = DateTime.Now;
                    DateTime startOfYear = new DateTime(now.Year, 1, 1);
                    DateTime endOfYear = new DateTime(now.Year, 12, 31);

                    string query = @"
                    SELECT rr.Date
                    FROM RestaurantReservations rr
                    INNER JOIN Restaurants r ON rr.RestaurantId = r.Id
                    WHERE rr.RestaurantId = @RestaurantId
                    AND r.OwnerId = @OwnerId
                    AND rr.Date >= @StartOfYear AND rr.Date <= @EndOfYear";

                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RestaurantId", restaurantId);
                        command.Parameters.AddWithValue("@OwnerId", ownerId);
                        command.Parameters.AddWithValue("@StartOfYear", startOfYear.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@EndOfYear", endOfYear.ToString("yyyy-MM-dd"));

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime date = reader.GetDateTime(0);
                                int month = date.Month;

                                if (monthlyCounts.ContainsKey(month))
                                    monthlyCounts[month]++;
                                else
                                    monthlyCounts[month] = 1;
                            }
                        }
                    }
                }

                // Pretvaranje brojeva meseca u nazive meseci (srpski, latinica)
                Dictionary<string, int> resultWithMonthNames = new Dictionary<string, int>();
                CultureInfo culture = new CultureInfo("sr-Latn");

                for (int month = 1; month <= 12; month++)
                {
                    string monthName = culture.DateTimeFormat.GetMonthName(month);
                    int count = monthlyCounts.ContainsKey(month) ? monthlyCounts[month] : 0;
                    resultWithMonthNames[monthName] = count;
                }

                return resultWithMonthNames;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting monthly reservation counts: {ex.Message}");
                throw;
            }
        }


    }
}