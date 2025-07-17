using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories;

public class TourRepository
{
    private readonly string _connectionString;
    private readonly TourKeyPointRepository _tourKPRepo;
    public TourRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionString:SQLiteConnection"];
        _tourKPRepo = new TourKeyPointRepository(configuration);
    }

    public List<Tour> GetPaged(int page, int pageSize, string orderBy, string orderDirection)
    {
        List<Tour> tours = new List<Tour>();

        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @$"
                    SELECT t.Id, t.Name, t.Description, t.DateTime, t.MaxGuests, t.Status,
                           u.Id AS GuideId, u.Username 
                    FROM Tours t
                    INNER JOIN Users u ON t.GuideId = u.Id
                    WHERE t.Status = 'objavljeno'
                    ORDER BY {orderBy} {orderDirection} LIMIT @PageSize OFFSET @Offset";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@Offset", pageSize * (page - 1));

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (reader["Status"].ToString() != "objavljeno")
                {
                    continue;
                }
                tours.Add(new Tour
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Description = reader["Description"].ToString(),
                    DateTime = Convert.ToDateTime(reader["DateTime"]),
                    MaxGuests = Convert.ToInt32(reader["MaxGuests"]),
                    Status = reader["Status"].ToString(),
                    GuideId = Convert.ToInt32(reader["GuideId"]),
                    Guide = new User
                    {
                        Id = Convert.ToInt32(reader["GuideId"]),
                        Username = reader["Username"].ToString()
                    },
                    KeyPoints = _tourKPRepo.GetByTour(Convert.ToInt32(reader["Id"])),
                    Ratings = GetRatings(connection, Convert.ToInt32(reader["Id"]))
                });
            }

            return tours;
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

            string query = "SELECT COUNT(*) FROM Tours";
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

    public List<Tour> GetByGuide(int guideId)
    {
        List<Tour> tours = new List<Tour>();

        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @$"
                    SELECT t.Id, t.Name, t.Description, t.DateTime, t.MaxGuests, t.Status,
                           u.Id AS GuideId, u.Username 
                    FROM Tours t 
                    INNER JOIN Users u ON t.GuideId = u.Id
                    WHERE t.GuideId = @GuideId";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@GuideId", guideId);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tours.Add(new Tour
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Description = reader["Description"].ToString(),
                    DateTime = Convert.ToDateTime(reader["DateTime"]),
                    MaxGuests = Convert.ToInt32(reader["MaxGuests"]),
                    Status = reader["Status"].ToString(),
                    GuideId = Convert.ToInt32(reader["GuideId"]),
                    Ratings = GetRatings(connection, Convert.ToInt32(reader["Id"]))
                });
            }

            return tours;
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

    public Tour GetById(int id)
    {
        Tour tour = null;

        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                    SELECT t.Id, t.Name, t.Description, t.DateTime, t.MaxGuests, t.Status,
                           u.Id AS GuideId, u.Username,
                           kp.Id AS KeyPointId, kp.OrderPosition, kp.Name AS KeyPointName, kp.Description AS KeyPointDescription, 
                           kp.ImageUrl AS KeyPointImageUrl, kp.Latitude, kp.Longitude
                    FROM Tours t
                    INNER JOIN Users u ON t.GuideId = u.Id
                    LEFT JOIN ToursKeyPoints tkp ON tkp.TourId = t.Id
                    LEFT JOIN KeyPoints kp ON tkp.KeyPointId = kp.Id
                    WHERE t.Id = @Id";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (tour == null)
                {
                    tour = new Tour
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString(),
                        DateTime = Convert.ToDateTime(reader["DateTime"]),
                        MaxGuests = Convert.ToInt32(reader["MaxGuests"]),
                        Status = reader["Status"].ToString(),
                        GuideId = Convert.ToInt32(reader["GuideId"]),
                        Guide = new User
                        {
                            Id = Convert.ToInt32(reader["GuideId"]),
                            Username = reader["Username"].ToString()
                        },
                        KeyPoints = new List<KeyPoint>(),
                        Ratings = GetRatings(connection, Convert.ToInt32(reader["Id"]))
                    };
                }

                if (reader["KeyPointId"] != DBNull.Value)
                {
                    KeyPoint keyPoint = new KeyPoint
                    {
                        Id = Convert.ToInt32(reader["KeyPointId"]),
                        Order = Convert.ToInt32(reader["OrderPosition"]),
                        Name = reader["KeyPointName"].ToString(),
                        Description = reader["KeyPointDescription"].ToString(),
                        ImageUrl = reader["KeyPointImageUrl"].ToString(),
                        Latitude = Convert.ToInt32(reader["Latitude"]),
                        Longitude = Convert.ToInt32(reader["Longitude"]),
                    };
                    tour.KeyPoints.Add(keyPoint);
                }
            }
            if (tour != null && tour.Status != "objavljeno")
            {
                return null;
            }

            return tour;
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

    public Tour Create(Tour tour)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            var query = new StringBuilder();

            query.Append(@"
                    INSERT INTO Tours (Name, Description, DateTime, MaxGuests, Status, GuideID) VALUES
                   (@Name, @Description, @DateTime, @MaxGuests, @Status, @GuideId);
                    SELECT LAST_INSERT_ROWID();");
            command.Parameters.AddWithValue("@Name", tour.Name);
            command.Parameters.AddWithValue("@Description", tour.Description);
            command.Parameters.AddWithValue("@DateTime", tour.DateTime);
            command.Parameters.AddWithValue("@MaxGuests", tour.MaxGuests);
            command.Parameters.AddWithValue("@Status", tour.Status);
            command.Parameters.AddWithValue("@GuideId", tour.GuideId);
            command.CommandText = query.ToString();

            tour.Id = Convert.ToInt32(command.ExecuteScalar());

            return tour;
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

    public Tour Update(Tour tour)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            var query = new StringBuilder();

            query.Append(@"
                    UPDATE Tours 
                    SET Name = @Name, Description = @Description, DateTime = @DateTime, 
                        MaxGuests = @MaxGuests, Status = @Status
                    WHERE Id = @Id;");

            command.Parameters.AddWithValue("@Id", tour.Id);
            command.Parameters.AddWithValue("@Name", tour.Name);
            command.Parameters.AddWithValue("@Description", tour.Description);
            command.Parameters.AddWithValue("@DateTime", tour.DateTime);
            command.Parameters.AddWithValue("@MaxGuests", tour.MaxGuests);
            command.Parameters.AddWithValue("@Status", tour.Status);
            command.CommandText = query.ToString();
            int affectedRows = command.ExecuteNonQuery();
            return affectedRows > 0 ? tour : null;
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

            string query = "DELETE FROM Tours WHERE Id = @Id";
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

    public bool AddRating(TourRating rating)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            bool rated = RatedAlready(connection, rating.UserId, rating.TourId);
            if (rated)
            {
                return rated;
            }

            string query = @"
                INSERT INTO TourRatings (TourId, UserId, Rating, Comment, CreatedAt)
                VALUES (@TourId, @UserId, @Rating, @Comment, @CreatedAt)";

            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TourId", rating.TourId);
            command.Parameters.AddWithValue("@UserId", rating.UserId);
            command.Parameters.AddWithValue("@Rating", rating.Rating);
            command.Parameters.AddWithValue("@Comment", (object?)rating.Comment ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", rating.CreatedAt.ToString("s"));

            command.ExecuteNonQuery();

            return rated;
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

    public List<TourRating> GetRatings(SqliteConnection connection, int tourId)
    {
        List<TourRating> ratings = new List<TourRating>();

        try
        {
            string query = @"
            SELECT tr.Id, tr.TourId, tr.UserId, tr.Rating, tr.Comment, tr.CreatedAt, u.Username
            FROM TourRatings tr
            INNER JOIN Users u ON tr.UserId = u.Id
            WHERE tr.TourId = @TourId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TourId", tourId);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ratings.Add(new TourRating
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    TourId = Convert.ToInt32(reader["TourId"]),
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

    public bool RatedAlready(SqliteConnection connection, int userId, int tourId)
    {
        try
        {
            string query = @"
            SELECT tr.Id, tr.TourId, tr.UserId, tr.Rating, tr.Comment, tr.CreatedAt
            FROM TourRatings tr
            WHERE tr.TourId = @TourId AND tr.UserId = @UserId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TourId", tourId);
            command.Parameters.AddWithValue("@UserId", userId);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                TourRating rating = new TourRating
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    TourId = Convert.ToInt32(reader["TourId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Rating = Convert.ToInt32(reader["Rating"]),
                    Comment = reader["Comment"]?.ToString(),
                    CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString())
                };
                if (rating != null)
                {
                    return true;
                }
            }
            return false;
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
