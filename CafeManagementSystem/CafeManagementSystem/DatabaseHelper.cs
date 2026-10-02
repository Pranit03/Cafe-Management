// DatabaseHelper.cs

using System.Data.SqlClient;

namespace CafeManagementSystem
{
    /// <summary>
    /// A static helper class to manage the database connection.
    /// This centralizes the connection string so it only needs to be updated in one place.
    /// </summary>
    public static class DatabaseHelper
    {
        // The private, read-only connection string for the entire application.
        // IMPORTANT: Replace 'YOUR_SERVER_NAME' with your actual MS SQL Server instance name.
        private static readonly string connectionString = "Server=PRANIT\\SQLEXPRESS;Database=CafeDB;Trusted_Connection=True;";

        /// <summary>
        /// Creates and returns a new instance of SqlConnection.
        /// </summary>
        /// <returns>A new, unopened SqlConnection object.</returns>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}