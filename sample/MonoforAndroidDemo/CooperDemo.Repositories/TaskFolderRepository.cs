using System;
using System.Collections.Generic;
using System.IO;
using CooperDemo.Infrastructure;
using CooperDemo.Model;
using Mono.Data.Sqlite;

namespace CooperDemo.Repositories
{
    public class TaskFolderRepository : ITaskFolderRepository
    {
        private ILogger _logger;
        private string db_file = "notes.db3";

        public TaskFolderRepository(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());
        }

        void ITaskFolderRepository.Add(TaskFolder folder)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO TaskFolders (ID, Name) VALUES (@Id, @Name);";
                    cmd.Parameters.AddWithValue("@ID", folder.ID);
                    cmd.Parameters.AddWithValue("@Name", folder.Name);
                    cmd.ExecuteNonQuery();
                    _logger.InfoFormat("new task foler inserted, id:{0}, name:{1}", folder.ID, folder.Name);
                }
            }
        }
        void ITaskFolderRepository.Update(TaskFolder folder)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE TaskFolders SET Name = @Name WHERE ID = @Id";
                    cmd.Parameters.AddWithValue("@Id", Utils.ConvertType<int>(folder.ID));
                    cmd.Parameters.AddWithValue("@Name", folder.Name);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        void ITaskFolderRepository.Remove(TaskFolder folder)
        {
            var sql = string.Format("DELETE FROM TaskFolders WHERE ID = {0};", folder.ID);

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        TaskFolder ITaskFolderRepository.FindBy(int id)
        {
            var sql = string.Format("SELECT * FROM TaskFolders WHERE ID = {0};", id);

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TaskFolder { ID = reader.GetInt32(0), Name = reader.GetString(1) };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        IEnumerable<TaskFolder> ITaskFolderRepository.FindAll()
        {
            var sql = "SELECT * FROM TaskFolders;";

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new TaskFolder { ID = reader.GetInt32(0), Name = reader.GetString(1) };
                        }
                    }
                }
            }
        }

        private SqliteConnection GetConnection()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), db_file);
            var exists = File.Exists(dbPath);

            if (!exists)
            {
                SqliteConnection.CreateFile(dbPath);
            }

            var connection = new SqliteConnection("Data Source=" + dbPath);

            if (!exists)
            {
                CreateDatabase(connection);
            }

            return connection;
        }
        private void CreateDatabase(SqliteConnection connection)
        {
            var sql = "CREATE TABLE TaskFolders (ID INTEGER PRIMARY KEY, Name NVARCHAR(255));";

            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
}