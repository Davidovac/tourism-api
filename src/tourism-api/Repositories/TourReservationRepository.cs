using System.Text;
using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories
{
    public class TourReservationRepository
    {
        private readonly string _connectionString;

        public TourReservationRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString:SQLiteConnection"];
        }

        public List<TourReservation> GetByUser(int userId)
        {
            List<TourReservation> reservations = new List<TourReservation>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @$"
                    SELECT r.Id, r.GuestsCount, r.UserId, r.TourId,
                           t.Name AS TourName, t.Description AS TourDescription, t.DateTime AS TourDateTime, t.MaxGuests AS TourMaxGuests, t.Status AS TourStatus, t.GuideId AS TourGuideId
                    FROM Reservations r
                    INNER JOIN Users u ON r.UserId = u.Id
                    INNER JOIN Tours t ON r.TourId = t.Id
                    WHERE r.UserId = @UserId";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    reservations.Add(new TourReservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        GuestsCount = Convert.ToInt32(reader["GuestsCount"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        TourId = Convert.ToInt32(reader["TourId"]),
                        Tour = new Tour
                        {
                            Id = Convert.ToInt32(reader["TourId"]),
                            Name = reader["TourName"].ToString(),
                            Description = reader["TourDescription"].ToString(),
                            DateTime = Convert.ToDateTime(reader["TourDateTime"]),
                            MaxGuests = Convert.ToInt32(reader["TourMaxGuests"]),
                            Status = reader["TourStatus"].ToString(),
                            GuideId = Convert.ToInt32(reader["TourGuideId"])
                        }
                    });
                }

                return reservations;
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

        public bool DidReserveAlready(int userId, int tourId)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();
                string query = "SELECT COUNT(*) FROM Reservations WHERE UserId = @UserId AND TourId = @TourId";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@TourId", tourId);
                long count = (long)command.ExecuteScalar();
                return count > 0;
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

        public int? GetCapacityLeft(int id)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                                SELECT t.MaxGuests-SUM(r.GuestsCount)
                                FROM Reservations r
                                INNER JOIN Tours t ON r.TourId = t.Id
                                WHERE r.TourId = @TourId";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@TourId", id);
                object result = command.ExecuteScalar();

                if (result == DBNull.Value || result == null)
                {
                    return null;
                }
                    
                return Convert.ToInt32(result);
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


        public TourReservation Create(TourReservation reservation)
        {
            if (reservation == null || !reservation.Tour.IsValid())
            {
                throw new ArgumentException("Invalid reservation or tour data.");
            }
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                var query = new StringBuilder();
                query.Append(@"
                    INSERT INTO Reservations (GuestsCount, UserId, TourId)
                    VALUES (@GuestsCount, @UserId, @TourId);
                    SELECT last_insert_rowid();");
                command.Parameters.AddWithValue("@GuestsCount", reservation.GuestsCount);
                command.Parameters.AddWithValue("@UserId", reservation.UserId);
                command.Parameters.AddWithValue("@TourId", reservation.TourId);
                command.CommandText = query.ToString();
                reservation.Id = Convert.ToInt32(command.ExecuteScalar());

                return reservation;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka: {ex.Message}");
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

        public TourReservation Update(TourReservation reservation)
        {
            if (reservation == null)
            {
                throw new ArgumentException("Invalid reservation or tour data.");
            }
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                var query = new StringBuilder();
                query.Append(@"
                    UPDATE Reservations
                    SET GuestsCount = @GuestsCount + (SELECT GuestsCount FROM Reservations WHERE UserId = @UserId AND TourId = @TourId), UserId = @UserId, TourId = @TourId
                    WHERE UserId = @UserId AND TourId = @TourId;
                    SELECT last_insert_rowid();");
                command.Parameters.AddWithValue("@GuestsCount", reservation.GuestsCount);
                command.Parameters.AddWithValue("@UserId", reservation.UserId);
                command.Parameters.AddWithValue("@TourId", reservation.TourId);
                command.CommandText = query.ToString();
                reservation.Id = Convert.ToInt32(command.ExecuteScalar());

                return reservation;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka: {ex.Message}");
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

                string query = "DELETE FROM Reservations WHERE Id = @Id";
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

}
