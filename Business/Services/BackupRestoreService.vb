Imports System.IO
Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models

Namespace Business.Services
    Public Class BackupRestoreService
        Private ReadOnly _auditRepo As New AuditLogRepository()

        Public Function CreateBackup(targetFilePath As String) As (Success As Boolean, Message As String)
            Try
                Dim sourceDb As String = DatabaseContext.DatabaseFilePath
                If Not File.Exists(sourceDb) Then
                    Return (False, "Source database file does not exist.")
                End If

                Dim targetDir As String = Path.GetDirectoryName(targetFilePath)
                If Not String.IsNullOrEmpty(targetDir) AndAlso Not Directory.Exists(targetDir) Then
                    Directory.CreateDirectory(targetDir)
                End If

                File.Copy(sourceDb, targetFilePath, True)

                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Backup, "Database", Nothing, $"Database backup created at {targetFilePath}")
                End If

                Return (True, $"Backup successfully saved to: {targetFilePath}")
            Catch ex As Exception
                Return (False, $"Backup failed: {ex.Message}")
            End Try
        End Function

        Public Function RestoreBackup(backupFilePath As String) As (Success As Boolean, Message As String)
            Try
                If Not File.Exists(backupFilePath) Then
                    Return (False, "Selected backup file does not exist.")
                End If

                Dim targetDb As String = DatabaseContext.DatabaseFilePath

                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools()
                GC.Collect()
                GC.WaitForPendingFinalizers()

                Dim tempOldDb As String = targetDb & ".old"
                If File.Exists(targetDb) Then
                    File.Copy(targetDb, tempOldDb, True)
                End If

                File.Copy(backupFilePath, targetDb, True)

                If AuthService.CurrentUser IsNot Nothing Then
                    _auditRepo.Log(AuthService.CurrentUser.Id, AuthService.CurrentUser.Username, AuditAction.Restore, "Database", Nothing, $"Database restored from {backupFilePath}")
                End If

                Return (True, "Database successfully restored. Please restart the application if prompted.")
            Catch ex As Exception
                Return (False, $"Restore failed: {ex.Message}")
            End Try
        End Function
    End Class
End Namespace
