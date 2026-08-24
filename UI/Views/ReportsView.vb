Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Views
    Public Class ReportsView
        Inherits UserControl

        Private ReadOnly _reportService As New ReportService()
        Private ReadOnly _categoryService As New CategoryService()
        Private ReadOnly _dtpFrom As New DateTimePicker()
        Private ReadOnly _dtpTo As New DateTimePicker()
        Private ReadOnly _cboPeriod As New ModernComboBox()
        Private ReadOnly _cboCategory As New ModernComboBox()
        Private ReadOnly _btnGenerate As New CustomButton()
        Private ReadOnly _btnExportCsv As New CustomButton()
        Private ReadOnly _btnExportHtml As New CustomButton()
        Private ReadOnly _cardIncome As New CustomCard()
        Private ReadOnly _cardExpense As New CustomCard()
        Private ReadOnly _cardNet As New CustomCard()
        Private ReadOnly _gridDetails As New ModernDataGridView()
        Private ReadOnly _donutChart As New DonutChartControl()
        Private _currentReportData As ReportDataResult = Nothing

        Public Sub New()
            Dock = DockStyle.Fill
            BackColor = ThemeColors.AppBackground
            InitializeUI()
            LoadCategories()
        End Sub

        Private Sub InitializeUI()
            Dim pnlTop As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 115,
                .Padding = New Padding(25, 15, 25, 0)
            }

            Dim lblTitle As New Label() With {
                .Text = "Financial Reports & Statement Analysis",
                .Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(25, 12)
            }

            Dim pnlControls As New Panel() With {
                .Location = New Point(25, 55),
                .Size = New Size(940, 50),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            }

            _cboPeriod.Location = New Point(0, 5)
            _cboPeriod.Size = New Size(140, 34)
            _cboPeriod.Items.AddRange({"Today", "This Week", "This Month", "Last Month", "This Quarter", "This Year", "Custom Period"})
            _cboPeriod.SelectedIndex = 2
            AddHandler _cboPeriod.SelectedIndexChanged, AddressOf PeriodChanged

            _dtpFrom.Location = New Point(150, 5)
            _dtpFrom.Size = New Size(115, 34)
            _dtpFrom.Font = New Font("Segoe UI", 9.0F)
            _dtpFrom.Format = DateTimePickerFormat.Short

            _dtpTo.Location = New Point(275, 5)
            _dtpTo.Size = New Size(115, 34)
            _dtpTo.Font = New Font("Segoe UI", 9.0F)
            _dtpTo.Format = DateTimePickerFormat.Short

            _cboCategory.Location = New Point(400, 5)
            _cboCategory.Size = New Size(150, 34)

            _btnGenerate.Text = "Run Report"
            _btnGenerate.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnGenerate.Size = New Size(110, 34)
            _btnGenerate.Location = New Point(560, 5)
            AddHandler _btnGenerate.Click, Sub() RefreshData()

            _btnExportCsv.Text = "Export Excel/CSV"
            _btnExportCsv.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnExportCsv.Size = New Size(135, 34)
            _btnExportCsv.Location = New Point(680, 5)
            AddHandler _btnExportCsv.Click, AddressOf ExportCsv

            _btnExportHtml.Text = "Export Print/PDF"
            _btnExportHtml.ButtonStyle = CustomButton.ButtonStyleType.Success
            _btnExportHtml.Size = New Size(130, 34)
            _btnExportHtml.Location = New Point(825, 5)
            AddHandler _btnExportHtml.Click, AddressOf ExportHtml

            pnlControls.Controls.AddRange({_cboPeriod, _dtpFrom, _dtpTo, _cboCategory, _btnGenerate, _btnExportCsv, _btnExportHtml})
            pnlTop.Controls.AddRange({lblTitle, pnlControls})

            Dim pnlCards As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 110,
                .Padding = New Padding(25, 10, 25, 10)
            }

            _cardIncome.CardTitle = "Period Income"
            _cardIncome.AccentColor = ThemeColors.Success
            _cardIncome.Location = New Point(25, 5)
            _cardIncome.Size = New Size(290, 95)

            _cardExpense.CardTitle = "Period Expenses"
            _cardExpense.AccentColor = ThemeColors.Danger
            _cardExpense.Location = New Point(330, 5)
            _cardExpense.Size = New Size(290, 95)

            _cardNet.CardTitle = "Net Result"
            _cardNet.AccentColor = ThemeColors.Primary
            _cardNet.Location = New Point(635, 5)
            _cardNet.Size = New Size(290, 95)

            pnlCards.Controls.AddRange({_cardIncome, _cardExpense, _cardNet})

            Dim pnlContent As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 10, 25, 20)
            }

            _donutChart.Location = New Point(25, 5)
            _donutChart.Size = New Size(320, 320)
            _donutChart.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left

            _gridDetails.Location = New Point(360, 5)
            _gridDetails.Size = New Size(580, 320)
            _gridDetails.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

            SetupGridColumns()

            pnlContent.Controls.AddRange({_donutChart, _gridDetails})

            Controls.Add(pnlContent)
            Controls.Add(pnlCards)
            Controls.Add(pnlTop)

            SetDatesForPeriod(2)
        End Sub

        Private Sub SetupGridColumns()
            _gridDetails.Columns.Clear()

            _gridDetails.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColDate",
                .HeaderText = "Date",
                .Width = 100,
                .DataPropertyName = "ExpenseDate"
            })
            _gridDetails.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColTitle",
                .HeaderText = "Item Title",
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                .DataPropertyName = "Title"
            })
            _gridDetails.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColCat",
                .HeaderText = "Category",
                .Width = 140,
                .DataPropertyName = "CategoryName"
            })
            _gridDetails.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColAmount",
                .HeaderText = "Amount (EGP)",
                .Width = 130,
                .DataPropertyName = "Amount"
            })

            AddHandler _gridDetails.CellFormatting, Sub(s, e)
                                                       If e.RowIndex < 0 Then Return
                                                       If _gridDetails.Columns(e.ColumnIndex).Name = "ColDate" AndAlso TypeOf e.Value Is DateTime Then
                                                           e.Value = DirectCast(e.Value, DateTime).ToString("yyyy-MM-dd")
                                                           e.FormattingApplied = True
                                                       ElseIf _gridDetails.Columns(e.ColumnIndex).Name = "ColAmount" AndAlso TypeOf e.Value Is Decimal Then
                                                           e.Value = $"{CDec(e.Value):N2} EGP"
                                                           e.CellStyle.ForeColor = ThemeColors.Danger
                                                           e.CellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                                                           e.FormattingApplied = True
                                                       End If
                                                   End Sub
        End Sub

        Private Sub LoadCategories()
            Dim cats = _categoryService.GetAllCategories()
            Dim filterList As New List(Of Category)()
            filterList.Add(New Category() With {.Id = 0, .Name = "All Categories"})
            filterList.AddRange(cats)

            _cboCategory.DisplayMember = "Name"
            _cboCategory.ValueMember = "Id"
            _cboCategory.DataSource = filterList
        End Sub

        Private Sub PeriodChanged(sender As Object, e As EventArgs)
            SetDatesForPeriod(_cboPeriod.SelectedIndex)
            RefreshData()
        End Sub

        Private Sub SetDatesForPeriod(index As Integer)
            Dim today As DateTime = DateTime.Today
            Select Case index
                Case 0
                    _dtpFrom.Value = today
                    _dtpTo.Value = today
                Case 1
                    Dim startOfWeek As DateTime = today.AddDays(-CInt(today.DayOfWeek))
                    _dtpFrom.Value = startOfWeek
                    _dtpTo.Value = today
                Case 2
                    _dtpFrom.Value = New DateTime(today.Year, today.Month, 1)
                    _dtpTo.Value = _dtpFrom.Value.AddMonths(1).AddDays(-1)
                Case 3
                    Dim lastMonth As DateTime = today.AddMonths(-1)
                    _dtpFrom.Value = New DateTime(lastMonth.Year, lastMonth.Month, 1)
                    _dtpTo.Value = _dtpFrom.Value.AddMonths(1).AddDays(-1)
                Case 4
                    Dim qMonth As Integer = ((today.Month - 1) \ 3) * 3 + 1
                    _dtpFrom.Value = New DateTime(today.Year, qMonth, 1)
                    _dtpTo.Value = _dtpFrom.Value.AddMonths(3).AddDays(-1)
                Case 5
                    _dtpFrom.Value = New DateTime(today.Year, 1, 1)
                    _dtpTo.Value = New DateTime(today.Year, 12, 31)
            End Select
        End Sub

        Public Sub RefreshData()
            Dim catId As Nullable(Of Integer) = Nothing
            If _cboCategory.SelectedValue IsNot Nothing AndAlso Convert.ToInt32(_cboCategory.SelectedValue) > 0 Then
                catId = Convert.ToInt32(_cboCategory.SelectedValue)
            End If

            _currentReportData = _reportService.GetReportData(_dtpFrom.Value.Date, _dtpTo.Value.Date, catId)

            _cardIncome.CardValue = $"{_currentReportData.TotalIncome:N2} EGP"
            _cardIncome.Subtitle = $"{_currentReportData.Income.Count} income transactions"

            _cardExpense.CardValue = $"{_currentReportData.TotalExpenses:N2} EGP"
            _cardExpense.Subtitle = $"{_currentReportData.Expenses.Count} expense transactions"

            _cardNet.CardValue = $"{_currentReportData.NetBalance:N2} EGP"
            _cardNet.Subtitle = If(_currentReportData.NetBalance >= 0, "Net Surplus", "Net Deficit")
            _cardNet.AccentColor = If(_currentReportData.NetBalance >= 0, ThemeColors.Primary, ThemeColors.Danger)

            _cardIncome.Invalidate()
            _cardExpense.Invalidate()
            _cardNet.Invalidate()

            _donutChart.Items = _currentReportData.CategoryBreakdown
            _gridDetails.DataSource = Nothing
            _gridDetails.DataSource = _currentReportData.Expenses
        End Sub

        Private Sub ExportCsv(sender As Object, e As EventArgs)
            If _currentReportData Is Nothing Then RefreshData()

            Using sfd As New SaveFileDialog()
                sfd.Filter = "CSV Files (*.csv)|*.csv"
                sfd.FileName = $"Masa_Financial_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                If sfd.ShowDialog(FindForm()) = DialogResult.OK Then
                    If _reportService.ExportToCsv(_currentReportData, sfd.FileName) Then
                        CustomMessageBox.Show($"Report successfully exported to {Path.GetFileName(sfd.FileName)}", "Export Complete", CustomMessageBox.MessageType.Success, FindForm())
                    Else
                        CustomMessageBox.Show("Failed to export report.", "Export Error", CustomMessageBox.MessageType.Error, FindForm())
                    End If
                End If
            End Using
        End Sub

        Private Sub ExportHtml(sender As Object, e As EventArgs)
            If _currentReportData Is Nothing Then RefreshData()

            Using sfd As New SaveFileDialog()
                sfd.Filter = "HTML / Printable Report (*.html)|*.html"
                sfd.FileName = $"Masa_Financial_Statement_{DateTime.Now:yyyyMMdd_HHmm}.html"
                If sfd.ShowDialog(FindForm()) = DialogResult.OK Then
                    If _reportService.ExportToHtmlReport(_currentReportData, sfd.FileName) Then
                        If CustomMessageBox.Confirm("Report generated successfully. Would you like to open it in your browser for printing/saving as PDF?", "Open Report", FindForm()) Then
                            Process.Start(New ProcessStartInfo(sfd.FileName) With {.UseShellExecute = True})
                        End If
                    Else
                        CustomMessageBox.Show("Failed to export HTML report.", "Export Error", CustomMessageBox.MessageType.Error, FindForm())
                    End If
                End If
            End Using
        End Sub
    End Class
End Namespace
