Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Models

Namespace DataAccess
    Public Class AuditLogRepository
        Public Sub Log(userId As Integer, username As String, action As AuditAction, entityName As String, Optional entityId As Nullable(Of Integer) = Nothing, Optional details As String = "")
            Try
                Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "
                            INSERT INTO AuditLogs (UserId, Username, Action, EntityName, EntityId, Details, IpAddress, Timestamp)
                            VALUES (@u, @un, @a, @en, @eid, @d, @ip, @ts);
                        "
                        cmd.Parameters.AddWithValue("@u", userId)
                        cmd.Parameters.AddWithValue("@un", username)
                        cmd.Parameters.AddWithValue("@a", CInt(action))
                        cmd.Parameters.AddWithValue("@en", entityName)
                        cmd.Parameters.AddWithValue("@eid", If(entityId.HasValue, CObj(entityId.Value), DBNull.Value))
                        cmd.Parameters.AddWithValue("@d", If(String.IsNullOrEmpty(details), DBNull.Value, CObj(details)))
                        cmd.Parameters.AddWithValue("@ip", "127.0.0.1")
                        cmd.Parameters.AddWithValue("@ts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch
            End Try
        End Sub

        Public Function GetAll(Optional limit As Integer = 100) As List(Of AuditLog)
            Dim list As New List(Of AuditLog)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        SELECT Id, UserId, Username, Action, EntityName, EntityId, Details, IpAddress, Timestamp
                        FROM AuditLogs
                        ORDER BY Timestamp DESC, Id DESC
                        LIMIT @lim;
                    "
                    cmd.Parameters.AddWithValue("@lim", limit)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim log As New AuditLog()
                            log.Id = reader.GetInt32(0)
                            log.UserId = reader.GetInt32(1)
                            log.Username = reader.GetString(2)
                            log.Action = CType(reader.GetInt32(3), AuditAction)
                            log.EntityName = reader.GetString(4)
                            log.EntityId = If(reader.IsDBNull(5), Nothing, CType(reader.GetInt32(5), Nullable(Of Integer)))
                            log.Details = If(reader.IsDBNull(6), String.Empty, reader.GetString(6))
                            log.IpAddress = If(reader.IsDBNull(7), "127.0.0.1", reader.GetString(7))
                            log.Timestamp = DateTime.Parse(reader.GetString(8))
                            list.Add(log)
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function
    End Class
End Namespace
