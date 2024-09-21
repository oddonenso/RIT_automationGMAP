using System;
using System.Data;
using System.Data.SqlClient;

namespace MarkerApp
{
    public class DatabaseManager
    {
        private readonly string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlDataReader GetMarkers()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand("SELECT Id, Name, Latitude, Longitude FROM Markers", connection);
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void AddMarkerToDatabase(string name, double latitude, double longitude)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Markers (Name, Latitude, Longitude) VALUES (@Name, @Latitude, @Longitude)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Latitude", latitude);
                    command.Parameters.AddWithValue("@Longitude", longitude);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateMarkerPosition(int id, double latitude, double longitude)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Markers SET Latitude = @Latitude, Longitude = @Longitude WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Latitude", latitude);
                    command.Parameters.AddWithValue("@Longitude", longitude);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
