Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Models

Namespace DataAccess
    Public Class SettingsRepository
        Public Function GetValue(key As String, Optional defaultValue As String = "") As String
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "SELECT Value FROM Settings WHERE Key = @k;"
                    cmd.Parameters.AddWithValue("@k", key)
                    Dim res As Object = cmd.ExecuteScalar()
                    If res IsNot Nothing AndAlso Not Convert.IsDBNull(res) Then
                        Return Convert.ToString(res)
                    End If
                End Using
            End Using
            Return defaultValue
        End Function

        Public Function SetValue(key As String, value As String, Optional description As String = "") As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        INSERT INTO Settings (Key, Value, Description, UpdatedAt)
                        VALUES (@k, @v, @d, @u)
                        ON CONFLICT(Key) DO UPDATE SET Value = @v, Description = COALESCE(NULLIF(@d, ''), Description), UpdatedAt = @u;
                    "
                    cmd.Parameters.AddWithValue("@k", key)
                    cmd.Parameters.AddWithValue("@v", value)
                    cmd.Parameters.AddWithValue("@d", description)
                    cmd.Parameters.AddWithValue("@u", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function GetAll() As Dictionary(Of String, String)
            Dim dict As New Dictionary(Of String, String)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "SELECT Key, Value FROM Settings;"
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            dict(reader.GetString(0)) = reader.GetString(1)
                        End While
                    End Using
                End Using
            End Using
            Return dict
        End Function
    End Class
End Namespace
