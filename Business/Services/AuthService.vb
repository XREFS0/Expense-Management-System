Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.Business.Security

Namespace Business.Services
    Public Class AuthService
        Private ReadOnly _userRepo As New UserRepository()
        Private ReadOnly _auditRepo As New AuditLogRepository()

        Public Shared Property CurrentUser As User = Nothing

        Public Function Login(username As String, password As String) As (Success As Boolean, Message As String, User As User)
            If String.IsNullOrWhiteSpace(username) Then
                Return (False, "Please enter your username.", Nothing)
            End If

            If String.IsNullOrEmpty(password) Then
                Return (False, "Please enter your password.", Nothing)
            End If

            Dim user As User = _userRepo.GetByUsername(username)
            If user Is Nothing Then
                Return (False, "Invalid username or password.", Nothing)
            End If

            If Not user.IsActive Then
                Return (False, "This account has been deactivated. Please contact an administrator.", Nothing)
            End If

            If Not PasswordHasher.VerifyPassword(password, user.Salt, user.PasswordHash) Then
                Return (False, "Invalid username or password.", Nothing)
            End If

            _userRepo.UpdateLastLogin(user.Id)
            CurrentUser = user

            _auditRepo.Log(user.Id, user.Username, AuditAction.Login, "Users", user.Id, "User logged in successfully")

            Return (True, "Login successful.", user)
        End Function

        Public Sub Logout()
            If CurrentUser IsNot Nothing Then
                _auditRepo.Log(CurrentUser.Id, CurrentUser.Username, AuditAction.Logout, "Users", CurrentUser.Id, "User logged out")
            End If
            CurrentUser = Nothing
        End Sub

        Public Function HasPermission(requiredRole As UserRole) As Boolean
            If CurrentUser Is Nothing Then Return False
            Return CurrentUser.Role <= requiredRole
        End Function
    End Class
End Namespace
