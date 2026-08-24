Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Models

Namespace DataAccess
    Public Class UserRepository
        Public Function GetById(id As Integer) As User
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Username, PasswordHash, Salt, FullName, Email, Role, IsActive, CreatedAt, LastLogin FROM Users WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", id)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapUser(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetByUsername(username As String) As User
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Username, PasswordHash, Salt, FullName, Email, Role, IsActive, CreatedAt, LastLogin FROM Users WHERE LOWER(Username) = LOWER(@u);"
                    cmd.Parameters.AddWithValue("@u", username.Trim())
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapUser(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetAll() As List(Of User)
            Dim list As New List(Of User)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Username, PasswordHash, Salt, FullName, Email, Role, IsActive, CreatedAt, LastLogin FROM Users ORDER BY Id ASC;"
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapUser(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Insert(user As User) As Integer
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        INSERT INTO Users (Username, PasswordHash, Salt, FullName, Email, Role, IsActive, CreatedAt)
                        VALUES (@u, @p, @s, @f, @e, @r, @a, @c);
                        SELECT last_insert_rowid();
                    "
                    cmd.Parameters.AddWithValue("@u", user.Username)
                    cmd.Parameters.AddWithValue("@p", user.PasswordHash)
                    cmd.Parameters.AddWithValue("@s", user.Salt)
                    cmd.Parameters.AddWithValue("@f", user.FullName)
                    cmd.Parameters.AddWithValue("@e", user.Email)
                    cmd.Parameters.AddWithValue("@r", CInt(user.Role))
                    cmd.Parameters.AddWithValue("@a", If(user.IsActive, 1, 0))
                    cmd.Parameters.AddWithValue("@c", user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    user.Id = Convert.ToInt32(cmd.ExecuteScalar())
                    Return user.Id
                End Using
            End Using
        End Function

        Public Function Update(user As User) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        UPDATE Users 
                        SET Username = @u, FullName = @f, Email = @e, Role = @r, IsActive = @a
                        WHERE Id = @id;
                    "
                    cmd.Parameters.AddWithValue("@id", user.Id)
                    cmd.Parameters.AddWithValue("@u", user.Username)
                    cmd.Parameters.AddWithValue("@f", user.FullName)
                    cmd.Parameters.AddWithValue("@e", user.Email)
                    cmd.Parameters.AddWithValue("@r", CInt(user.Role))
                    cmd.Parameters.AddWithValue("@a", If(user.IsActive, 1, 0))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function UpdatePassword(userId As Integer, newHash As String, newSalt As String) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "UPDATE Users SET PasswordHash = @p, Salt = @s WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", userId)
                    cmd.Parameters.AddWithValue("@p", newHash)
                    cmd.Parameters.AddWithValue("@s", newSalt)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function UpdateLastLogin(userId As Integer) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "UPDATE Users SET LastLogin = @ll WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", userId)
                    cmd.Parameters.AddWithValue("@ll", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(userId As Integer) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM Users WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", userId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Private Function MapUser(reader As SqliteDataReader) As User
            Dim u As New User()
            u.Id = reader.GetInt32(0)
            u.Username = reader.GetString(1)
            u.PasswordHash = reader.GetString(2)
            u.Salt = reader.GetString(3)
            u.FullName = reader.GetString(4)
            u.Email = reader.GetString(5)
            u.Role = CType(reader.GetInt32(6), UserRole)
            u.IsActive = (reader.GetInt32(7) = 1)
            u.CreatedAt = DateTime.Parse(reader.GetString(8))
            If Not reader.IsDBNull(9) Then
                u.LastLogin = DateTime.Parse(reader.GetString(9))
            End If
            Return u
        End Function
    End Class
End Namespace
