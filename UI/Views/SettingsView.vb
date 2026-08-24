Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Views
    Public Class SettingsView
        Inherits UserControl

        Private ReadOnly _settingsRepo As New SettingsRepository()
        Private ReadOnly _auditRepo As New AuditLogRepository()
        Private ReadOnly _backupService As New BackupRestoreService()
        Private ReadOnly _userService As New UserService()

        Private ReadOnly _txtCompanyName As New CustomTextBox()
        Private ReadOnly _txtCurrency As New CustomTextBox()
        Private ReadOnly _txtCurrencyCode As New CustomTextBox()
        Private ReadOnly _btnSaveSettings As New CustomButton()

        Private ReadOnly _txtCurrentPass As New CustomTextBox()
        Private ReadOnly _txtNewPass As New CustomTextBox()
        Private ReadOnly _txtConfirmPass As New CustomTextBox()
        Private ReadOnly _btnChangePass As New CustomButton()

        Private ReadOnly _btnBackupNow As New CustomButton()
        Private ReadOnly _btnRestoreDb As New CustomButton()
        Private ReadOnly _lblDbPath As New Label()

        Private ReadOnly _gridAudit As New ModernDataGridView()
        Private ReadOnly _btnRefreshAudit As New CustomButton()

        Public Sub New()
            Dock = DockStyle.Fill
            BackColor = ThemeColors.AppBackground
            AutoScroll = True
            InitializeUI()
            LoadSettings()
        End Sub

        Private Sub InitializeUI()
            Dim pnlTop As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 60,
                .Padding = New Padding(25, 15, 25, 0)
            }

            Dim lblTitle As New Label() With {
                .Text = "System Configuration & Security",
                .Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(25, 15)
            }
            pnlTop.Controls.Add(lblTitle)

            Dim pnlGeneral As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 175,
                .Padding = New Padding(25, 10, 25, 10)
            }

            Dim lblGenSec As New Label() With {.Text = "General Application Settings", .Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold), .ForeColor = ThemeColors.TextPrimary, .Location = New Point(25, 5), .AutoSize = True}

            Dim lblComp As New Label() With {.Text = "Company / Org Name", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, 35), .AutoSize = True}
            _txtCompanyName.Location = New Point(25, 55)
            _txtCompanyName.Size = New Size(280, 36)

            Dim lblSym As New Label() With {.Text = "Currency Symbol", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(320, 35), .AutoSize = True}
            _txtCurrency.Location = New Point(320, 55)
            _txtCurrency.Size = New Size(130, 36)

            Dim lblCode As New Label() With {.Text = "Currency Code", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(465, 35), .AutoSize = True}
            _txtCurrencyCode.Location = New Point(465, 55)
            _txtCurrencyCode.Size = New Size(130, 36)

            _btnSaveSettings.Text = "Save General Settings"
            _btnSaveSettings.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnSaveSettings.Size = New Size(170, 36)
            _btnSaveSettings.Location = New Point(25, 105)
            AddHandler _btnSaveSettings.Click, AddressOf SaveGeneralSettings

            pnlGeneral.Controls.AddRange({lblGenSec, lblComp, _txtCompanyName, lblSym, _txtCurrency, lblCode, _txtCurrencyCode, _btnSaveSettings})

            Dim pnlSecurityAndBackup As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 190,
                .Padding = New Padding(25, 10, 25, 10)
            }

            Dim lblSecHeader As New Label() With {.Text = "Change Password", .Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold), .ForeColor = ThemeColors.TextPrimary, .Location = New Point(25, 5), .AutoSize = True}

            _txtCurrentPass.Location = New Point(25, 35)
            _txtCurrentPass.Size = New Size(180, 36)
            _txtCurrentPass.UseSystemPasswordChar = True
            _txtCurrentPass.PlaceholderText = "Current Password"

            _txtNewPass.Location = New Point(215, 35)
            _txtNewPass.Size = New Size(180, 36)
            _txtNewPass.UseSystemPasswordChar = True
            _txtNewPass.PlaceholderText = "New Password"

            _txtConfirmPass.Location = New Point(405, 35)
            _txtConfirmPass.Size = New Size(180, 36)
            _txtConfirmPass.UseSystemPasswordChar = True
            _txtConfirmPass.PlaceholderText = "Confirm New"

            _btnChangePass.Text = "Update Password"
            _btnChangePass.ButtonStyle = CustomButton.ButtonStyleType.Warning
            _btnChangePass.Size = New Size(140, 36)
            _btnChangePass.Location = New Point(595, 35)
            AddHandler _btnChangePass.Click, AddressOf ChangePasswordClicked

            Dim lblDbHeader As New Label() With {.Text = "Database Maintenance & Backup", .Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold), .ForeColor = ThemeColors.TextPrimary, .Location = New Point(25, 90), .AutoSize = True}

            _lblDbPath.Text = $"Database: {DatabaseContext.DatabaseFilePath}"
            _lblDbPath.Font = New Font("Segoe UI", 8.5F)
            _lblDbPath.ForeColor = ThemeColors.TextMuted
            _lblDbPath.Location = New Point(25, 115)
            _lblDbPath.AutoSize = True

            _btnBackupNow.Text = "Backup Database Now"
            _btnBackupNow.ButtonStyle = CustomButton.ButtonStyleType.Success
            _btnBackupNow.Size = New Size(170, 36)
            _btnBackupNow.Location = New Point(25, 140)
            AddHandler _btnBackupNow.Click, AddressOf BackupDatabaseClicked

            _btnRestoreDb.Text = "Restore Database..."
            _btnRestoreDb.ButtonStyle = CustomButton.ButtonStyleType.Danger
            _btnRestoreDb.Size = New Size(150, 36)
            _btnRestoreDb.Location = New Point(205, 140)
            AddHandler _btnRestoreDb.Click, AddressOf RestoreDatabaseClicked

            pnlSecurityAndBackup.Controls.AddRange({lblSecHeader, _txtCurrentPass, _txtNewPass, _txtConfirmPass, _btnChangePass, lblDbHeader, _lblDbPath, _btnBackupNow, _btnRestoreDb})

            Dim pnlAudit As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 10, 25, 20)
            }

            Dim lblAuditHeader As New Label() With {.Text = "System Activity & Security Audit Logs", .Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold), .ForeColor = ThemeColors.TextPrimary, .Location = New Point(25, 5), .AutoSize = True}

            _btnRefreshAudit.Text = "Refresh Logs"
            _btnRefreshAudit.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnRefreshAudit.Size = New Size(110, 30)
            _btnRefreshAudit.Location = New Point(Width - 160, 5)
            _btnRefreshAudit.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnRefreshAudit.Click, Sub() LoadAuditLogs()

            _gridAudit.Location = New Point(25, 40)
            _gridAudit.Size = New Size(940, 200)
            _gridAudit.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

            SetupAuditGridColumns()

            pnlAudit.Controls.AddRange({lblAuditHeader, _btnRefreshAudit, _gridAudit})

            Controls.Add(pnlAudit)
            Controls.Add(pnlSecurityAndBackup)
            Controls.Add(pnlGeneral)
            Controls.Add(pnlTop)
        End Sub

        Private Sub SetupAuditGridColumns()
            _gridAudit.Columns.Clear()

            _gridAudit.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColTime",
                .HeaderText = "Timestamp",
                .Width = 140,
                .DataPropertyName = "Timestamp"
            })
            _gridAudit.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColUser",
                .HeaderText = "User",
                .Width = 120,
                .DataPropertyName = "Username"
            })
            _gridAudit.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColAction",
                .HeaderText = "Action",
                .Width = 100,
                .DataPropertyName = "Action"
            })
            _gridAudit.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColEntity",
                .HeaderText = "Entity",
                .Width = 120,
                .DataPropertyName = "EntityName"
            })
            _gridAudit.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColDetails",
                .HeaderText = "Details",
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                .DataPropertyName = "Details"
            })

            AddHandler _gridAudit.CellFormatting, Sub(s, e)
                                                     If e.RowIndex < 0 Then Return
                                                     If _gridAudit.Columns(e.ColumnIndex).Name = "ColTime" AndAlso TypeOf e.Value Is DateTime Then
                                                         e.Value = DirectCast(e.Value, DateTime).ToString("yyyy-MM-dd HH:mm:ss")
                                                         e.FormattingApplied = True
                                                     End If
                                                 End Sub
        End Sub

        Public Sub LoadSettings()
            _txtCompanyName.Text = _settingsRepo.GetValue("CompanyName", "MASA Solutions Egypt")
            _txtCurrency.Text = _settingsRepo.GetValue("CurrencySymbol", "EGP")
            _txtCurrencyCode.Text = _settingsRepo.GetValue("CurrencyCode", "EGP")
            _lblDbPath.Text = $"Database Path: {DatabaseContext.DatabaseFilePath}"
            LoadAuditLogs()
        End Sub

        Private Sub LoadAuditLogs()
            Dim logs = _auditRepo.GetAll(50)
            _gridAudit.DataSource = Nothing
            _gridAudit.DataSource = logs
        End Sub

        Private Sub SaveGeneralSettings(sender As Object, e As EventArgs)
            _settingsRepo.SetValue("CompanyName", _txtCompanyName.Text.Trim(), "Company or organization name")
            _settingsRepo.SetValue("CurrencySymbol", _txtCurrency.Text.Trim(), "Active currency symbol")
            _settingsRepo.SetValue("CurrencyCode", _txtCurrencyCode.Text.Trim(), "Active currency code")

            CustomMessageBox.Show("Settings updated successfully.", "Settings Saved", CustomMessageBox.MessageType.Success, FindForm())
        End Sub

        Private Sub ChangePasswordClicked(sender As Object, e As EventArgs)
            If AuthService.CurrentUser Is Nothing Then Return

            Dim cur = _txtCurrentPass.Text
            Dim newP = _txtNewPass.Text
            Dim conf = _txtConfirmPass.Text

            If String.IsNullOrWhiteSpace(cur) OrElse String.IsNullOrWhiteSpace(newP) Then
                CustomMessageBox.Show("Please enter current and new passwords.", "Validation Error", CustomMessageBox.MessageType.Warning, FindForm())
                Return
            End If

            If newP <> conf Then
                CustomMessageBox.Show("New password and confirmation do not match.", "Validation Error", CustomMessageBox.MessageType.Warning, FindForm())
                Return
            End If

            Dim res = _userService.ChangePassword(AuthService.CurrentUser.Id, cur, newP)
            If res.Success Then
                CustomMessageBox.Show("Password changed successfully.", "Security", CustomMessageBox.MessageType.Success, FindForm())
                _txtCurrentPass.Text = ""
                _txtNewPass.Text = ""
                _txtConfirmPass.Text = ""
            Else
                CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, FindForm())
            End If
        End Sub

        Private Sub BackupDatabaseClicked(sender As Object, e As EventArgs)
            Using sfd As New SaveFileDialog()
                sfd.Filter = "SQLite Database Backup (*.db)|*.db"
                sfd.FileName = $"Masa_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                If sfd.ShowDialog(FindForm()) = DialogResult.OK Then
                    Dim res = _backupService.CreateBackup(sfd.FileName)
                    If res.Success Then
                        CustomMessageBox.Show(res.Message, "Backup Success", CustomMessageBox.MessageType.Success, FindForm())
                        LoadAuditLogs()
                    Else
                        CustomMessageBox.Show(res.Message, "Backup Error", CustomMessageBox.MessageType.Error, FindForm())
                    End If
                End If
            End Using
        End Sub

        Private Sub RestoreDatabaseClicked(sender As Object, e As EventArgs)
            Using ofd As New OpenFileDialog()
                ofd.Filter = "SQLite Database Backup (*.db)|*.db"
                If ofd.ShowDialog(FindForm()) = DialogResult.OK Then
                    If CustomMessageBox.Confirm("Are you sure you want to restore the database from this backup? Current unsaved data will be replaced.", "Confirm Restore", FindForm()) Then
                        Dim res = _backupService.RestoreBackup(ofd.FileName)
                        If res.Success Then
                            CustomMessageBox.Show(res.Message, "Restore Success", CustomMessageBox.MessageType.Success, FindForm())
                            LoadAuditLogs()
                        Else
                            CustomMessageBox.Show(res.Message, "Restore Error", CustomMessageBox.MessageType.Error, FindForm())
                        End If
                    End If
                End If
            End Using
        End Sub
    End Class
End Namespace
