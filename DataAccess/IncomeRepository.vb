Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Models

Namespace DataAccess
    Public Class IncomeRepository
        Public Function GetById(id As Integer) As Income
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        SELECT i.Id, i.Source, i.CategoryId, c.Name, i.Amount, 
                               i.IncomeDate, i.Notes, i.CreatedBy, u.FullName, i.CreatedAt, i.UpdatedAt
                        FROM Income i
                        INNER JOIN Categories c ON i.CategoryId = c.Id
                        INNER JOIN Users u ON i.CreatedBy = u.Id
                        WHERE i.Id = @id;
                    "
                    cmd.Parameters.AddWithValue("@id", id)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapIncome(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetAll(Optional categoryId As Nullable(Of Integer) = Nothing,
                               Optional startDate As Nullable(Of DateTime) = Nothing,
                               Optional endDate As Nullable(Of DateTime) = Nothing,
                               Optional searchText As String = Nothing) As List(Of Income)
            Dim list As New List(Of Income)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    Dim sql As String = "
                        SELECT i.Id, i.Source, i.CategoryId, c.Name, i.Amount, 
                               i.IncomeDate, i.Notes, i.CreatedBy, u.FullName, i.CreatedAt, i.UpdatedAt
                        FROM Income i
                        INNER JOIN Categories c ON i.CategoryId = c.Id
                        INNER JOIN Users u ON i.CreatedBy = u.Id
                        WHERE 1=1
                    "

                    If categoryId.HasValue AndAlso categoryId.Value > 0 Then
                        sql &= " AND i.CategoryId = @catId"
                        cmd.Parameters.AddWithValue("@catId", categoryId.Value)
                    End If

                    If startDate.HasValue Then
                        sql &= " AND date(i.IncomeDate) >= date(@start)"
                        cmd.Parameters.AddWithValue("@start", startDate.Value.ToString("yyyy-MM-dd"))
                    End If

                    If endDate.HasValue Then
                        sql &= " AND date(i.IncomeDate) <= date(@end)"
                        cmd.Parameters.AddWithValue("@end", endDate.Value.ToString("yyyy-MM-dd"))
                    End If

                    If Not String.IsNullOrWhiteSpace(searchText) Then
                        sql &= " AND (i.Source LIKE @search OR i.Notes LIKE @search OR c.Name LIKE @search)"
                        cmd.Parameters.AddWithValue("@search", "%" & searchText.Trim() & "%")
                    End If

                    sql &= " ORDER BY i.IncomeDate DESC, i.Id DESC;"
                    cmd.CommandText = sql

                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapIncome(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Insert(inc As Income) As Integer
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        INSERT INTO Income (Source, CategoryId, Amount, IncomeDate, Notes, CreatedBy, CreatedAt, UpdatedAt)
                        VALUES (@source, @catId, @amt, @idate, @notes, @cb, @ca, @ua);
                        SELECT last_insert_rowid();
                    "
                    cmd.Parameters.AddWithValue("@source", inc.Source)
                    cmd.Parameters.AddWithValue("@catId", inc.CategoryId)
                    cmd.Parameters.AddWithValue("@amt", inc.Amount)
                    cmd.Parameters.AddWithValue("@idate", inc.IncomeDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@notes", If(String.IsNullOrEmpty(inc.Notes), DBNull.Value, CObj(inc.Notes)))
                    cmd.Parameters.AddWithValue("@cb", inc.CreatedBy)
                    cmd.Parameters.AddWithValue("@ca", inc.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("@ua", inc.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    inc.Id = Convert.ToInt32(cmd.ExecuteScalar())
                    Return inc.Id
                End Using
            End Using
        End Function

        Public Function Update(inc As Income) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        UPDATE Income
                        SET Source = @source, CategoryId = @catId, Amount = @amt, 
                            IncomeDate = @idate, Notes = @notes, UpdatedAt = @ua
                        WHERE Id = @id;
                    "
                    cmd.Parameters.AddWithValue("@id", inc.Id)
                    cmd.Parameters.AddWithValue("@source", inc.Source)
                    cmd.Parameters.AddWithValue("@catId", inc.CategoryId)
                    cmd.Parameters.AddWithValue("@amt", inc.Amount)
                    cmd.Parameters.AddWithValue("@idate", inc.IncomeDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@notes", If(String.IsNullOrEmpty(inc.Notes), DBNull.Value, CObj(inc.Notes)))
                    cmd.Parameters.AddWithValue("@ua", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM Income WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", id)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function GetTotalIncome(Optional startDate As Nullable(Of DateTime) = Nothing, Optional endDate As Nullable(Of DateTime) = Nothing) As Decimal
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    Dim sql As String = "SELECT COALESCE(SUM(Amount), 0) FROM Income WHERE 1=1"
                    If startDate.HasValue Then
                        sql &= " AND date(IncomeDate) >= date(@start)"
                        cmd.Parameters.AddWithValue("@start", startDate.Value.ToString("yyyy-MM-dd"))
                    End If
                    If endDate.HasValue Then
                        sql &= " AND date(IncomeDate) <= date(@end)"
                        cmd.Parameters.AddWithValue("@end", endDate.Value.ToString("yyyy-MM-dd"))
                    End If
                    cmd.CommandText = sql
                    Return Convert.ToDecimal(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Private Function MapIncome(reader As SqliteDataReader) As Income
            Dim inc As New Income()
            inc.Id = reader.GetInt32(0)
            inc.Source = reader.GetString(1)
            inc.CategoryId = reader.GetInt32(2)
            inc.CategoryName = reader.GetString(3)
            inc.Amount = reader.GetDecimal(4)
            inc.IncomeDate = DateTime.Parse(reader.GetString(5))
            inc.Notes = If(reader.IsDBNull(6), String.Empty, reader.GetString(6))
            inc.CreatedBy = reader.GetInt32(7)
            inc.CreatorName = reader.GetString(8)
            inc.CreatedAt = DateTime.Parse(reader.GetString(9))
            inc.UpdatedAt = DateTime.Parse(reader.GetString(10))
            Return inc
        End Function
    End Class
End Namespace
