using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories;

public class UserRepository
{
    private readonly string _connectionString;
    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionString:SQLiteConnection"];
    }
    public User Get(string username, string password)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = "SELECT Id, Username, Password, Role FROM Users WHERE Username = @Username AND Password = @Password";

            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                User user = new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"].ToString(),
                    Password = reader["Password"].ToString(),
                    Role = reader["Role"].ToString()
                };
                return user;
            }

            return null;
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

    public User? GetById(int id)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = "SELECT Id, Username, Password, Role FROM Users WHERE Id = @Id";

            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                User user = new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"].ToString(),
                    Password = reader["Password"].ToString(),
                    Role = reader["Role"].ToString()
                };
                return user;
            }

            return null;
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
}
