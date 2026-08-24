Imports System.IO
Imports Microsoft.Data.Sqlite

Namespace DataAccess
    Public Class DatabaseContext
        Private Shared ReadOnly _dbFolder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MasaExpenseManager")
        Private Shared ReadOnly _dbPath As String = Path.Combine(_dbFolder, "masa_expense.db")

        Public Shared Property CustomDatabasePath As String = _dbPath

        Public Shared Function GetConnectionString() As String
            If Not Directory.Exists(Path.GetDirectoryName(CustomDatabasePath)) Then
                Directory.CreateDirectory(Path.GetDirectoryName(CustomDatabasePath))
            End If
            Return $"Data Source={CustomDatabasePath};"
        End Function

        Public Shared Function CreateConnection() As SqliteConnection
            Dim conn As New SqliteConnection(GetConnectionString())
            conn.Open()
            Return conn
        End Function

        Public Shared ReadOnly Property DatabaseFilePath As String
            Get
                Return CustomDatabasePath
            End Get
        End Property
    End Class
End Namespace
