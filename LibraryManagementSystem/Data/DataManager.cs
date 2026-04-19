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

        public static int GetTotalBooks()
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT SUM(quantity) FROM books", conn);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
            catch { return 0; }
        }

        public static int GetTotalUsers()
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT COUNT(*) FROM users", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        public static List<User> LoadUsers()
        {
            var users = new List<User>();
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM users", conn);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Username = reader["username"]?.ToString() ?? "",
                            Password = reader["password"]?.ToString() ?? "",
                            Role = reader["role"]?.ToString() ?? "User"
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("DB Error: " + ex.Message); }
            return users;
        }

        public static void RegisterUser(User user)
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var query = "INSERT INTO users (username, password, role) VALUES (@u, @p, @r)";
                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", user.Username);
                    cmd.Parameters.AddWithValue("@p", user.Password);
                    cmd.Parameters.AddWithValue("@r", user.Role);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { Console.WriteLine("Reg Error: " + ex.Message); }
        }

        public static List<Book> LoadBooks()
        {
            var books = new List<Book>();
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM books", conn);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Title = reader["book_name"]?.ToString() ?? "N/A",
                            Author = reader["author"]?.ToString() ?? "N/A",
                            Quantity = Convert.ToInt32(reader["quantity"]),
                            ISBN = reader["isbn"]?.ToString() ?? "N/A",
                            Category = reader["category"]?.ToString() ?? "General",
                            Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Book Load Error: " + ex.Message); }
            return books;
        }

        public static void AddBookToDb(Book book)
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var query = "INSERT INTO books (book_name, author, quantity, isbn, category, price) VALUES (@t, @a, @q, @i, @c, @p)";
                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@t", book.Title);
                    cmd.Parameters.AddWithValue("@a", book.Author);
                    cmd.Parameters.AddWithValue("@q", book.Quantity);
                    cmd.Parameters.AddWithValue("@i", book.ISBN);
                    cmd.Parameters.AddWithValue("@c", book.Category);
                    cmd.Parameters.AddWithValue("@p", book.Price);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { Console.WriteLine("Add Error: " + ex.Message); }
        }

        public static bool UpdateBookQuantity(int id, int change)
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    if (change < 0)
                    {
                        var checkCmd = new MySqlCommand("SELECT quantity FROM books WHERE id = @id", conn);
                        checkCmd.Parameters.AddWithValue("@id", id);
                        var res = checkCmd.ExecuteScalar();
                        if (res == null || Convert.ToInt32(res) <= 0) return false;
                    }
                    var query = "UPDATE books SET quantity = quantity + @change WHERE id = @id";
                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@change", change);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public static void RecordTransaction(string user, int bid, string action)
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = action == "Issue"
                        ? "INSERT INTO issued_books (username, book_id, issue_date, status) VALUES (@u, @bid, NOW(), 'Issued')"
                        : "UPDATE issued_books SET return_date = NOW(), status = 'Returned' WHERE username = @u AND book_id = @bid AND status = 'Issued' LIMIT 1";
                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", user);
                    cmd.Parameters.AddWithValue("@bid", bid);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { Console.WriteLine("Log Error: " + ex.Message); }
        }

        public static DataTable GetTransactionHistory()
        {
            var dt = new DataTable();
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM issued_books ORDER BY issue_date DESC", conn);
                    using var adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            catch { }
            return dt;
        }

        public static List<Book> SearchBooks(string keyword)
        {
            var books = new List<Book>();
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var query = "SELECT * FROM books WHERE book_name LIKE @k OR author LIKE @k";
                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@k", "%" + keyword + "%");
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Title = reader["book_name"]?.ToString() ?? "N/A",
                            Author = reader["author"]?.ToString() ?? "N/A",
                            Quantity = Convert.ToInt32(reader["quantity"]),
                            Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0
                        });
                    }
                }
            }
            catch { }
            return books;
        }

        public static void UpdateBookField(int id, string col, object val)
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand($"UPDATE books SET {col} = @v WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@v", val);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        public static void DeleteBookFromDb(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM books WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }
    }
}