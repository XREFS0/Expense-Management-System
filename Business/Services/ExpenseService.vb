Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models

Namespace Business.Services
    Public Class ExpenseService
        Private ReadOnly _expenseRepo As New ExpenseRepository()
        Private ReadOnly _auditRepo As New AuditLogRepository()

        Public Function GetAllExpenses(Optional categoryId As Nullable(Of Integer) = Nothing,
                                      Optional startDate As Nullable(Of DateTime) = Nothing,
                                      Optional endDate As Nullable(Of DateTime) = Nothing,
                                      Optional searchText As String = Nothing) As List(Of Expense)
            Return _expenseRepo.GetAll(categoryId, startDate, endDate, searchText)
        End Function

        Public Function GetExpenseById(id As Integer) As Expense
            Return _expenseRepo.GetById(id)
        End Function

        Public Function AddExpense(title As String, categoryId As Integer, amount As Decimal, paymentMethod As PaymentMethod, expenseDate As DateTime, notes As String, userId As Integer) As (Success As Boolean, Message As String, Id As Integer)
            If String.IsNullOrWhiteSpace(title) Then Return (False, "Expense title is required.", 0)
            If categoryId <= 0 Then Return (False, "Please select a valid category.", 0)
            If amount <= 0 Then Return (False, "Expense amount must be greater than zero.", 0)

            Dim exp As New Expense() With {
                .Title = title.Trim(),
                .CategoryId = categoryId,
                .Amount = amount,
                .PaymentMethod = paymentMethod,
                .ExpenseDate = expenseDate,
                .Notes = If(notes Is Nothing, String.Empty, notes.Trim()),
                .CreatedBy = userId,
                .CreatedAt = DateTime.Now,
                .UpdatedAt = DateTime.Now
            }

            Try
                Dim id As Integer = _expenseRepo.Insert(exp)
                If id > 0 Then
                    If AuthService.CurrentUser IsNot Nothing Then
                        _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Create, "Expenses", id, $"Created expense '{exp.Title}' of {exp.Amount:N2}")
                    End If
                    Return (True, "Expense added successfully.", id)
                End If
            Catch ex As Exception
                Return (False, $"Failed to add expense: {ex.Message}", 0)
            End Try

            Return (False, "Failed to add expense.", 0)
        End Function

        Public Function UpdateExpense(id As Integer, title As String, categoryId As Integer, amount As Decimal, paymentMethod As PaymentMethod, expenseDate As DateTime, notes As String) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(title) Then Return (False, "Expense title is required.")
            If categoryId <= 0 Then Return (False, "Please select a valid category.")
            If amount <= 0 Then Return (False, "Expense amount must be greater than zero.")

            Dim exp As Expense = _expenseRepo.GetById(id)
            If exp Is Nothing Then Return (False, "Expense record not found.")

            exp.Title = title.Trim()
            exp.CategoryId = categoryId
            exp.Amount = amount
            exp.PaymentMethod = paymentMethod
            exp.ExpenseDate = expenseDate
            exp.Notes = If(notes Is Nothing, String.Empty, notes.Trim())

            Try
                Dim success As Boolean = _expenseRepo.Update(exp)
                If success Then
                    If AuthService.CurrentUser IsNot Nothing Then
                        _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Update, "Expenses", id, $"Updated expense '{exp.Title}' of {exp.Amount:N2}")
                    End If
                    Return (True, "Expense updated successfully.")
                End If
            Catch ex As Exception
                Return (False, $"Failed to update expense: {ex.Message}")
            End Try

            Return (False, "Failed to update expense.")
        End Function

        Public Function DeleteExpense(id As Integer) As (Success As Boolean, Message As String)
            Dim exp As Expense = _expenseRepo.GetById(id)
            If exp Is Nothing Then Return (False, "Expense record not found.")

            Dim success As Boolean = _expenseRepo.Delete(id)
            If success Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Delete, "Expenses", id, $"Deleted expense '{exp.Title}' ({exp.Amount:N2})")
                End If
                Return (True, "Expense deleted successfully.")
            End If

            Return (False, "Failed to delete expense.")
        End Function
    End Class
End Namespace
