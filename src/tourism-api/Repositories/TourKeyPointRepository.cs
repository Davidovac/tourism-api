﻿using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories
{
    public class TourKeyPointRepository
    {
        private readonly string _connectionString;

        public TourKeyPointRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString:SQLiteConnection"];
        }

        public List<KeyPoint> GetByTour(int id)
        {
            List<KeyPoint> keyPoints = new List<KeyPoint>();
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @$"
                    SELECT Id, OrderPosition, Name, Description, ImageUrl, Latitude, Longitude
                    FROM (SELECT k.Id, k.OrderPosition, k.Name, k.Description, k.ImageUrl, k.Latitude, k.Longitude, tk.KeyPointId
                    FROM KeyPoints k
                    INNER JOIN ToursKeyPoints tk ON k.Id = tk.KeyPointId
                    INNER JOIN Tours t ON t.Id = tk.TourId
                    WHERE t.Id = @TourId
                    ORDER BY k.Name)";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@TourId", id);
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

        public bool AddKeyPointToTour(Tour tour)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                using SqliteCommand command = connection.CreateCommand();
                var query = new StringBuilder();

                if (tour.KeyPoints != null || (tour.KeyPoints.Count > 0))
                {
                    query.AppendLine(@"
                    INSERT INTO ToursKeyPoints (TourId, KeyPointId) VALUES");
                    foreach (var keyPoint in tour.KeyPoints)
                    {
                        string k = "@KeyPointId" + (tour.KeyPoints.IndexOf(keyPoint) + 1);
                        if (tour.KeyPoints.IndexOf(keyPoint) == (tour.KeyPoints.Count() - 1))
                        {
                            query.Append(@"
                        (@TourId, " + k + ");");
                            command.Parameters.AddWithValue(k, keyPoint.Id);
                            break;
                        }
                        query.Append(@"
                    (@TourId, " + k + "),");
                        command.Parameters.AddWithValue(k, keyPoint.Id);
                    }
                }
                command.Parameters.AddWithValue("@TourId", tour.Id);
                command.CommandText = query.ToString();
                int ind = Convert.ToInt32(command.ExecuteNonQuery());

                if (ind != null && ind > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
        

        public bool ReplaceKeyPointsInTour(Tour tour)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                using SqliteCommand command = connection.CreateCommand();
                var query = new StringBuilder();

                if (tour.KeyPoints == null || (tour.KeyPoints.Count <= 0))
                {
                    query.AppendLine(@"
                    DELETE FROM ToursKeyPoints WHERE TourId = @TourId;");
                }
                else
                {
                    query.AppendLine(@"
                    DELETE FROM ToursKeyPoints WHERE TourId = @TourId;
                    INSERT INTO ToursKeyPoints (TourId, KeyPointId) VALUES");
                    foreach (var keyPoint in tour.KeyPoints)
                    {
                        string k = "@KeyPointId" + (tour.KeyPoints.IndexOf(keyPoint) + 1);
                        if (tour.KeyPoints.IndexOf(keyPoint) == (tour.KeyPoints.Count() - 1))
                        {
                            query.Append(@"
                        (@TourId, " + k + ");");
                            command.Parameters.AddWithValue(k, keyPoint.Id);
                            break;
                        }
                        query.Append(@"
                    (@TourId, " + k + "),");
                        command.Parameters.AddWithValue(k, keyPoint.Id);
                    }
                }
                command.Parameters.AddWithValue("@TourId", tour.Id);
                command.CommandText = query.ToString();
                int ind = Convert.ToInt32(command.ExecuteNonQuery());

                if (ind != null && ind > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
}
