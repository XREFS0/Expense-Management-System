Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Models

Namespace DataAccess
    Public Class ExpenseRepository
        Public Function GetById(id As Integer) As Expense
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        SELECT e.Id, e.Title, e.CategoryId, c.Name, c.ColorHex, e.Amount, e.PaymentMethod, 
                               e.ExpenseDate, e.Notes, e.CreatedBy, u.FullName, e.CreatedAt, e.UpdatedAt
                        FROM Expenses e
                        INNER JOIN Categories c ON e.CategoryId = c.Id
                        INNER JOIN Users u ON e.CreatedBy = u.Id
                        WHERE e.Id = @id;
                    "
                    cmd.Parameters.AddWithValue("@id", id)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapExpense(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetAll(Optional categoryId As Nullable(Of Integer) = Nothing,
                               Optional startDate As Nullable(Of DateTime) = Nothing,
                               Optional endDate As Nullable(Of DateTime) = Nothing,
                               Optional searchText As String = Nothing) As List(Of Expense)
            Dim list As New List(Of Expense)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    Dim sql As String = "
                        SELECT e.Id, e.Title, e.CategoryId, c.Name, c.ColorHex, e.Amount, e.PaymentMethod, 
                               e.ExpenseDate, e.Notes, e.CreatedBy, u.FullName, e.CreatedAt, e.UpdatedAt
                        FROM Expenses e
                        INNER JOIN Categories c ON e.CategoryId = c.Id
                        INNER JOIN Users u ON e.CreatedBy = u.Id
                        WHERE 1=1
                    "

                    If categoryId.HasValue AndAlso categoryId.Value > 0 Then
                        sql &= " AND e.CategoryId = @catId"
                        cmd.Parameters.AddWithValue("@catId", categoryId.Value)
                    End If

                    If startDate.HasValue Then
                        sql &= " AND date(e.ExpenseDate) >= date(@start)"
                        cmd.Parameters.AddWithValue("@start", startDate.Value.ToString("yyyy-MM-dd"))
                    End If

                    If endDate.HasValue Then
                        sql &= " AND date(e.ExpenseDate) <= date(@end)"
                        cmd.Parameters.AddWithValue("@end", endDate.Value.ToString("yyyy-MM-dd"))
                    End If

                    If Not String.IsNullOrWhiteSpace(searchText) Then
                        sql &= " AND (e.Title LIKE @search OR e.Notes LIKE @search OR c.Name LIKE @search)"
                        cmd.Parameters.AddWithValue("@search", "%" & searchText.Trim() & "%")
                    End If

                    sql &= " ORDER BY e.ExpenseDate DESC, e.Id DESC;"
                    cmd.CommandText = sql

                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapExpense(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Insert(exp As Expense) As Integer
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        INSERT INTO Expenses (Title, CategoryId, Amount, PaymentMethod, ExpenseDate, Notes, CreatedBy, CreatedAt, UpdatedAt)
                        VALUES (@title, @catId, @amt, @pm, @ed, @notes, @cb, @ca, @ua);
                        SELECT last_insert_rowid();
                    "
                    cmd.Parameters.AddWithValue("@title", exp.Title)
                    cmd.Parameters.AddWithValue("@catId", exp.CategoryId)
                    cmd.Parameters.AddWithValue("@amt", exp.Amount)
                    cmd.Parameters.AddWithValue("@pm", CInt(exp.PaymentMethod))
                    cmd.Parameters.AddWithValue("@ed", exp.ExpenseDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@notes", If(String.IsNullOrEmpty(exp.Notes), DBNull.Value, CObj(exp.Notes)))
                    cmd.Parameters.AddWithValue("@cb", exp.CreatedBy)
                    cmd.Parameters.AddWithValue("@ca", exp.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("@ua", exp.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    exp.Id = Convert.ToInt32(cmd.ExecuteScalar())
                    Return exp.Id
                End Using
            End Using
        End Function

        Public Function Update(exp As Expense) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        UPDATE Expenses
                        SET Title = @title, CategoryId = @catId, Amount = @amt, PaymentMethod = @pm, 
                            ExpenseDate = @ed, Notes = @notes, UpdatedAt = @ua
                        WHERE Id = @id;
                    "
                    cmd.Parameters.AddWithValue("@id", exp.Id)
                    cmd.Parameters.AddWithValue("@title", exp.Title)
                    cmd.Parameters.AddWithValue("@catId", exp.CategoryId)
                    cmd.Parameters.AddWithValue("@amt", exp.Amount)
                    cmd.Parameters.AddWithValue("@pm", CInt(exp.PaymentMethod))
                    cmd.Parameters.AddWithValue("@ed", exp.ExpenseDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@notes", If(String.IsNullOrEmpty(exp.Notes), DBNull.Value, CObj(exp.Notes)))
                    cmd.Parameters.AddWithValue("@ua", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM Expenses WHERE Id = @id;"
                    cmd.Parameters.AddWithValue("@id", id)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function GetTotalExpenses(Optional startDate As Nullable(Of DateTime) = Nothing, Optional endDate As Nullable(Of DateTime) = Nothing) As Decimal
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    Dim sql As String = "SELECT COALESCE(SUM(Amount), 0) FROM Expenses WHERE 1=1"
                    If startDate.HasValue Then
                        sql &= " AND date(ExpenseDate) >= date(@start)"
                        cmd.Parameters.AddWithValue("@start", startDate.Value.ToString("yyyy-MM-dd"))
                    End If
                    If endDate.HasValue Then
                        sql &= " AND date(ExpenseDate) <= date(@end)"
                        cmd.Parameters.AddWithValue("@end", endDate.Value.ToString("yyyy-MM-dd"))
                    End If
                    cmd.CommandText = sql
                    Return Convert.ToDecimal(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Private Function MapExpense(reader As SqliteDataReader) As Expense
            Dim exp As New Expense()
            exp.Id = reader.GetInt32(0)
            exp.Title = reader.GetString(1)
            exp.CategoryId = reader.GetInt32(2)
            exp.CategoryName = reader.GetString(3)
            exp.CategoryColor = reader.GetString(4)
            exp.Amount = reader.GetDecimal(5)
            exp.PaymentMethod = CType(reader.GetInt32(6), PaymentMethod)
            exp.ExpenseDate = DateTime.Parse(reader.GetString(7))
            exp.Notes = If(reader.IsDBNull(8), String.Empty, reader.GetString(8))
            exp.CreatedBy = reader.GetInt32(9)
            exp.CreatorName = reader.GetString(10)
            exp.CreatedAt = DateTime.Parse(reader.GetString(11))
            exp.UpdatedAt = DateTime.Parse(reader.GetString(12))
            Return exp
        End Function
    End Class
End Namespace
