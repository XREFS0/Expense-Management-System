Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Views
    Public Class DashboardView
        Inherits UserControl

        Private ReadOnly _dashService As New DashboardService()
        Private ReadOnly _cardExpense As New CustomCard()
        Private ReadOnly _cardIncome As New CustomCard()
        Private ReadOnly _cardBalance As New CustomCard()
        Private ReadOnly _cardMonthExp As New CustomCard()
        Private ReadOnly _donutChart As New DonutChartControl()
        Private ReadOnly _barChart As New BarChartControl()
        Private ReadOnly _gridRecent As New ModernDataGridView()
        Private ReadOnly _lblRecentTitle As New Label()

        Public Sub New()
            Dock = DockStyle.Fill
            BackColor = ThemeColors.AppBackground
            AutoScroll = True
            InitializeUI()
        End Sub

        Private Sub InitializeUI()
            Dim pnlHeader As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 60,
                .Padding = New Padding(25, 15, 25, 0)
            }
            Dim lblTitle As New Label() With {
                .Text = "Dashboard Overview",
                .Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(25, 15)
            }
            Dim lblSubtitle As New Label() With {
                .Text = "Real-time enterprise financial analytics and summary",
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = ThemeColors.TextSecondary,
                .AutoSize = True,
                .Location = New Point(25, 38)
            }
            pnlHeader.Controls.Add(lblTitle)
            pnlHeader.Controls.Add(lblSubtitle)

            Dim pnlCards As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 120,
                .Padding = New Padding(25, 10, 25, 10)
            }

            _cardExpense.CardTitle = "Total Expenses"
            _cardExpense.AccentColor = ThemeColors.Danger
            _cardExpense.Location = New Point(25, 5)
            _cardExpense.Size = New Size(220, 100)

            _cardIncome.CardTitle = "Total Income"
            _cardIncome.AccentColor = ThemeColors.Success
            _cardIncome.Location = New Point(265, 5)
            _cardIncome.Size = New Size(220, 100)

            _cardBalance.CardTitle = "Net Balance"
            _cardBalance.AccentColor = ThemeColors.Primary
            _cardBalance.Location = New Point(505, 5)
            _cardBalance.Size = New Size(220, 100)

            _cardMonthExp.CardTitle = "This Month"
            _cardMonthExp.AccentColor = ThemeColors.Warning
            _cardMonthExp.Location = New Point(745, 5)
            _cardMonthExp.Size = New Size(220, 100)

            pnlCards.Controls.AddRange({_cardExpense, _cardIncome, _cardBalance, _cardMonthExp})

            Dim pnlCharts As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 280,
                .Padding = New Padding(25, 10, 25, 10)
            }
            _donutChart.Location = New Point(25, 5)
            _donutChart.Size = New Size(360, 260)

            _barChart.Location = New Point(405, 5)
            _barChart.Size = New Size(560, 260)
            _barChart.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

            pnlCharts.Controls.AddRange({_donutChart, _barChart})

            Dim pnlRecent As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 10, 25, 20)
            }

            _lblRecentTitle.Text = "Recent Transactions"
            _lblRecentTitle.Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold)
            _lblRecentTitle.ForeColor = ThemeColors.TextPrimary
            _lblRecentTitle.Location = New Point(25, 5)
            _lblRecentTitle.AutoSize = True

            _gridRecent.Location = New Point(25, 32)
            _gridRecent.Size = New Size(940, 220)
            _gridRecent.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

            SetupRecentGridColumns()

            pnlRecent.Controls.Add(_lblRecentTitle)
            pnlRecent.Controls.Add(_gridRecent)

            Controls.Add(pnlRecent)
            Controls.Add(pnlCharts)
            Controls.Add(pnlCards)
            Controls.Add(pnlHeader)
        End Sub

        Private Sub SetupRecentGridColumns()
            _gridRecent.Columns.Clear()

            _gridRecent.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColDate",
                .HeaderText = "Date",
                .Width = 110,
                .DataPropertyName = "TransactionDate"
            })
            _gridRecent.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColType",
                .HeaderText = "Type",
                .Width = 100,
                .DataPropertyName = "Type"
            })
            _gridRecent.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColTitle",
                .HeaderText = "Title / Source",
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                .DataPropertyName = "Title"
            })
            _gridRecent.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColCategory",
                .HeaderText = "Category",
                .Width = 150,
                .DataPropertyName = "CategoryName"
            })
            _gridRecent.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColAmount",
                .HeaderText = "Amount",
                .Width = 120,
                .DataPropertyName = "Amount"
            })
            _gridRecent.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColUser",
                .HeaderText = "User",
                .Width = 120,
                .DataPropertyName = "UserName"
            })

            AddHandler _gridRecent.CellFormatting, AddressOf GridCellFormatting
        End Sub

        Private Sub GridCellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
            If e.RowIndex < 0 Then Return

            If _gridRecent.Columns(e.ColumnIndex).Name = "ColDate" AndAlso TypeOf e.Value Is DateTime Then
                e.Value = DirectCast(e.Value, DateTime).ToString("yyyy-MM-dd")
                e.FormattingApplied = True
            ElseIf _gridRecent.Columns(e.ColumnIndex).Name = "ColAmount" AndAlso TypeOf e.Value Is Decimal Then
                Dim amt As Decimal = CDec(e.Value)
                Dim row = _gridRecent.Rows(e.RowIndex).DataBoundItem
                If row IsNot Nothing AndAlso TypeOf row Is Transaction Then
                    Dim tx As Transaction = DirectCast(row, Transaction)
                    If tx.Type = TransactionType.Expense Then
                        e.Value = $"-{amt:N2} EGP"
                        e.CellStyle.ForeColor = ThemeColors.Danger
                    Else
                        e.Value = $"+{amt:N2} EGP"
                        e.CellStyle.ForeColor = ThemeColors.Success
                    End If
                    e.CellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                    e.FormattingApplied = True
                End If
            ElseIf _gridRecent.Columns(e.ColumnIndex).Name = "ColType" AndAlso TypeOf e.Value Is TransactionType Then
                Dim t As TransactionType = DirectCast(e.Value, TransactionType)
                e.Value = t.ToString()
                e.FormattingApplied = True
            End If
        End Sub

        Public Sub RefreshData()
            Dim summary As DashboardSummary = _dashService.GetDashboardSummary()

            _cardExpense.CardValue = $"{summary.TotalExpenses:N2} EGP"
            _cardExpense.Subtitle = $"{summary.ExpenseCountThisMonth} expenses this month"

            _cardIncome.CardValue = $"{summary.TotalIncome:N2} EGP"
            _cardIncome.Subtitle = $"{summary.IncomeCountThisMonth} income streams this month"

            _cardBalance.CardValue = $"{summary.CurrentBalance:N2} EGP"
            _cardBalance.Subtitle = If(summary.CurrentBalance >= 0, "Healthy positive balance", "Deficit alert")
            _cardBalance.AccentColor = If(summary.CurrentBalance >= 0, ThemeColors.Primary, ThemeColors.Danger)

            _cardMonthExp.CardValue = $"{summary.MonthlyExpenseTotal:N2} EGP"
            _cardMonthExp.Subtitle = $"Top: {summary.TopExpenseCategory}"

            _cardExpense.Invalidate()
            _cardIncome.Invalidate()
            _cardBalance.Invalidate()
            _cardMonthExp.Invalidate()

            _donutChart.Items = summary.CategoryBreakdown
            _barChart.Trends = summary.MonthlyTrends

            _gridRecent.DataSource = Nothing
            _gridRecent.DataSource = summary.RecentTransactions
        End Sub
    End Class
End Namespace
