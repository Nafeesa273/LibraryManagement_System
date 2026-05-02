using MySql.Data.MySqlClient;
using LibraryManagementSystem.Models;
using System.Collections.Generic;
using System;
using System.Data;

namespace LibraryManagementSystem.Data
{
    public static class DataManager
    {
        private static string connString = "server=127.0.0.1;user=root;database=librarydb;password=;Connect Timeout=10;";

        // 1. HELPER: SQL Command chalane ke liye (Code chota karne ke liye)
        private static void Run(string query, Action<MySqlCommand> addParams = null)
        {
            try
            {
                using var conn = new MySqlConnection(connString);
                conn.Open();
                using var cmd = new MySqlCommand(query, conn);
                addParams?.Invoke(cmd);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        // --- Logging ---
        public static void SaveLog(string action, string detail) =>
            Run("INSERT INTO activity_logs (action_type, details) VALUES (@a, @d)", m => {
                m.Parameters.AddWithValue("@a", action); m.Parameters.AddWithValue("@d", detail);
            });

        public static DataTable GetLogs() => GetTable("SELECT * FROM activity_logs ORDER BY log_date DESC");

        // --- Dashboard Stats ---
        public static int GetTotalBooks() => GetScalar("SELECT SUM(quantity) FROM books WHERE is_deleted = 0");
        public static int GetTotalUsers() => GetScalar("SELECT COUNT(*) FROM users");

        // --- Book Operations ---
        public static List<Book> LoadBooks() => FetchBooks("SELECT * FROM books WHERE is_deleted = 0");

        public static List<Book> SearchBooks(string k) =>
            FetchBooks("SELECT * FROM books WHERE (book_name LIKE @k OR author LIKE @k OR category LIKE @k) AND is_deleted = 0",
            m => m.Parameters.AddWithValue("@k", "%" + k + "%"));

        public static void AddBookToDb(Book b) =>
            Run("INSERT INTO books (book_name, author, quantity, isbn, category, price, is_deleted) VALUES (@t, @a, @q, @i, @c, @p, 0)", m => {
                m.Parameters.AddWithValue("@t", b.Title); m.Parameters.AddWithValue("@a", b.Author);
                m.Parameters.AddWithValue("@q", b.Quantity); m.Parameters.AddWithValue("@i", b.ISBN);
                m.Parameters.AddWithValue("@c", b.Category); m.Parameters.AddWithValue("@p", b.Price);
            });

        public static void UpdateBookField(int id, string col, object val) =>
            Run($"UPDATE books SET {col} = @v WHERE id = @id", m => {
                m.Parameters.AddWithValue("@v", val); m.Parameters.AddWithValue("@id", id);
            });

        // --- Recycle Bin ---
        public static void DeleteBookFromDb(int id) => Run($"UPDATE books SET is_deleted = 1 WHERE id = {id}");
        public static void RestoreBook(int id) => Run($"UPDATE books SET is_deleted = 0 WHERE id = {id}");
        public static void HardDeleteBook(int id) => Run($"DELETE FROM books WHERE id = {id}");
        public static List<Book> GetDeletedBooks() => FetchBooks("SELECT * FROM books WHERE is_deleted = 1");

        // --- User Management ---
        public static List<User> LoadUsers()
        {
            var list = new List<User>();
            try
            {
                using var c = new MySqlConnection(connString); c.Open();
                using var r = new MySqlCommand("SELECT * FROM users", c).ExecuteReader();
                while (r.Read()) list.Add(new User { Username = r["username"].ToString(), Password = r["password"].ToString(), Role = r["role"].ToString() });
            }
            catch { }
            return list;
        }

        public static void RegisterUser(User u) =>
            Run("INSERT INTO users (username, password, role) VALUES (@u, @p, @r)", m => {
                m.Parameters.AddWithValue("@u", u.Username); m.Parameters.AddWithValue("@p", u.Password); m.Parameters.AddWithValue("@r", u.Role);
            });

        // --- Transactions & Fines ---
        public static DataTable GetTransactionHistory() => GetTable("SELECT username, book_id, issue_date, return_date, status FROM issued_books ORDER BY issue_date DESC");

        public static bool UpdateBookQuantity(int id, int change)
        {
            Run($"UPDATE books SET quantity = quantity + {change} WHERE id = {id}");
            return true;
        }

        public static int RecordTransaction(string user, int bid, string action)
        {
            int fine = 0;
            if (action == "Issue") Run("INSERT INTO issued_books (username, book_id, issue_date, status) VALUES (@u, @b, NOW(), 'Issued')", m => { m.Parameters.AddWithValue("@u", user); m.Parameters.AddWithValue("@b", bid); });
            else
            {
                int days = GetScalar($"SELECT DATEDIFF(NOW(), issue_date) FROM issued_books WHERE username = '{user}' AND book_id = {bid} AND status = 'Issued' LIMIT 1");
                if (days > 7) fine = (days - 7) * 100;
                Run($"UPDATE issued_books SET return_date = NOW(), status = 'Returned' WHERE username = '{user}' AND book_id = {bid} AND status = 'Issued' LIMIT 1");
            }
            return fine;
        }

        // --- Supporting Helpers (Hidden Complexity) ---
        private static int GetScalar(string q)
        {
            try
            {
                using var c = new MySqlConnection(connString); c.Open();
                var res = new MySqlCommand(q, c).ExecuteScalar();
                return res != DBNull.Value ? Convert.ToInt32(res) : 0;
            }
            catch { return 0; }
        }

        private static DataTable GetTable(string q)
        {
            var dt = new DataTable();
            try
            {
                using var c = new MySqlConnection(connString); c.Open();
                new MySqlDataAdapter(q, c).Fill(dt);
            }
            catch { }
            return dt;
        }

        private static List<Book> FetchBooks(string q, Action<MySqlCommand> addParams = null)
        {
            var list = new List<Book>();
            try
            {
                using var c = new MySqlConnection(connString); c.Open();
                using var cmd = new MySqlCommand(q, c);
                addParams?.Invoke(cmd);
                using var r = cmd.ExecuteReader();
                while (r.Read()) list.Add(new Book
                {
                    Id = (int)r["id"],
                    Title = r["book_name"].ToString(),
                    Author = r["author"].ToString(),
                    Quantity = (int)r["quantity"],
                    ISBN = r["isbn"].ToString(),
                    Category = r["category"].ToString(),
                    Price = (int)r["price"]
                });
            }
            catch { }
            return list;
        }
    }
}