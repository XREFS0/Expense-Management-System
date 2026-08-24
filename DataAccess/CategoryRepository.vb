Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Models

Namespace DataAccess
    Public Class CategoryRepository
        Public Function GetById(id As Integer) As Category
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Name, Type, ColorHex, Icon, Description, CreatedAt FROM Categories WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", id)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapCategory(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetAll(Optional type As Nullable(Of CategoryType) = Nothing) As List(Of Category)
            Dim list As New List(Of Category)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    If type.HasValue Then
                        cmd.CommandText = "SELECT Id, Name, Type, ColorHex, Icon, Description, CreatedAt FROM Categories WHERE Type = @t ORDER BY Name ASC;"
                        cmd.Parameters.AddWithValue("@t", CInt(type.Value))
                    Else
                        cmd.CommandText = "SELECT Id, Name, Type, ColorHex, Icon, Description, CreatedAt FROM Categories ORDER BY Type ASC, Name ASC;"
                    End If

                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapCategory(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Insert(cat As Category) As Integer
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        INSERT INTO Categories (Name, Type, ColorHex, Icon, Description, CreatedAt)
                        VALUES (@n, @t, @c, @i, @d, @ca);
                        SELECT last_insert_rowid();
                    "
                    cmd.Parameters.AddWithValue("@n", cat.Name)
                    cmd.Parameters.AddWithValue("@t", CInt(cat.Type))
                    cmd.Parameters.AddWithValue("@c", cat.ColorHex)
                    cmd.Parameters.AddWithValue("@i", cat.Icon)
                    cmd.Parameters.AddWithValue("@d", If(String.IsNullOrEmpty(cat.Description), DBNull.Value, CObj(cat.Description)))
                    cmd.Parameters.AddWithValue("@ca", cat.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    cat.Id = Convert.ToInt32(cmd.ExecuteScalar())
                    Return cat.Id
                End Using
            End Using
        End Function

        Public Function Update(cat As Category) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        UPDATE Categories 
                        SET Name = @n, Type = @t, ColorHex = @c, Icon = @i, Description = @d
                        WHERE Id = @id;
                    "
                    cmd.Parameters.AddWithValue("@id", cat.Id)
                    cmd.Parameters.AddWithValue("@n", cat.Name)
                    cmd.Parameters.AddWithValue("@t", CInt(cat.Type))
                    cmd.Parameters.AddWithValue("@c", cat.ColorHex)
                    cmd.Parameters.AddWithValue("@i", cat.Icon)
                    cmd.Parameters.AddWithValue("@d", If(String.IsNullOrEmpty(cat.Description), DBNull.Value, CObj(cat.Description)))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(categoryId As Integer) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM Categories WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", categoryId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function IsInUse(categoryId As Integer) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        SELECT 
                            (SELECT COUNT(*) FROM Expenses WHERE CategoryId = @id) +
                            (SELECT COUNT(*) FROM Income WHERE CategoryId = @id);
                    "
                    cmd.Parameters.AddWithValue("@id", categoryId)
                    Dim count As Long = Convert.ToInt64(cmd.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        End Function

        Private Function MapCategory(reader As SqliteDataReader) As Category
            Dim c As New Category()
            c.Id = reader.GetInt32(0)
            c.Name = reader.GetString(1)
            c.Type = CType(reader.GetInt32(2), CategoryType)
            c.ColorHex = reader.GetString(3)
            c.Icon = reader.GetString(4)
            c.Description = If(reader.IsDBNull(5), String.Empty, reader.GetString(5))
            c.CreatedAt = DateTime.Parse(reader.GetString(6))
            Return c
        End Function
    End Class
End Namespace
