Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models

Namespace Business.Services
    Public Class CategoryService
        Private ReadOnly _categoryRepo As New CategoryRepository()
        Private ReadOnly _auditRepo As New AuditLogRepository()

        Public Function GetAllCategories(Optional type As Nullable(Of CategoryType) = Nothing) As List(Of Category)
            Return _categoryRepo.GetAll(type)
        End Function

        Public Function GetCategoryById(id As Integer) As Category
            Return _categoryRepo.GetById(id)
        End Function

        Public Function CreateCategory(name As String, type As CategoryType, colorHex As String, icon As String, description As String) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(name) Then Return (False, "Category name is required.")

            Dim cat As New Category() With {
                .Name = name.Trim(),
                .Type = type,
                .ColorHex = If(String.IsNullOrWhiteSpace(colorHex), "#3699FF", colorHex.Trim()),
                .Icon = If(String.IsNullOrWhiteSpace(icon), "tag", icon.Trim()),
                .Description = If(description Is Nothing, String.Empty, description.Trim()),
                .CreatedAt = DateTime.Now
            }

            Try
                Dim id As Integer = _categoryRepo.Insert(cat)
                If id > 0 Then
                    If AuthService.CurrentUser IsNot Nothing Then
                        _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Create, "Categories", id, $"Created category {cat.Name} ({type})")
                    End If
                    Return (True, "Category created successfully.")
                End If
            Catch ex As Exception
                Return (False, $"Failed to create category: {ex.Message}")
            End Try

            Return (False, "Failed to create category.")
        End Function

        Public Function UpdateCategory(id As Integer, name As String, type As CategoryType, colorHex As String, icon As String, description As String) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(name) Then Return (False, "Category name is required.")

            Dim cat As Category = _categoryRepo.GetById(id)
            If cat Is Nothing Then Return (False, "Category not found.")

            cat.Name = name.Trim()
            cat.Type = type
            cat.ColorHex = If(String.IsNullOrWhiteSpace(colorHex), "#3699FF", colorHex.Trim())
            cat.Icon = If(String.IsNullOrWhiteSpace(icon), "tag", icon.Trim())
            cat.Description = If(description Is Nothing, String.Empty, description.Trim())

            Try
                Dim success As Boolean = _categoryRepo.Update(cat)
                If success Then
                    If AuthService.CurrentUser IsNot Nothing Then
                        _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Update, "Categories", id, $"Updated category {cat.Name}")
                    End If
                    Return (True, "Category updated successfully.")
                End If
            Catch ex As Exception
                Return (False, $"Failed to update category: {ex.Message}")
            End Try

            Return (False, "Failed to update category.")
        End Function

        Public Function DeleteCategory(id As Integer) As (Success As Boolean, Message As String)
            Dim cat As Category = _categoryRepo.GetById(id)
            If cat Is Nothing Then Return (False, "Category not found.")

            If _categoryRepo.IsInUse(id) Then
                Return (False, "Cannot delete this category because it contains associated expenses or income records.")
            End If

            Dim success As Boolean = _categoryRepo.Delete(id)
            If success Then
                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Delete, "Categories", id, $"Deleted category {cat.Name}")
                End If
                Return (True, "Category deleted successfully.")
            End If

            Return (False, "Failed to delete category.")
        End Function
    End Class
End Namespace
