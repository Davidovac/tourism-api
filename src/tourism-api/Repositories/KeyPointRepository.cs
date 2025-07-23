using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories;

public class KeyPointRepository
{
    private readonly string _connectionString;

    public KeyPointRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionString:SQLiteConnection"];
    }

    public List<KeyPoint> GetPaged(int tourId, int page, int pageSize)
    {
        List<KeyPoint> keyPoints = new List<KeyPoint>();

        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @$"
                    SELECT Id, OrderPosition, Name, Description, ImageUrl, Latitude, Longitude
                    FROM (SELECT k.Id, k.OrderPosition, k.Name, k.Description, k.ImageUrl, k.Latitude, k.Longitude
                    FROM KeyPoints k
                    LEFT JOIN ToursKeyPoints tk ON k.Id = tk.KeyPointId
                    ";
            if (tourId != 0)
            {
                query += "WHERE tk.TourId != @TourId OR tk.TourId IS NULL";
            }
            query += $@"
                    GROUP BY k.Id
                    ORDER BY k.Id)
                    LIMIT @PageSize OFFSET @Offset";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TourId", tourId);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@Offset", pageSize * (page - 1));

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                keyPoints.Add(new KeyPoint
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Order = Convert.ToInt32(reader["OrderPosition"]),
                    Name = reader["Name"].ToString(),
                    Description = reader["Description"].ToString(),
                    ImageUrl = reader["ImageUrl"].ToString(),
                    Latitude = Convert.ToDouble(reader["Latitude"]),
                    Longitude = Convert.ToDouble(reader["Longitude"]),
                });
            }

            return keyPoints;
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

    public int CountAll(int tourId)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @$"SELECT COUNT(*)
                            FROM (SELECT k.Id, k.OrderPosition, k.Name, k.Description, k.ImageUrl, k.Latitude, k.Longitude
                            FROM KeyPoints k
                            LEFT JOIN ToursKeyPoints tk ON k.Id = tk.KeyPointId";
            if (tourId != 0)
            {
                query += "WHERE tk.TourId != @TourId OR tk.TourId IS NULL";
            }
            query += $@"
                    GROUP BY k.Id
                    ORDER BY k.Id)";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TourId", tourId);
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


    public KeyPoint Create(KeyPoint keyPoint)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                    INSERT INTO KeyPoints (OrderPosition, Name, Description, ImageUrl, Latitude, Longitude)
                    VALUES (@Order, @Name, @Description, @ImageUrl, @Latitude, @Latitude);
                    SELECT LAST_INSERT_ROWID();";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Order", keyPoint.Order);
            command.Parameters.AddWithValue("@Name", keyPoint.Name);
            command.Parameters.AddWithValue("@Description", keyPoint.Description);
            command.Parameters.AddWithValue("@ImageUrl", keyPoint.ImageUrl);
            command.Parameters.AddWithValue("@Latitude", keyPoint.Latitude);
            command.Parameters.AddWithValue("@Longitude", keyPoint.Longitude);

            keyPoint.Id = Convert.ToInt32(command.ExecuteScalar());

            return keyPoint;
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

            string query = "DELETE FROM KeyPoints WHERE Id = @Id";
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
}
