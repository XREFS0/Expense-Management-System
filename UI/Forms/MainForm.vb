Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme
Imports MasaExpenseManager.UI.Views

Namespace UI.Forms
    Public Class MainForm
        Inherits Form

        Private ReadOnly _authService As New AuthService()
        Private ReadOnly _pnlSidebar As New Panel()
        Private ReadOnly _pnlContent As New Panel()
        Private ReadOnly _pnlTopBar As New Panel()
        Private ReadOnly _lblCurrentUser As New Label()
        Private ReadOnly _lblModuleTitle As New Label()

        Private ReadOnly _btnNavDashboard As New Button()
        Private ReadOnly _btnNavExpenses As New Button()
        Private ReadOnly _btnNavIncome As New Button()
        Private ReadOnly _btnNavCategories As New Button()
        Private ReadOnly _btnNavReports As New Button()
        Private ReadOnly _btnNavUsers As New Button()
        Private ReadOnly _btnNavSettings As New Button()
        Private ReadOnly _btnNavLogout As New Button()

        Private _viewDashboard As DashboardView
        Private _viewExpenses As ExpensesView
        Private _viewIncome As IncomeView
        Private _viewCategories As CategoriesView
        Private _viewReports As ReportsView
        Private _viewUsers As UsersView
        Private _viewSettings As SettingsView

        Private _activeNavButton As Button = Nothing

        Public Sub New()
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.CenterScreen
            BackColor = ThemeColors.AppBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(1280, 800)
            MinimumSize = New Size(1024, 700)
            DoubleBuffered = True
            SetStyle(ControlStyles.ResizeRedraw, True)

            InitializeUI()
            InitializeViews()
            SwitchView(_btnNavDashboard, _viewDashboard)
        End Sub

        Private Sub InitializeUI()
            Dim pnlTitleBar As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 40,
                .BackColor = ThemeColors.HeaderBackground
            }

            Dim lblAppTitle As New Label() With {
                .Text = "MASA Expense Manager Enterprise v1.0",
                .Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .Location = New Point(15, 11),
                .AutoSize = True
            }
            pnlTitleBar.Controls.Add(lblAppTitle)

            Dim pnlWindowButtons As New Panel() With {
                .Dock = DockStyle.Right,
                .Width = 135,
                .BackColor = Color.Transparent
            }

            Dim btnMin As New Button() With {
                .Text = "—",
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = ThemeColors.TextSecondary,
                .BackColor = Color.Transparent,
                .FlatStyle = FlatStyle.Flat,
                .Size = New Size(45, 40),
                .Location = New Point(0, 0),
                .Cursor = Cursors.Hand
            }
            btnMin.FlatAppearance.BorderSize = 0
            AddHandler btnMin.Click, Sub() WindowState = FormWindowState.Minimized
            AddHandler btnMin.MouseEnter, Sub()
                                             btnMin.ForeColor = ThemeColors.TextPrimary
                                             btnMin.BackColor = ThemeColors.SidebarHover
                                         End Sub
            AddHandler btnMin.MouseLeave, Sub()
                                             btnMin.ForeColor = ThemeColors.TextSecondary
                                             btnMin.BackColor = Color.Transparent
                                         End Sub

            Dim btnMax As New Button() With {
                .Text = "◻",
                .Font = New Font("Segoe UI", 10.0F),
                .ForeColor = ThemeColors.TextSecondary,
                .BackColor = Color.Transparent,
                .FlatStyle = FlatStyle.Flat,
                .Size = New Size(45, 40),
                .Location = New Point(45, 0),
                .Cursor = Cursors.Hand
            }
            btnMax.FlatAppearance.BorderSize = 0
            AddHandler btnMax.Click, Sub()
                                         WindowState = If(WindowState = FormWindowState.Maximized, FormWindowState.Normal, FormWindowState.Maximized)
                                     End Sub
            AddHandler btnMax.MouseEnter, Sub()
                                             btnMax.ForeColor = ThemeColors.TextPrimary
                                             btnMax.BackColor = ThemeColors.SidebarHover
                                         End Sub
            AddHandler btnMax.MouseLeave, Sub()
                                             btnMax.ForeColor = ThemeColors.TextSecondary
                                             btnMax.BackColor = Color.Transparent
                                         End Sub

            Dim btnClose As New Button() With {
                .Text = "✕",
                .Font = New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextSecondary,
                .BackColor = Color.Transparent,
                .FlatStyle = FlatStyle.Flat,
                .Size = New Size(45, 40),
                .Location = New Point(90, 0),
                .Cursor = Cursors.Hand
            }
            btnClose.FlatAppearance.BorderSize = 0
            AddHandler btnClose.Click, Sub() Application.Exit()
            AddHandler btnClose.MouseEnter, Sub()
                                               btnClose.ForeColor = Color.White
                                               btnClose.BackColor = ThemeColors.Danger
                                           End Sub
            AddHandler btnClose.MouseLeave, Sub()
                                               btnClose.ForeColor = ThemeColors.TextSecondary
                                               btnClose.BackColor = Color.Transparent
                                           End Sub

            pnlWindowButtons.Controls.AddRange({btnMin, btnMax, btnClose})
            pnlTitleBar.Controls.Add(pnlWindowButtons)

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

            _pnlSidebar.Dock = DockStyle.Left
            _pnlSidebar.Width = 240
            _pnlSidebar.BackColor = ThemeColors.SidebarBackground

            Dim pnlLogo As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 70,
                .Padding = New Padding(20, 15, 20, 10)
            }
            Dim lblLogo As New Label() With {
                .Text = "MASA FINANCE",
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.Primary,
                .Location = New Point(20, 16),
                .AutoSize = True
            }
            Dim lblSubLogo As New Label() With {
                .Text = "Enterprise Expense Suite",
                .Font = New Font("Segoe UI", 8.0F),
                .ForeColor = ThemeColors.TextMuted,
                .Location = New Point(20, 38),
                .AutoSize = True
            }
            pnlLogo.Controls.AddRange({lblLogo, lblSubLogo})

            Dim pnlNavItems As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(10, 10, 10, 10),
                .AutoScroll = True
            }

            SetupNavButton(_btnNavDashboard, "Dashboard", 0)
            SetupNavButton(_btnNavExpenses, "Expenses", 48)
            SetupNavButton(_btnNavIncome, "Income", 96)
            SetupNavButton(_btnNavCategories, "Categories", 144)
            SetupNavButton(_btnNavReports, "Reports & Export", 192)
            SetupNavButton(_btnNavUsers, "User Accounts", 240)
            SetupNavButton(_btnNavSettings, "Settings & Backup", 288)

            pnlNavItems.Controls.AddRange({_btnNavDashboard, _btnNavExpenses, _btnNavIncome, _btnNavCategories, _btnNavReports, _btnNavUsers, _btnNavSettings})

            Dim pnlNavBottom As New Panel() With {
                .Dock = DockStyle.Bottom,
                .Height = 60,
                .Padding = New Padding(10)
            }
            SetupNavButton(_btnNavLogout, "Sign Out", 0)
            _btnNavLogout.Dock = DockStyle.Fill
            AddHandler _btnNavLogout.Click, AddressOf PerformLogout
            pnlNavBottom.Controls.Add(_btnNavLogout)

            _pnlSidebar.Controls.Add(pnlNavItems)
            _pnlSidebar.Controls.Add(pnlNavBottom)
            _pnlSidebar.Controls.Add(pnlLogo)

            _pnlTopBar.Dock = DockStyle.Top
            _pnlTopBar.Height = 55
            _pnlTopBar.BackColor = ThemeColors.HeaderBackground
            _pnlTopBar.Padding = New Padding(20, 0, 20, 0)

            _lblModuleTitle.Text = "Dashboard"
            _lblModuleTitle.Font = New Font("Segoe UI Semibold", 12.0F, FontStyle.Bold)
            _lblModuleTitle.ForeColor = ThemeColors.TextPrimary
            _lblModuleTitle.Location = New Point(20, 16)
            _lblModuleTitle.AutoSize = True

            _lblCurrentUser.Text = If(AuthService.CurrentUser IsNot Nothing, $"{AuthService.CurrentUser.FullName} ({AuthService.CurrentUser.Role})", "Administrator")
            _lblCurrentUser.Font = New Font("Segoe UI", 9.5F)
            _lblCurrentUser.ForeColor = ThemeColors.TextSecondary
            _lblCurrentUser.Location = New Point(Width - _pnlSidebar.Width - 280, 17)
            _lblCurrentUser.AutoSize = True
            _lblCurrentUser.Anchor = AnchorStyles.Top Or AnchorStyles.Right

            _pnlTopBar.Controls.AddRange({_lblModuleTitle, _lblCurrentUser})

            _pnlContent.Dock = DockStyle.Fill
            _pnlContent.BackColor = ThemeColors.AppBackground

            Dim pnlMainContainer As New Panel() With {.Dock = DockStyle.Fill}
            pnlMainContainer.Controls.Add(_pnlContent)
            pnlMainContainer.Controls.Add(_pnlTopBar)

            Dim pnlBodyWrapper As New Panel() With {.Dock = DockStyle.Fill}
            pnlBodyWrapper.Controls.Add(pnlMainContainer)
            pnlBodyWrapper.Controls.Add(_pnlSidebar)

            Controls.Add(pnlBodyWrapper)
            Controls.Add(pnlTitleBar)
            pnlTitleBar.BringToFront()
            _pnlTopBar.BringToFront()
            _pnlContent.BringToFront()
        End Sub

        Private Sub SetupNavButton(btn As Button, text As String, top As Integer)
            btn.Text = text
            btn.TextAlign = ContentAlignment.MiddleLeft
            btn.Padding = New Padding(15, 0, 10, 0)
            btn.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 0
            btn.ForeColor = ThemeColors.SidebarText
            btn.BackColor = ThemeColors.SidebarBackground
            btn.Cursor = Cursors.Hand
            btn.Size = New Size(220, 42)
            btn.Location = New Point(10, top)

            AddHandler btn.MouseEnter, Sub()
                                           If btn IsNot _activeNavButton Then
                                               btn.BackColor = ThemeColors.SidebarHover
                                               btn.ForeColor = ThemeColors.TextPrimary
                                           End If
                                       End Sub
            AddHandler btn.MouseLeave, Sub()
                                           If btn IsNot _activeNavButton Then
                                               btn.BackColor = ThemeColors.SidebarBackground
                                               btn.ForeColor = ThemeColors.SidebarText
                                           End If
                                       End Sub
        End Sub

        Private Sub InitializeViews()
            _viewDashboard = New DashboardView()
            _viewExpenses = New ExpensesView()
            _viewIncome = New IncomeView()
            _viewCategories = New CategoriesView()
            _viewReports = New ReportsView()
            _viewUsers = New UsersView()
            _viewSettings = New SettingsView()

            AddHandler _btnNavDashboard.Click, Sub() SwitchView(_btnNavDashboard, _viewDashboard)
            AddHandler _btnNavExpenses.Click, Sub() SwitchView(_btnNavExpenses, _viewExpenses)
            AddHandler _btnNavIncome.Click, Sub() SwitchView(_btnNavIncome, _viewIncome)
            AddHandler _btnNavCategories.Click, Sub() SwitchView(_btnNavCategories, _viewCategories)
            AddHandler _btnNavReports.Click, Sub() SwitchView(_btnNavReports, _viewReports)
            AddHandler _btnNavUsers.Click, Sub() SwitchView(_btnNavUsers, _viewUsers)
            AddHandler _btnNavSettings.Click, Sub() SwitchView(_btnNavSettings, _viewSettings)

            If AuthService.CurrentUser IsNot Nothing AndAlso AuthService.CurrentUser.Role = UserRole.User Then
                _btnNavUsers.Visible = False
            End If
        End Sub

        Private Sub SwitchView(navButton As Button, viewControl As Control)
            If _activeNavButton IsNot Nothing Then
                _activeNavButton.BackColor = ThemeColors.SidebarBackground
                _activeNavButton.ForeColor = ThemeColors.SidebarText
            End If

            _activeNavButton = navButton
            _activeNavButton.BackColor = ThemeColors.SidebarHover
            _activeNavButton.ForeColor = ThemeColors.SidebarActive

            _lblModuleTitle.Text = navButton.Text.Trim()

            _pnlContent.Controls.Clear()
            _pnlContent.Controls.Add(viewControl)

            If TypeOf viewControl Is DashboardView Then
                DirectCast(viewControl, DashboardView).RefreshData()
            ElseIf TypeOf viewControl Is ExpensesView Then
                DirectCast(viewControl, ExpensesView).RefreshData()
            ElseIf TypeOf viewControl Is IncomeView Then
                DirectCast(viewControl, IncomeView).RefreshData()
            ElseIf TypeOf viewControl Is CategoriesView Then
                DirectCast(viewControl, CategoriesView).RefreshData()
            ElseIf TypeOf viewControl Is ReportsView Then
                DirectCast(viewControl, ReportsView).RefreshData()
            ElseIf TypeOf viewControl Is UsersView Then
                DirectCast(viewControl, UsersView).RefreshData()
            ElseIf TypeOf viewControl Is SettingsView Then
                DirectCast(viewControl, SettingsView).LoadSettings()
            End If
        End Sub

        Private Sub PerformLogout(sender As Object, e As EventArgs)
            If CustomMessageBox.Confirm("Are you sure you want to sign out?", "Sign Out Confirmation", Me) Then
                _authService.Logout()
                Hide()
                Using login As New LoginForm()
                    If login.ShowDialog() = DialogResult.OK Then
                        _lblCurrentUser.Text = If(AuthService.CurrentUser IsNot Nothing, $"{AuthService.CurrentUser.FullName} ({AuthService.CurrentUser.Role})", "Administrator")
                        If AuthService.CurrentUser IsNot Nothing AndAlso AuthService.CurrentUser.Role = UserRole.User Then
                            _btnNavUsers.Visible = False
                        Else
                            _btnNavUsers.Visible = True
                        End If
                        SwitchView(_btnNavDashboard, _viewDashboard)
                        Show()
                    Else
                        Application.Exit()
                    End If
                End Using
            End If
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Using p As New Pen(ThemeColors.CardBorder, 1.0F)
                e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1)
            End Using
        End Sub
    End Class
End Namespace
