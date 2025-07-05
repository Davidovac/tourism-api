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

            string query = @$"
                    SELECT r.Id, r.Name, r.Description, r.Capacity, r.ImageUrl, r.Latitude, r.Longitude, r.Status,
                           u.Id AS OwnerId, u.Username
                    FROM Restaurants r
                    INNER JOIN Users u ON r.OwnerId = u.Id
                    ORDER BY {orderBy} {orderDirection} LIMIT @PageSize OFFSET @Offset";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@Offset", pageSize * (page - 1));

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                restaurants.Add(new Restaurant
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Description = reader["Description"].ToString(),
                    Capacity = Convert.ToInt32(reader["Capacity"]),
                    ImageUrl = reader["ImageUrl"].ToString(),
                    Latitude = Convert.ToDouble(reader["Latitude"]),
                    Longitude = Convert.ToDouble(reader["Longitude"]),
                    Status = reader["Status"].ToString(),
                    OwnerId = Convert.ToInt32(reader["OwnerId"]),
                    Owner = new User
                    {
                        Id = Convert.ToInt32(reader["OwnerId"]),
                        Username = reader["Username"].ToString()
                    }
                });
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

            string query = "SELECT COUNT(*) FROM Restaurants";
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
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = "SELECT COUNT(*) FROM Restaurants WHERE OwnerId = @OwnerId";
            using var command = new SqliteCommand(query, connection);
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
        List<Restaurant> restaurants = new List<Restaurant>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = $@"
                SELECT r.Id, r.Name, r.Description, r.Capacity, r.ImageUrl, r.Latitude, r.Longitude, r.Status,
                       u.Id AS OwnerId, u.Username
                FROM Restaurants r
                INNER JOIN Users u ON r.OwnerId = u.Id
                WHERE r.OwnerId = @OwnerId
                ORDER BY {orderBy} {orderDirection}
                LIMIT @PageSize OFFSET @Offset";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@OwnerId", ownerId);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@Offset", pageSize * (page - 1));

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                restaurants.Add(ReadRestaurantFromReader(reader));
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


    public Restaurant GetById(int id)
    {
        Restaurant restaurant = null;

        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                    SELECT r.Id, r.Name, r.Description, r.Capacity, r.ImageUrl, r.Longitude, r.Latitude, r.Status,
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
                    ImageUrl = reader["ImageUrl"].ToString(),
                    Longitude = Convert.ToDouble(reader["Longitude"]),
                    Latitude = Convert.ToDouble(reader["Latitude"]),
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
                            ImageUrl = reader["MealImageUrl"] != DBNull.Value ? reader["MealImageUrl"].ToString() : null,
                            RestaurantId = Convert.ToInt32(reader["Id"])
                        };
                        restaurant.Meals.Add(meal);
                    }
                } while (reader.Read());
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
                    INSERT INTO Restaurants (Name, Description, Capacity, ImageUrl, Latitude, Longitude, Status, OwnerId) 
                    VALUES (@Name, @Description, @Capacity, @ImageUrl, @Latitude, @Longitude, @Status, @OwnerId);
                    SELECT LAST_INSERT_ROWID();";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", restaurant.Name);
            command.Parameters.AddWithValue("@Description", restaurant.Description);
            command.Parameters.AddWithValue("@Capacity", restaurant.Capacity);
            command.Parameters.AddWithValue("@ImageUrl", restaurant.ImageUrl);
            command.Parameters.AddWithValue("@Latitude", restaurant.Latitude);
            command.Parameters.AddWithValue("@Longitude", restaurant.Longitude);
            command.Parameters.AddWithValue("@Status", restaurant.Status);
            command.Parameters.AddWithValue("@OwnerId", restaurant.OwnerId);

            restaurant.Id = Convert.ToInt32(command.ExecuteScalar());

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
                    UPDATE Restaurants 
                    SET Name = @Name, Description = @Description, Capacity = @Capacity, ImageUrl = @ImageUrl,
                        Latitude = @Latitude, Longitude = @Longitude, Status = @Status
                    WHERE Id = @Id";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", restaurant.Id);
            command.Parameters.AddWithValue("@Name", restaurant.Name);
            command.Parameters.AddWithValue("@Description", restaurant.Description);
            command.Parameters.AddWithValue("@Capacity", restaurant.Capacity);
            command.Parameters.AddWithValue("@ImageUrl", restaurant.ImageUrl);
            command.Parameters.AddWithValue("@Latitude", restaurant.Latitude);
            command.Parameters.AddWithValue("@Longitude", restaurant.Longitude);
            command.Parameters.AddWithValue("@Status", restaurant.Status);

            int affectedRows = command.ExecuteNonQuery();
            return affectedRows > 0 ? restaurant : null;
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
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
            ImageUrl = reader["ImageUrl"].ToString(),
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
}
