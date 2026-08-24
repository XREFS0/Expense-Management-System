Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Forms
    Public Class LoginForm
        Inherits Form

        Private ReadOnly _authService As New AuthService()
        Private ReadOnly _txtUsername As New CustomTextBox()
        Private ReadOnly _txtPassword As New CustomTextBox()
        Private ReadOnly _btnLogin As New CustomButton()
        Private ReadOnly _lblError As New Label()

        Public Sub New()
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.CenterScreen
            BackColor = ThemeColors.SidebarBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(420, 520)

            InitializeUI()
        End Sub

        Private Sub InitializeUI()
            Dim pnlTitleBar As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 40,
                .BackColor = ThemeColors.HeaderBackground
            }

            Dim btnClose As New Label() With {
                .Text = "✕",
                .Font = New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextSecondary,
                .Size = New Size(40, 40),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Cursor = Cursors.Hand,
                .Location = New Point(Width - 40, 0),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            }
            AddHandler btnClose.Click, Sub() Application.Exit()
            AddHandler btnClose.MouseEnter, Sub()
                                               btnClose.ForeColor = ThemeColors.Danger
                                               btnClose.BackColor = Color.FromArgb(254, 226, 226)
                                           End Sub
            AddHandler btnClose.MouseLeave, Sub()
                                               btnClose.ForeColor = ThemeColors.TextSecondary
                                               btnClose.BackColor = Color.Transparent
                                           End Sub

            Dim btnMin As New Label() With {
                .Text = "—",
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = ThemeColors.TextSecondary,
                .Size = New Size(40, 40),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Cursor = Cursors.Hand,
                .Location = New Point(Width - 80, 0),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            }
            AddHandler btnMin.Click, Sub() WindowState = FormWindowState.Minimized
            AddHandler btnMin.MouseEnter, Sub()
                                             btnMin.ForeColor = ThemeColors.TextPrimary
                                             btnMin.BackColor = ThemeColors.SidebarHover
                                         End Sub
            AddHandler btnMin.MouseLeave, Sub()
                                             btnMin.ForeColor = ThemeColors.TextSecondary
                                             btnMin.BackColor = Color.Transparent
                                         End Sub

            pnlTitleBar.Controls.AddRange({btnClose, btnMin})

            Dim isDragging As Boolean = False
            Dim dragCursor As Point = Point.Empty
            Dim dragForm As Point = Point.Empty
            AddHandler pnlTitleBar.MouseDown, Sub(s, e)
                                                 If e.Button = MouseButtons.Left Then
                                                     isDragging = True
                                                     dragCursor = Cursor.Position
                                                     dragForm = Location
                                                 End If
                                             End Sub
            AddHandler pnlTitleBar.MouseMove, Sub(s, e)
                                                 If isDragging Then
                                                     Dim diff As Point = Point.Subtract(Cursor.Position, New Size(dragCursor))
                                                     Location = Point.Add(dragForm, New Size(diff))
                                                 End If
                                             End Sub
            AddHandler pnlTitleBar.MouseUp, Sub(s, e) isDragging = False

            Dim pnlBody As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(35, 20, 35, 20)
            }

            Dim lblBrand As New Label() With {
                .Text = "MASA",
                .Font = New Font("Segoe UI", 24.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.Primary,
                .AutoSize = True,
                .Location = New Point(35, 20)
            }

            Dim lblSubtitle As New Label() With {
                .Text = "Expense Management System",
                .Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(35, 68)
            }

            Dim lblInstruction As New Label() With {
                .Text = "Sign in with your enterprise credentials",
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = ThemeColors.TextSecondary,
                .AutoSize = True,
                .Location = New Point(35, 95)
            }

            Dim lblUser As New Label() With {
                .Text = "USERNAME",
                .Font = New Font("Segoe UI Semibold", 8.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextSecondary,
                .AutoSize = True,
                .Location = New Point(35, 145)
            }

            _txtUsername.Location = New Point(35, 168)
            _txtUsername.Size = New Size(350, 38)
            _txtUsername.PlaceholderText = "Username or email"
            _txtUsername.Text = "admin"

            Dim lblPass As New Label() With {
                .Text = "PASSWORD",
                .Font = New Font("Segoe UI Semibold", 8.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextSecondary,
                .AutoSize = True,
                .Location = New Point(35, 225)
            }

            _txtPassword.Location = New Point(35, 248)
            _txtPassword.Size = New Size(350, 38)
            _txtPassword.UseSystemPasswordChar = True
            _txtPassword.PlaceholderText = "Enter password"
            _txtPassword.Text = "admin123"

            _lblError.Text = ""
            _lblError.Font = New Font("Segoe UI", 8.5F)
            _lblError.ForeColor = ThemeColors.Danger
            _lblError.Location = New Point(35, 295)
            _lblError.Size = New Size(350, 25)

            _btnLogin.Text = "Sign In"
            _btnLogin.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnLogin.Size = New Size(350, 42)
            _btnLogin.Location = New Point(35, 330)
            AddHandler _btnLogin.Click, AddressOf PerformLogin

            Dim lblDefaultHint As New Label() With {
                .Text = "Default Admin: admin / admin123",
                .Font = New Font("Segoe UI", 8.0F),
                .ForeColor = ThemeColors.TextMuted,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Location = New Point(35, 390),
                .Size = New Size(350, 20)
            }

            pnlBody.Controls.AddRange({lblBrand, lblSubtitle, lblInstruction, lblUser, _txtUsername, lblPass, _txtPassword, _lblError, _btnLogin, lblDefaultHint})

            Controls.Add(pnlBody)
            Controls.Add(pnlTitleBar)

            AcceptButton = _btnLogin
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Using p As New Pen(ThemeColors.CardBorder, 1.5F)
                e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1)
            End Using
        End Sub

        Private Sub PerformLogin(sender As Object, e As EventArgs)
            _lblError.Text = ""
            Dim res = _authService.Login(_txtUsername.Text, _txtPassword.Text)
            If res.Success Then
                DialogResult = DialogResult.OK
                Close()
            Else
                _lblError.Text = res.Message
            End If
        End Sub
    End Class
End Namespace
