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
    }
}