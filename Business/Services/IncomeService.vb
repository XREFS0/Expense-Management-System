Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models

Namespace Business.Services
    Public Class IncomeService
        Private ReadOnly _incomeRepo As New IncomeRepository()
        Private ReadOnly _auditRepo As New AuditLogRepository()

        Public Function GetAllIncome(Optional categoryId As Nullable(Of Integer) = Nothing,
                                    Optional startDate As Nullable(Of DateTime) = Nothing,
                                    Optional endDate As Nullable(Of DateTime) = Nothing,
                                    Optional searchText As String = Nothing) As List(Of Income)
            Return _incomeRepo.GetAll(categoryId, startDate, endDate, searchText)
        End Function

        Public Function GetIncomeById(id As Integer) As Income
            Return _incomeRepo.GetById(id)
        End Function

        Public Function AddIncome(source As String, categoryId As Integer, amount As Decimal, incomeDate As DateTime, notes As String, userId As Integer) As (Success As Boolean, Message As String, Id As Integer)
            If String.IsNullOrWhiteSpace(source) Then Return (False, "Income source is required.", 0)
            If categoryId <= 0 Then Return (False, "Please select a valid category.", 0)
            If amount <= 0 Then Return (False, "Income amount must be greater than zero.", 0)

            Dim inc As New Income() With {
                .Source = source.Trim(),
                .CategoryId = categoryId,
                .Amount = amount,
                .IncomeDate = incomeDate,
                .Notes = If(notes Is Nothing, String.Empty, notes.Trim()),
                .CreatedBy = userId,
                .CreatedAt = DateTime.Now,
                .UpdatedAt = DateTime.Now
            }

            Try
                Dim id As Integer = _incomeRepo.Insert(inc)
                If id > 0 Then
                    If AuthService.CurrentUser IsNot Nothing Then
                        _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Create, "Income", id, $"Recorded income '{inc.Source}' of {inc.Amount:N2}")
                    End If
                    Return (True, "Income recorded successfully.", id)
                End If
            Catch ex As Exception
                Return (False, $"Failed to record income: {ex.Message}", 0)
            End Try

            Return (False, "Failed to record income.", 0)
        End Function

        Public Function UpdateIncome(id As Integer, source As String, categoryId As Integer, amount As Decimal, incomeDate As DateTime, notes As String) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(source) Then Return (False, "Income source is required.")
            If categoryId <= 0 Then Return (False, "Please select a valid category.")
            If amount <= 0 Then Return (False, "Income amount must be greater than zero.")

            Dim inc As Income = _incomeRepo.GetById(id)
            If inc Is Nothing Then Return (False, "Income record not found.")

            inc.Source = source.Trim()
            inc.CategoryId = categoryId
            inc.Amount = amount
            inc.IncomeDate = incomeDate
            inc.Notes = If(notes Is Nothing, String.Empty, notes.Trim())

            Try
                Dim success As Boolean = _incomeRepo.Update(inc)
                If success Then
                    If AuthService.CurrentUser IsNot Nothing Then
                        _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Update, "Income", id, $"Updated income '{inc.Source}' of {inc.Amount:N2}")
                    End If
                    Return (True, "Income updated successfully.")
                End If
            Catch ex As Exception
                Return (False, $"Failed to update income: {ex.Message}")
            End Try

            Return (False, "Failed to update income.")
        End Function

        Public Function DeleteIncome(id As Integer) As (Success As Boolean, Message As String)
            Dim inc As Income = _incomeRepo.GetById(id)
            If inc Is Nothing Then Return (False, "Income record not found.")

            Dim success As Boolean = _incomeRepo.Delete(id)
            If success Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Delete, "Income", id, $"Deleted income '{inc.Source}' ({inc.Amount:N2})")
                End If
                Return (True, "Income deleted successfully.")
            End If

            Return (False, "Failed to delete income.")
        End Function
    End Class
End Namespace
