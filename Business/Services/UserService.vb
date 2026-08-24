Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.Business.Security

Namespace Business.Services
    Public Class UserService
        Private ReadOnly _userRepo As New UserRepository()
        Private ReadOnly _auditRepo As New AuditLogRepository()

        Public Function GetAllUsers() As List(Of User)
            Return _userRepo.GetAll()
        End Function

        Public Function GetUserById(id As Integer) As User
            Return _userRepo.GetById(id)
        End Function

        Public Function CreateUser(username As String, password As String, fullName As String, email As String, role As UserRole, isActive As Boolean) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(username) Then Return (False, "Username is required.")
            If String.IsNullOrWhiteSpace(password) Then Return (False, "Password is required.")
            If password.Length < 6 Then Return (False, "Password must be at least 6 characters.")
            If String.IsNullOrWhiteSpace(fullName) Then Return (False, "Full name is required.")

            Dim existing As User = _userRepo.GetByUsername(username)
            If existing IsNot Nothing Then
                Return (False, "Username is already taken.")
            End If

            Dim salt As String = PasswordHasher.GenerateSalt()
            Dim hash As String = PasswordHasher.HashPassword(password, salt)

            Dim user As New User() With {
                .Username = username.Trim(),
                .PasswordHash = hash,
                .Salt = salt,
                .FullName = fullName.Trim(),
                .Email = If(email Is Nothing, String.Empty, email.Trim()),
                .Role = role,
                .IsActive = isActive,
                .CreatedAt = DateTime.Now
            }

            Dim id As Integer = _userRepo.Insert(user)
            If id > 0 Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Create, "Users", id, $"Created user {user.Username} ({role})")
                End If
                Return (True, "User created successfully.")
            End If

            Return (False, "Failed to create user.")
        End Function

        Public Function UpdateUser(id As Integer, username As String, fullName As String, email As String, role As UserRole, isActive As Boolean) As (Success As Boolean, Message As String)
            Dim user As User = _userRepo.GetById(id)
            If user Is Nothing Then Return (False, "User not found.")

            Dim existing As User = _userRepo.GetByUsername(username)
            If existing IsNot Nothing AndAlso existing.Id <> id Then
                Return (False, "Username is already used by another account.")
            End If

            user.Username = username.Trim()
            user.FullName = fullName.Trim()
            user.Email = If(email Is Nothing, String.Empty, email.Trim())
            user.Role = role
            user.IsActive = isActive

            Dim success As Boolean = _userRepo.Update(user)
            If success Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Update, "Users", id, $"Updated user {user.Username}")
                End If
                Return (True, "User updated successfully.")
            End If

            Return (False, "Failed to update user.")
        End Function

        Public Function ChangePassword(userId As Integer, currentPassword As String, newPassword As String) As (Success As Boolean, Message As String)
            Dim user As User = _userRepo.GetById(userId)
            If user Is Nothing Then Return (False, "User not found.")

            If Not PasswordHasher.VerifyPassword(currentPassword, user.Salt, user.PasswordHash) Then
                Return (False, "Current password is incorrect.")
            End If

            If String.IsNullOrWhiteSpace(newPassword) OrElse newPassword.Length < 6 Then
                Return (False, "New password must be at least 6 characters.")
            End If

            Dim newSalt As String = PasswordHasher.GenerateSalt()
            Dim newHash As String = PasswordHasher.HashPassword(newPassword, newSalt)

            Dim success As Boolean = _userRepo.UpdatePassword(userId, newHash, newSalt)
            If success Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Update, "Users", userId, "Changed account password")
                End If
                Return (True, "Password changed successfully.")
            End If

            Return (False, "Failed to change password.")
        End Function

        Public Function ResetUserPassword(userId As Integer, newPassword As String) As (Success As Boolean, Message As String)
            Dim user As User = _userRepo.GetById(userId)
            If user Is Nothing Then Return (False, "User not found.")

            If String.IsNullOrWhiteSpace(newPassword) OrElse newPassword.Length < 6 Then
                Return (False, "New password must be at least 6 characters.")
            End If

            Dim newSalt As String = PasswordHasher.GenerateSalt()
            Dim newHash As String = PasswordHasher.HashPassword(newPassword, newSalt)

            Dim success As Boolean = _userRepo.UpdatePassword(userId, newHash, newSalt)
            If success Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Update, "Users", userId, $"Reset password for user {user.Username}")
                End If
                Return (True, "Password reset successfully.")
            End If

            Return (False, "Failed to reset password.")
        End Function

        Public Function DeleteUser(userId As Integer) As (Success As Boolean, Message As String)
            If AuthService.CurrentUser IsNot Nothing AndAlso AuthService.CurrentUser.Id = userId Then
                Return (False, "You cannot delete your own logged-in account.")
            End If

            Dim user As User = _userRepo.GetById(userId)
            If user Is Nothing Then Return (False, "User not found.")

            Dim success As Boolean = _userRepo.Delete(userId)
            If success Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Delete, "Users", userId, $"Deleted user {user.Username}")
                End If
                Return (True, "User deleted successfully.")
            End If

            Return (False, "Failed to delete user.")
        End Function
    End Class
End Namespace
