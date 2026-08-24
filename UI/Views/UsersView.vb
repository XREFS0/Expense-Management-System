Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Views
    Public Class UserDialog
        Inherits Form

        Private ReadOnly _userService As New UserService()
        Private ReadOnly _txtUsername As New CustomTextBox()
        Private ReadOnly _txtPassword As New CustomTextBox()
        Private ReadOnly _txtFullName As New CustomTextBox()
        Private ReadOnly _txtEmail As New CustomTextBox()
        Private ReadOnly _cboRole As New ModernComboBox()
        Private ReadOnly _chkActive As New CheckBox()
        Private ReadOnly _btnSave As New CustomButton()
        Private ReadOnly _btnCancel As New CustomButton()
        Private _editingId As Nullable(Of Integer) = Nothing

        Public Sub New(Optional userId As Nullable(Of Integer) = Nothing)
            _editingId = userId
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.CenterParent
            BackColor = ThemeColors.CardBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(440, 460)
            ShowInTaskbar = False

            InitializeUI()
            LoadRoles()

            If _editingId.HasValue Then
                LoadUserData(_editingId.Value)
            End If
        End Sub

        Private Sub InitializeUI()
            Dim pnlHeader As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 50,
                .BackColor = ThemeColors.HeaderBackground,
                .Padding = New Padding(20, 15, 20, 0)
            }
            Dim lblTitle As New Label() With {
                .Text = If(_editingId.HasValue, "Edit User Account", "Create New User Account"),
                .Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(20, 15)
            }
            pnlHeader.Controls.Add(lblTitle)

            Dim pnlBody As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 20, 25, 20)
            }

            Dim y As Integer = 15

            Dim lblU As New Label() With {.Text = "Username *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtUsername.Location = New Point(25, y)
            _txtUsername.Size = New Size(390, 36)
            _txtUsername.PlaceholderText = "e.g. john.doe"
            y += 48

            Dim lblP As New Label() With {.Text = If(_editingId.HasValue, "Password (Leave blank to keep current)", "Password *"), .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtPassword.Location = New Point(25, y)
            _txtPassword.Size = New Size(390, 36)
            _txtPassword.UseSystemPasswordChar = True
            _txtPassword.PlaceholderText = "Minimum 6 characters"
            y += 48

            Dim lblF As New Label() With {.Text = "Full Name *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtFullName.Location = New Point(25, y)
            _txtFullName.Size = New Size(390, 36)
            _txtFullName.PlaceholderText = "e.g. John Doe"
            y += 48

            Dim lblE As New Label() With {.Text = "Email Address", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtEmail.Location = New Point(25, y)
            _txtEmail.Size = New Size(390, 36)
            _txtEmail.PlaceholderText = "e.g. john@masa.com"
            y += 48

            Dim lblR As New Label() With {.Text = "System Role *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _cboRole.Location = New Point(25, y)
            _cboRole.Size = New Size(240, 36)

            _chkActive.Text = "Account Active"
            _chkActive.Font = New Font("Segoe UI", 9.5F)
            _chkActive.ForeColor = ThemeColors.TextPrimary
            _chkActive.Checked = True
            _chkActive.Location = New Point(280, y + 6)
            _chkActive.AutoSize = True

            pnlBody.Controls.AddRange({lblU, _txtUsername, lblP, _txtPassword, lblF, _txtFullName, lblE, _txtEmail, lblR, _cboRole, _chkActive})

            Dim pnlFooter As New Panel() With {
                .Dock = DockStyle.Bottom,
                .Height = 60,
                .BackColor = ThemeColors.HeaderBackground
            }

            _btnCancel.Text = "Cancel"
            _btnCancel.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnCancel.Size = New Size(100, 36)
            _btnCancel.Location = New Point(205, 12)
            AddHandler _btnCancel.Click, Sub()
                                             DialogResult = DialogResult.Cancel
                                             Close()
                                         End Sub

            _btnSave.Text = "Save User"
            _btnSave.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnSave.Size = New Size(110, 36)
            _btnSave.Location = New Point(315, 12)
            AddHandler _btnSave.Click, AddressOf SaveUser

            pnlFooter.Controls.Add(_btnCancel)
            pnlFooter.Controls.Add(_btnSave)

            Controls.Add(pnlBody)
            Controls.Add(pnlFooter)
            Controls.Add(pnlHeader)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Using p As New Pen(ThemeColors.CardBorder, 1.5F)
                e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1)
            End Using
        End Sub

        Private Sub LoadRoles()
            _cboRole.Items.Clear()
            _cboRole.Items.Add(UserRole.Admin)
            _cboRole.Items.Add(UserRole.Manager)
            _cboRole.Items.Add(UserRole.User)
            _cboRole.SelectedIndex = 2
        End Sub

        Private Sub LoadUserData(id As Integer)
            Dim user As User = _userService.GetUserById(id)
            If user IsNot Nothing Then
                _txtUsername.Text = user.Username
                _txtFullName.Text = user.FullName
                _txtEmail.Text = user.Email
                _cboRole.SelectedItem = user.Role
                _chkActive.Checked = user.IsActive
            End If
        End Sub

        Private Sub SaveUser(sender As Object, e As EventArgs)
            Dim username As String = _txtUsername.Text.Trim()
            Dim fullName As String = _txtFullName.Text.Trim()
            Dim email As String = _txtEmail.Text.Trim()
            Dim role As UserRole = If(_cboRole.SelectedItem IsNot Nothing, CType(_cboRole.SelectedItem, UserRole), UserRole.User)
            Dim isActive As Boolean = _chkActive.Checked

            If String.IsNullOrWhiteSpace(username) Then
                CustomMessageBox.Show("Please enter a username.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            If String.IsNullOrWhiteSpace(fullName) Then
                CustomMessageBox.Show("Please enter the user's full name.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            If _editingId.HasValue Then
                Dim res = _userService.UpdateUser(_editingId.Value, username, fullName, email, role, isActive)
                If res.Success Then
                    If Not String.IsNullOrWhiteSpace(_txtPassword.Text) Then
                        _userService.ResetUserPassword(_editingId.Value, _txtPassword.Text)
                    End If
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            Else
                Dim password As String = _txtPassword.Text
                If String.IsNullOrWhiteSpace(password) Then
                    CustomMessageBox.Show("Please specify an initial password.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                    Return
                End If

                Dim res = _userService.CreateUser(username, password, fullName, email, role, isActive)
                If res.Success Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            End If
        End Sub
    End Class

    Public Class UsersView
        Inherits UserControl

        Private ReadOnly _userService As New UserService()
        Private ReadOnly _gridUsers As New ModernDataGridView()
        Private ReadOnly _btnAdd As New CustomButton()
        Private ReadOnly _btnEdit As New CustomButton()
        Private ReadOnly _btnDelete As New CustomButton()

        Public Sub New()
            Dock = DockStyle.Fill
            BackColor = ThemeColors.AppBackground
            InitializeUI()
        End Sub

        Private Sub InitializeUI()
            Dim pnlTop As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 70,
                .Padding = New Padding(25, 15, 25, 0)
            }

            Dim lblTitle As New Label() With {
                .Text = "User Management & Role Permissions",
                .Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(25, 15)
            }

            _btnAdd.Text = "Add User"
            _btnAdd.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnAdd.Size = New Size(120, 36)
            _btnAdd.Location = New Point(Width - 345, 15)
            _btnAdd.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnAdd.Click, AddressOf OpenAddDialog

            _btnEdit.Text = "Edit"
            _btnEdit.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnEdit.Size = New Size(80, 36)
            _btnEdit.Location = New Point(Width - 215, 15)
            _btnEdit.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnEdit.Click, AddressOf OpenEditDialog

            _btnDelete.Text = "Delete"
            _btnDelete.ButtonStyle = CustomButton.ButtonStyleType.Danger
            _btnDelete.Size = New Size(80, 36)
            _btnDelete.Location = New Point(Width - 125, 15)
            _btnDelete.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnDelete.Click, AddressOf DeleteSelected

            pnlTop.Controls.AddRange({lblTitle, _btnAdd, _btnEdit, _btnDelete})

            Dim pnlGrid As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 10, 25, 20)
            }

            _gridUsers.Dock = DockStyle.Fill
            SetupGridColumns()
            pnlGrid.Controls.Add(_gridUsers)

            Controls.Add(pnlGrid)
            Controls.Add(pnlTop)
        End Sub

        Private Sub SetupGridColumns()
            _gridUsers.Columns.Clear()

            _gridUsers.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColId",
                .HeaderText = "ID",
                .Width = 60,
                .DataPropertyName = "Id"
            })
            _gridUsers.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColUsername",
                .HeaderText = "Username",
                .Width = 140,
                .DataPropertyName = "Username"
            })
            _gridUsers.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColFullName",
                .HeaderText = "Full Name",
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                .DataPropertyName = "FullName"
            })
            _gridUsers.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColEmail",
                .HeaderText = "Email",
                .Width = 200,
                .DataPropertyName = "Email"
            })
            _gridUsers.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColRole",
                .HeaderText = "Role",
                .Width = 120,
                .DataPropertyName = "Role"
            })
            _gridUsers.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColStatus",
                .HeaderText = "Status",
                .Width = 110,
                .DataPropertyName = "IsActive"
            })
            _gridUsers.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColLastLogin",
                .HeaderText = "Last Login",
                .Width = 150,
                .DataPropertyName = "LastLogin"
            })

            AddHandler _gridUsers.CellFormatting, AddressOf GridCellFormatting
            AddHandler _gridUsers.CellDoubleClick, Sub() OpenEditDialog(Nothing, EventArgs.Empty)
        End Sub

        Private Sub GridCellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
            If e.RowIndex < 0 Then Return

            If _gridUsers.Columns(e.ColumnIndex).Name = "ColStatus" AndAlso TypeOf e.Value Is Boolean Then
                Dim active As Boolean = CBool(e.Value)
                e.Value = If(active, "Active", "Inactive")
                e.CellStyle.ForeColor = If(active, ThemeColors.Success, ThemeColors.Danger)
                e.CellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                e.FormattingApplied = True
            ElseIf _gridUsers.Columns(e.ColumnIndex).Name = "ColLastLogin" Then
                If e.Value Is Nothing OrElse Convert.IsDBNull(e.Value) Then
                    e.Value = "Never"
                    e.CellStyle.ForeColor = ThemeColors.TextMuted
                    e.FormattingApplied = True
                ElseIf TypeOf e.Value Is DateTime Then
                    e.Value = DirectCast(e.Value, DateTime).ToString("yyyy-MM-dd HH:mm")
                    e.FormattingApplied = True
                End If
            End If
        End Sub

        Public Sub RefreshData()
            Dim list = _userService.GetAllUsers()
            _gridUsers.DataSource = Nothing
            _gridUsers.DataSource = list
        End Sub

        Private Sub OpenAddDialog(sender As Object, e As EventArgs)
            Using dlg As New UserDialog()
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub OpenEditDialog(sender As Object, e As EventArgs)
            If _gridUsers.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select a user to edit.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim user = DirectCast(_gridUsers.SelectedRows(0).DataBoundItem, User)
            Using dlg As New UserDialog(user.Id)
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub DeleteSelected(sender As Object, e As EventArgs)
            If _gridUsers.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select a user to delete.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim user = DirectCast(_gridUsers.SelectedRows(0).DataBoundItem, User)
            If CustomMessageBox.Confirm($"Are you sure you want to delete user account '{user.Username}'?", "Confirm Delete", FindForm()) Then
                Dim res = _userService.DeleteUser(user.Id)
                If res.Success Then
                    RefreshData()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, FindForm())
                End If
            End If
        End Sub
    End Class
End Namespace
