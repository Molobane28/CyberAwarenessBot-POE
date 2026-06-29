using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CyberAwarenessBot
{
    /// <summary>
    /// MySQL implementation of ITaskRepository with full error handling
    /// </summary>
    public class MySqlTaskRepository : ITaskRepository
    {
        private readonly string connectionString;

        public MySqlTaskRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        CREATE TABLE IF NOT EXISTS tasks (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            title VARCHAR(255) NOT NULL,
                            description TEXT NULL,
                            reminderat DATETIME NULL,
                            iscompleted BIT NOT NULL DEFAULT 0,
                            createdat DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        public int AddTask(CyberTask task)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        INSERT INTO tasks (title, description, reminderat, iscompleted, createdat)
                        VALUES (@title, @description, @reminderat, @iscompleted, @createdat);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@title", task.Title ?? string.Empty);
                        command.Parameters.AddWithValue("@description", (object)task.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@reminderat", (object)task.ReminderAt ?? DBNull.Value);
                        command.Parameters.AddWithValue("@iscompleted", task.IsCompleted ? 1 : 0);
                        command.Parameters.AddWithValue("@createdat", task.CreatedAt);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTask error: {ex.Message}");
                throw;
            }
        }

        public List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT id, title, description, reminderat, iscompleted, createdat FROM tasks ORDER BY createdat DESC";

                    using (var command = new MySqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new CyberTask
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                                ReminderAt = reader.IsDBNull(reader.GetOrdinal("reminderat")) ? (DateTime?)null : reader.GetDateTime("reminderat"),
                                IsCompleted = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("iscompleted"))),
                                CreatedAt = reader.GetDateTime("createdat")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllTasks error: {ex.Message}");
                throw;
            }

            return tasks;
        }

        public CyberTask GetTaskById(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT id, title, description, reminderat, iscompleted, createdat FROM tasks WHERE id = @id LIMIT 1";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CyberTask
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                                    ReminderAt = reader.IsDBNull(reader.GetOrdinal("reminderat")) ? (DateTime?)null : reader.GetDateTime("reminderat"),
                                    IsCompleted = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("iscompleted"))),
                                    CreatedAt = reader.GetDateTime("createdat")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTaskById error: {ex.Message}");
                throw;
            }

            return null;
        }

        public void MarkCompleted(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "UPDATE tasks SET iscompleted = 1 WHERE id = @id";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception($"Task with ID {id} not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkCompleted error: {ex.Message}");
                throw;
            }
        }

        public void DeleteTask(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "DELETE FROM tasks WHERE id = @id";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception($"Task with ID {id} not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteTask error: {ex.Message}");
                throw;
            }
        }

        public void UpdateReminder(int id, DateTime? reminderAt)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "UPDATE tasks SET reminderat = @reminderat WHERE id = @id";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@reminderat", (object)reminderAt ?? DBNull.Value);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception($"Task with ID {id} not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateReminder error: {ex.Message}");
                throw;
            }
        }
    }
}