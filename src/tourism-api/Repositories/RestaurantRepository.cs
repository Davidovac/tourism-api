using Microsoft.Data.Sqlite;
using tourism_api.Domain;
namespace tourism_api.Repositories;

public class RestaurantRepository
    {
        private readonly string _connectionString;
        public RestaurantRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString:SQLiteConnection"];
        }

        public List<Restaurant> GetPaged(int page, int pageSize, string orderBy, string orderDirection)
        {
            List<Restaurant> restaurants = new List<Restaurant>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = $@"
                    SELECT r.Id, r.Name, r.Description, r.Capacity, r.Latitude, r.Longitude, r.Status,
                           u.Id AS OwnerId, u.Username
                    FROM Restaurants r
                    INNER JOIN Users u ON r.OwnerId = u.Id
                    WHERE r.Status = 'objavljeno'
                    ORDER BY {orderBy} {orderDirection}
                    LIMIT @PageSize OFFSET @Offset";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.Parameters.AddWithValue("@Offset", pageSize * (page - 1));

                using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Restaurant restaurant = ReadRestaurantFromReader(reader);
                restaurant.ImageUrls = GetImageUrls(connection, restaurant.Id);
                restaurant.Ratings = GetRatings(connection, restaurant.Id);
                restaurants.Add(restaurant);
            }

            return restaurants;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public int CountAll()
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM Restaurants WHERE Status = 'objavljeno'";
                using SqliteCommand command = new SqliteCommand(query, connection);

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public int CountByOwner(int ownerId)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM Restaurants WHERE OwnerId = @OwnerId";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@OwnerId", ownerId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

    public List<Restaurant> GetByOwner(int ownerId, int page, int pageSize, string orderBy, string orderDirection)
    {
        List<Restaurant> restaurantList = new List<Restaurant>();
        Dictionary<int, Restaurant> restaurantMap = new Dictionary<int, Restaurant>();

        try
        {
            using SqliteConnection sqlConnection = new SqliteConnection(_connectionString);
            sqlConnection.Open();

            string query = @$"
            SELECT r.Id AS RestaurantId, r.Name, r.Description, r.Capacity, r.ImageUrl, r.Latitude, r.Longitude, r.Status,
                   u.Id AS OwnerId, u.Username,
                   ri.ImageUrl AS ExtraImageUrl
            FROM Restaurants r
            INNER JOIN Users u ON r.OwnerId = u.Id
            LEFT JOIN RestaurantImages ri ON ri.RestaurantId = r.Id
            WHERE r.OwnerId = @OwnerId
            ORDER BY {orderBy} {orderDirection}
            LIMIT @PageSize OFFSET @Offset";

            using SqliteCommand sqlCommand = new SqliteCommand(query, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@OwnerId", ownerId);
            sqlCommand.Parameters.AddWithValue("@PageSize", pageSize);
            sqlCommand.Parameters.AddWithValue("@Offset", pageSize * (page - 1));

            using SqliteDataReader sqlReader = sqlCommand.ExecuteReader();

            while (sqlReader.Read())
            {
                int restaurantId = Convert.ToInt32(sqlReader["RestaurantId"]);

                if (!restaurantMap.TryGetValue(restaurantId, out Restaurant restaurant))
                {
                    restaurant = new Restaurant
                    {
                        Id = restaurantId,
                        Name = sqlReader["Name"].ToString(),
                        Description = sqlReader["Description"].ToString(),
                        Capacity = Convert.ToInt32(sqlReader["Capacity"]),
                        Latitude = Convert.ToDouble(sqlReader["Latitude"]),
                        Longitude = Convert.ToDouble(sqlReader["Longitude"]),
                        Status = sqlReader["Status"].ToString(),
                        OwnerId = Convert.ToInt32(sqlReader["OwnerId"]),
                        Owner = new User
                        {
                            Id = Convert.ToInt32(sqlReader["OwnerId"]),
                            Username = sqlReader["Username"].ToString()
                        },
                        ImageUrls = new List<string>(),
                        Ratings = GetRatings(sqlConnection, restaurantId)
                    };

                    restaurantMap.Add(restaurantId, restaurant);
                    restaurantList.Add(restaurant);
                }

                if (sqlReader["ExtraImageUrl"] != DBNull.Value)
                {
                    restaurant.ImageUrls.Add(sqlReader["ExtraImageUrl"].ToString());
                }
            }

            return restaurantList;
        }
        catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

    public Restaurant GetById(int id)
    {
        Restaurant restaurant = null;

        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT r.Id, r.Name, r.Description, r.Capacity, r.Latitude, r.Longitude, r.Status,
                       u.Id AS OwnerId, u.Username,
                       m.Id AS MealId, m.OrderPosition, m.Name AS MealName, m.Price, m.Ingredients, m.ImageUrl AS MealImageUrl
                FROM Restaurants r
                INNER JOIN Users u ON r.OwnerId = u.Id
                LEFT JOIN Meals m ON m.RestaurantId = r.Id
                WHERE r.Id = @Id";

            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                restaurant = new Restaurant
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Description = reader["Description"].ToString(),
                    Capacity = Convert.ToInt32(reader["Capacity"]),
                    Latitude = Convert.ToDouble(reader["Latitude"]),
                    Longitude = Convert.ToDouble(reader["Longitude"]),
                    Status = reader["Status"].ToString(),
                    OwnerId = Convert.ToInt32(reader["OwnerId"]),
                    Owner = new User
                    {
                        Id = Convert.ToInt32(reader["OwnerId"]),
                        Username = reader["Username"].ToString()
                    },
                    Meals = new List<Meal>()
                };

                do
                {
                    if (reader["MealId"] != DBNull.Value)
                    {
                        Meal meal = new Meal
                        {
                            Id = Convert.ToInt32(reader["MealId"]),
                            Order = Convert.ToInt32(reader["OrderPosition"]),
                            Name = reader["MealName"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Ingredients = reader["Ingredients"].ToString(),
                            ImageUrl = reader["MealImageUrl"].ToString(),
                            RestaurantId = id
                        };
                        restaurant.Meals.Add(meal);
                    }
                } while (reader.Read());

                restaurant.ImageUrls = GetImageUrls(connection, id);
                restaurant.Ratings = GetRatings(connection, id);
            }

            return restaurant;
        }
        catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

    public Restaurant Create(Restaurant restaurant)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO Restaurants (Name, Description, Capacity, Latitude, Longitude, Status, OwnerId)
                VALUES (@Name, @Description, @Capacity, @Latitude, @Longitude, @Status, @OwnerId);
                SELECT last_insert_rowid();";

            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", restaurant.Name);
            command.Parameters.AddWithValue("@Description", restaurant.Description);
            command.Parameters.AddWithValue("@Capacity", restaurant.Capacity);
            command.Parameters.AddWithValue("@Latitude", restaurant.Latitude);
            command.Parameters.AddWithValue("@Longitude", restaurant.Longitude);
            command.Parameters.AddWithValue("@Status", restaurant.Status);
            command.Parameters.AddWithValue("@OwnerId", restaurant.OwnerId);

            restaurant.Id = Convert.ToInt32(command.ExecuteScalar());

            InsertImageUrls(connection, restaurant.Id, restaurant.ImageUrls);

            return restaurant;
        }
        catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

    public Restaurant Update(Restaurant restaurant)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Restaurants SET
                    Name = @Name,
                    Description = @Description,
                    Capacity = @Capacity,
                    Latitude = @Latitude,
                    Longitude = @Longitude,
                    Status = @Status
                WHERE Id = @Id";

            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", restaurant.Name);
            command.Parameters.AddWithValue("@Description", restaurant.Description);
            command.Parameters.AddWithValue("@Capacity", restaurant.Capacity);
            command.Parameters.AddWithValue("@Latitude", restaurant.Latitude);
            command.Parameters.AddWithValue("@Longitude", restaurant.Longitude);
            command.Parameters.AddWithValue("@Status", restaurant.Status);
            command.Parameters.AddWithValue("@Id", restaurant.Id);

            command.ExecuteNonQuery();

            // Remove old and add new image URLs
            DeleteImageUrls(connection, restaurant.Id);
            InsertImageUrls(connection, restaurant.Id, restaurant.ImageUrls);

            return restaurant;
        }
        catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = "DELETE FROM Restaurants WHERE Id = @Id";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        private Restaurant ReadRestaurantFromReader(SqliteDataReader reader)
        {
            return new Restaurant
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
                Description = reader["Description"].ToString(),
                Capacity = Convert.ToInt32(reader["Capacity"]),
                Latitude = Convert.ToDouble(reader["Latitude"]),
                Longitude = Convert.ToDouble(reader["Longitude"]),
                Status = reader["Status"].ToString(),
                OwnerId = Convert.ToInt32(reader["OwnerId"]),
                Owner = new User
                {
                    Id = Convert.ToInt32(reader["OwnerId"]),
                    Username = reader["Username"].ToString()
                }
            };
        }

        private List<string> GetImageUrls(SqliteConnection connection, int restaurantId)
        {
            List<string> imageUrls = new List<string>();

            using SqliteCommand command = new SqliteCommand("SELECT ImageUrl FROM RestaurantImages WHERE RestaurantId = @RestaurantId", connection);
            command.Parameters.AddWithValue("@RestaurantId", restaurantId);
            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                imageUrls.Add(reader["ImageUrl"].ToString());
            }

            return imageUrls;
        }

        private void InsertImageUrls(SqliteConnection connection, int restaurantId, List<string> imageUrls)
        {
            foreach (string imageUrl in imageUrls)
            {
                using SqliteCommand insertCommand = new SqliteCommand(
                    "INSERT INTO RestaurantImages (ImageUrl, RestaurantId) VALUES (@ImageUrl, @RestaurantId)",
                    connection);
                insertCommand.Parameters.AddWithValue("@ImageUrl", imageUrl);
                insertCommand.Parameters.AddWithValue("@RestaurantId", restaurantId);
                insertCommand.ExecuteNonQuery();
            }
        }
        private void DeleteImageUrls(SqliteConnection connection, int restaurantId)
        {
            using SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM RestaurantImages WHERE RestaurantId = @RestaurantId", connection);
            deleteCommand.Parameters.AddWithValue("@RestaurantId", restaurantId);
            deleteCommand.ExecuteNonQuery();
        }

        public void AddRating(RestaurantRating rating)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                INSERT INTO RestaurantRatings (RestaurantId, UserId, Rating, Comment, CreatedAt)
                VALUES (@RestaurantId, @UserId, @Rating, @Comment, @CreatedAt)";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@RestaurantId", rating.RestaurantId);
                command.Parameters.AddWithValue("@UserId", rating.UserId);
                command.Parameters.AddWithValue("@Rating", rating.Rating);
                command.Parameters.AddWithValue("@Comment", (object?)rating.Comment ?? DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", rating.CreatedAt.ToString("s"));

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri dodavanju ocene: {ex.Message}");
                throw;
            }
        }

        private List<RestaurantRating> GetRatings(SqliteConnection connection, int restaurantId)
        {
            List<RestaurantRating> ratings = new List<RestaurantRating>();

            string query = @"
            SELECT rr.Id, rr.RestaurantId, rr.UserId, rr.Rating, rr.Comment, rr.CreatedAt, u.Username
            FROM RestaurantRatings rr
            INNER JOIN Users u ON rr.UserId = u.Id
            WHERE rr.RestaurantId = @RestaurantId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@RestaurantId", restaurantId);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ratings.Add(new RestaurantRating
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    RestaurantId = Convert.ToInt32(reader["RestaurantId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Rating = Convert.ToInt32(reader["Rating"]),
                    Comment = reader["Comment"]?.ToString(),
                    CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                    User = new User
                    {
                        Id = Convert.ToInt32(reader["UserId"]),
                        Username = reader["Username"].ToString()
                    }
                });
            }

            return ratings;
        }

}
