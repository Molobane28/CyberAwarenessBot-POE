using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace CyberAwarenessBot
{
    // MySQL-backed implementation of ITaskRepository.
    // Every database operation is wrapped in try-catch so failures are handled gracefully:
    //  - Read operations log the error and return a safe default (empty list / null).
    //  - Write operations log the error and rethrow a descriptive exception so the caller/UI
    //    can inform the user instead of silently losing data.
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
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error initializing database: {ex.Message}");
                throw new Exception("Failed to initialize the database. Please verify MySQL is running and the connection string is correct.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error initializing database: {ex.Message}");
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
                        command.Parameters.AddWithValue("@title", task.Title);
                        command.Parameters.AddWithValue("@description", (object)task.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@reminderat", (object)task.ReminderAt ?? DBNull.Value);
                        command.Parameters.AddWithValue("@iscompleted", task.IsCompleted ? 1 : 0);
                        command.Parameters.AddWithValue("@createdat", task.CreatedAt);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error inserting task: {ex.Message}");
                throw new Exception("Failed to save the task to the database.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error inserting task: {ex.Message}");
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
                            tasks.Add(ReadTask(reader));
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error reading tasks: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error reading tasks: {ex.Message}");
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
                                return ReadTask(reader);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error reading task #{id}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error reading task #{id}: {ex.Message}");
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
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error marking task #{id} completed: {ex.Message}");
                throw new Exception("Failed to update the task status in the database.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error marking task #{id} completed: {ex.Message}");
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
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error deleting task #{id}: {ex.Message}");
                throw new Exception("Failed to delete the task from the database.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error deleting task #{id}: {ex.Message}");
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
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error updating reminder for task #{id}: {ex.Message}");
                throw new Exception("Failed to update the reminder in the database.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error updating reminder for task #{id}: {ex.Message}");
                throw;
            }
        }

        // Maps the current row of a data reader onto a CyberTask instance.
        private static CyberTask ReadTask(MySqlDataReader reader)
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
