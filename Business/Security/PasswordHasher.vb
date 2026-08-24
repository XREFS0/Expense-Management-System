Imports System.Security.Cryptography
Imports System.Text

Namespace Business.Security
    Public Class PasswordHasher
        Public Shared Function GenerateSalt() As String
            Dim saltBytes(15) As Byte
            Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
                rng.GetBytes(saltBytes)
            End Using
            Return Convert.ToBase64String(saltBytes)
        End Function

        Public Shared Function HashPassword(password As String, salt As String) As String
            If String.IsNullOrEmpty(password) Then Return String.Empty
            Using sha256 As SHA256 = SHA256.Create()
                Dim combinedBytes As Byte() = Encoding.UTF8.GetBytes(password & salt)
                Dim hashBytes As Byte() = sha256.ComputeHash(combinedBytes)
                Return Convert.ToBase64String(hashBytes)
            End Using
        End Function

        Public Shared Function VerifyPassword(password As String, salt As String, expectedHash As String) As Boolean
            If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(expectedHash) Then
                Return False
            End If
            Dim actualHash As String = HashPassword(password, salt)
            Return actualHash.Equals(expectedHash, StringComparison.Ordinal)
        End Function
    End Class
End Namespace
