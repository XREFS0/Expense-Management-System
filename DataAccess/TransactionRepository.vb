Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Models

Namespace DataAccess
    Public Class TransactionRepository
        Public Function GetRecent(limit As Integer) As List(Of Transaction)
            Dim list As New List(Of Transaction)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        SELECT Id, ReferenceId, Type, Title, CategoryName, Amount, TransactionDate, PaymentMethod, Notes, UserName
                        FROM (
                            SELECT e.Id AS Id, e.Id AS ReferenceId, 1 AS Type, e.Title AS Title, 
                                   c.Name AS CategoryName, e.Amount AS Amount, e.ExpenseDate AS TransactionDate, 
                                   CASE e.PaymentMethod
                                       WHEN 1 THEN 'Cash'
                                       WHEN 2 THEN 'Credit Card'
                                       WHEN 3 THEN 'Debit Card'
                                       WHEN 4 THEN 'Bank Transfer'
                                       WHEN 5 THEN 'E-Wallet'
                                       ELSE 'Other'
                                   END AS PaymentMethod, 
                                   e.Notes AS Notes, u.FullName AS UserName
                            FROM Expenses e
                            INNER JOIN Categories c ON e.CategoryId = c.Id
                            INNER JOIN Users u ON e.CreatedBy = u.Id

                            UNION ALL

                            SELECT i.Id AS Id, i.Id AS ReferenceId, 2 AS Type, i.Source AS Title, 
                                   c.Name AS CategoryName, i.Amount AS Amount, i.IncomeDate AS TransactionDate, 
                                   'N/A' AS PaymentMethod, 
                                   i.Notes AS Notes, u.FullName AS UserName
                            FROM Income i
                            INNER JOIN Categories c ON i.CategoryId = c.Id
                            INNER JOIN Users u ON i.CreatedBy = u.Id
                        )
                        ORDER BY date(TransactionDate) DESC, Id DESC
                        LIMIT @lim;
                    "
                    cmd.Parameters.AddWithValue("@lim", limit)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim t As New Transaction()
                            t.Id = reader.GetInt32(0)
                            t.ReferenceId = reader.GetInt32(1)
                            t.Type = CType(reader.GetInt32(2), TransactionType)
                            t.Title = reader.GetString(3)
                            t.CategoryName = reader.GetString(4)
                            t.Amount = reader.GetDecimal(5)
                            t.TransactionDate = DateTime.Parse(reader.GetString(6))
                            t.PaymentMethod = reader.GetString(7)
                            t.Notes = If(reader.IsDBNull(8), String.Empty, reader.GetString(8))
                            t.UserName = reader.GetString(9)
                            list.Add(t)
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetCategoryBreakdown(startDate As DateTime, endDate As DateTime) As List(Of CategoryBreakdownItem)
            Dim list As New List(Of CategoryBreakdownItem)()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        SELECT c.Name, c.ColorHex, COALESCE(SUM(e.Amount), 0) AS Total
                        FROM Categories c
                        INNER JOIN Expenses e ON c.Id = e.CategoryId
                        WHERE date(e.ExpenseDate) >= date(@start) AND date(e.ExpenseDate) <= date(@end)
                        GROUP BY c.Id, c.Name, c.ColorHex
                        ORDER BY Total DESC;
                    "
                    cmd.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@end", endDate.ToString("yyyy-MM-dd"))
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        Dim grandTotal As Decimal = 0
                        Dim rawList As New List(Of (Name As String, Color As String, Total As Decimal))()
                        While reader.Read()
                            Dim name As String = reader.GetString(0)
                            Dim color As String = reader.GetString(1)
                            Dim total As Decimal = reader.GetDecimal(2)
                            grandTotal += total
                            rawList.Add((name, color, total))
                        End While

                        For Each item In rawList
                            Dim percentage As Double = If(grandTotal > 0, Math.Round((CDbl(item.Total) / CDbl(grandTotal)) * 100.0, 1), 0.0)
                            list.Add(New CategoryBreakdownItem() With {
                                .CategoryName = item.Name,
                                .ColorHex = item.Color,
                                .TotalAmount = item.Total,
                                .Percentage = percentage
                            })
                        Next
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetMonthlyTrends(monthsBack As Integer) As List(Of MonthlyTrendItem)
            Dim list As New List(Of MonthlyTrendItem)()
            Dim today As DateTime = DateTime.Today

            For i As Integer = monthsBack - 1 To 0 Step -1
                Dim targetMonth As DateTime = today.AddMonths(-i)
                Dim startOfMonth As New DateTime(targetMonth.Year, targetMonth.Month, 1)
                Dim endOfMonth As DateTime = startOfMonth.AddMonths(1).AddDays(-1)

                Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                    Dim expTotal As Decimal = 0
                    Dim incTotal As Decimal = 0

                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "SELECT COALESCE(SUM(Amount), 0) FROM Expenses WHERE date(ExpenseDate) >= date(@s) AND date(ExpenseDate) <= date(@e);"
                        cmd.Parameters.AddWithValue("@s", startOfMonth.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("@e", endOfMonth.ToString("yyyy-MM-dd"))
                        expTotal = Convert.ToDecimal(cmd.ExecuteScalar())
                    End Using

                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "SELECT COALESCE(SUM(Amount), 0) FROM Income WHERE date(IncomeDate) >= date(@s) AND date(IncomeDate) <= date(@e);"
                        cmd.Parameters.AddWithValue("@s", startOfMonth.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("@e", endOfMonth.ToString("yyyy-MM-dd"))
                        incTotal = Convert.ToDecimal(cmd.ExecuteScalar())
                    End Using

                    list.Add(New MonthlyTrendItem() With {
                        .MonthName = startOfMonth.ToString("MMM yyyy"),
                        .ExpenseAmount = expTotal,
                        .IncomeAmount = incTotal
                    })
                End Using
            Next

            Return list
        End Function
    End Class
End Namespace
