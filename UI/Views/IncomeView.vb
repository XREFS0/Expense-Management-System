Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Views
    Public Class IncomeDialog
        Inherits Form

        Private ReadOnly _categoryService As New CategoryService()
        Private ReadOnly _incomeService As New IncomeService()
        Private ReadOnly _txtSource As New CustomTextBox()
        Private ReadOnly _cboCategory As New ModernComboBox()
        Private ReadOnly _txtAmount As New CustomTextBox()
        Private ReadOnly _dtpDate As New DateTimePicker()
        Private ReadOnly _txtNotes As New CustomTextBox()
        Private ReadOnly _btnSave As New CustomButton()
        Private ReadOnly _btnCancel As New CustomButton()
        Private _editingId As Nullable(Of Integer) = Nothing

        Public Sub New(Optional incomeId As Nullable(Of Integer) = Nothing)
            _editingId = incomeId
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.CenterParent
            BackColor = ThemeColors.CardBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(480, 460)
            ShowInTaskbar = False

            InitializeUI()
            LoadCategories()

            If _editingId.HasValue Then
                LoadIncomeData(_editingId.Value)
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
                .Text = If(_editingId.HasValue, "Edit Income Record", "Record New Income"),
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

            Dim lblS As New Label() With {.Text = "Income Source *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtSource.Location = New Point(25, y)
            _txtSource.Size = New Size(430, 36)
            _txtSource.PlaceholderText = "e.g. Monthly Salary, Freelance project"
            y += 48

            Dim lblCat As New Label() With {.Text = "Category *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            Dim lblAmt As New Label() With {.Text = "Amount (EGP) *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(245, y), .AutoSize = True}
            y += 22

            _cboCategory.Location = New Point(25, y)
            _cboCategory.Size = New Size(205, 36)

            _txtAmount.Location = New Point(245, y)
            _txtAmount.Size = New Size(210, 36)
            _txtAmount.PlaceholderText = "0.00"
            y += 48

            Dim lblDate As New Label() With {.Text = "Income Date", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _dtpDate.Location = New Point(25, y)
            _dtpDate.Size = New Size(430, 36)
            _dtpDate.Font = New Font("Segoe UI", 9.5F)
            _dtpDate.Format = DateTimePickerFormat.Short
            y += 48

            Dim lblNotes As New Label() With {.Text = "Notes / Description", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtNotes.Location = New Point(25, y)
            _txtNotes.Size = New Size(430, 70)
            _txtNotes.Multiline = True
            _txtNotes.PlaceholderText = "Client details, payment invoice number..."

            pnlBody.Controls.AddRange({lblS, _txtSource, lblCat, lblAmt, _cboCategory, _txtAmount, lblDate, _dtpDate, lblNotes, _txtNotes})

            Dim pnlFooter As New Panel() With {
                .Dock = DockStyle.Bottom,
                .Height = 60,
                .BackColor = ThemeColors.HeaderBackground
            }

            _btnCancel.Text = "Cancel"
            _btnCancel.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnCancel.Size = New Size(100, 36)
            _btnCancel.Location = New Point(245, 12)
            AddHandler _btnCancel.Click, Sub()
                                             DialogResult = DialogResult.Cancel
                                             Close()
                                         End Sub

            _btnSave.Text = "Save Income"
            _btnSave.ButtonStyle = CustomButton.ButtonStyleType.Success
            _btnSave.Size = New Size(120, 36)
            _btnSave.Location = New Point(355, 12)
            AddHandler _btnSave.Click, AddressOf SaveIncome

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

        Private Sub LoadCategories()
            Dim cats = _categoryService.GetAllCategories(CategoryType.Income)
            _cboCategory.DisplayMember = "Name"
            _cboCategory.ValueMember = "Id"
            _cboCategory.DataSource = cats
        End Sub

        Private Sub LoadIncomeData(id As Integer)
            Dim inc As Income = _incomeService.GetIncomeById(id)
            If inc IsNot Nothing Then
                _txtSource.Text = inc.Source
                _txtAmount.Text = inc.Amount.ToString("0.00")
                _dtpDate.Value = inc.IncomeDate
                _txtNotes.Text = inc.Notes
                _cboCategory.SelectedValue = inc.CategoryId
            End If
        End Sub

        Private Sub SaveIncome(sender As Object, e As EventArgs)
            Dim source As String = _txtSource.Text.Trim()
            If String.IsNullOrWhiteSpace(source) Then
                CustomMessageBox.Show("Please enter an income source.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            If _cboCategory.SelectedValue Is Nothing Then
                CustomMessageBox.Show("Please select a category.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            Dim catId As Integer = Convert.ToInt32(_cboCategory.SelectedValue)
            Dim amount As Decimal = 0
            If Not Decimal.TryParse(_txtAmount.Text.Trim(), amount) OrElse amount <= 0 Then
                CustomMessageBox.Show("Please enter a valid positive income amount.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            Dim incDate As DateTime = _dtpDate.Value.Date
            Dim notes As String = _txtNotes.Text.Trim()

            If _editingId.HasValue Then
                Dim res = _incomeService.UpdateIncome(_editingId.Value, source, catId, amount, incDate, notes)
                If res.Success Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            Else
                Dim userId As Integer = If(AuthService.CurrentUser IsNot Nothing, AuthService.CurrentUser.Id, 1)
                Dim res = _incomeService.AddIncome(source, catId, amount, incDate, notes, userId)
                If res.Success Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            End If
        End Sub
    End Class

    Public Class IncomeView
        Inherits UserControl

        Private ReadOnly _incomeService As New IncomeService()
        Private ReadOnly _categoryService As New CategoryService()
        Private ReadOnly _gridIncome As New ModernDataGridView()
        Private ReadOnly _txtSearch As New CustomTextBox()
        Private ReadOnly _cboFilterCategory As New ModernComboBox()
        Private ReadOnly _dtpFrom As New DateTimePicker()
        Private ReadOnly _dtpTo As New DateTimePicker()
        Private ReadOnly _btnAdd As New CustomButton()
        Private ReadOnly _btnEdit As New CustomButton()
        Private ReadOnly _btnDelete As New CustomButton()
        Private ReadOnly _btnFilter As New CustomButton()
        Private ReadOnly _btnReset As New CustomButton()
        Private ReadOnly _lblTotalIncome As New Label()

        Public Sub New()
            Dock = DockStyle.Fill
            BackColor = ThemeColors.AppBackground
            InitializeUI()
            LoadCategoryFilter()
        End Sub

        Private Sub InitializeUI()
            Dim pnlTop As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 115,
                .Padding = New Padding(25, 15, 25, 0)
            }

            Dim lblTitle As New Label() With {
                .Text = "Income Management",
                .Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(25, 12)
            }

            _btnAdd.Text = "Add Income"
            _btnAdd.ButtonStyle = CustomButton.ButtonStyleType.Success
            _btnAdd.Size = New Size(130, 36)
            _btnAdd.Location = New Point(Width - 365, 10)
            _btnAdd.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnAdd.Click, AddressOf OpenAddDialog

            _btnEdit.Text = "Edit"
            _btnEdit.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnEdit.Size = New Size(80, 36)
            _btnEdit.Location = New Point(Width - 225, 10)
            _btnEdit.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnEdit.Click, AddressOf OpenEditDialog

            _btnDelete.Text = "Delete"
            _btnDelete.ButtonStyle = CustomButton.ButtonStyleType.Danger
            _btnDelete.Size = New Size(80, 36)
            _btnDelete.Location = New Point(Width - 135, 10)
            _btnDelete.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnDelete.Click, AddressOf DeleteSelected

            Dim pnlFilters As New Panel() With {
                .Location = New Point(25, 60),
                .Size = New Size(940, 45),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            }

            _txtSearch.PlaceholderText = "Search by source, notes..."
            _txtSearch.Location = New Point(0, 5)
            _txtSearch.Size = New Size(220, 34)
            AddHandler _txtSearch.TextChanged, Sub() RefreshData()

            _cboFilterCategory.Location = New Point(230, 5)
            _cboFilterCategory.Size = New Size(160, 34)

            _dtpFrom.Location = New Point(400, 5)
            _dtpFrom.Size = New Size(120, 34)
            _dtpFrom.Font = New Font("Segoe UI", 9.0F)
            _dtpFrom.Format = DateTimePickerFormat.Short
            _dtpFrom.Value = DateTime.Today.AddMonths(-1)

            _dtpTo.Location = New Point(530, 5)
            _dtpTo.Size = New Size(120, 34)
            _dtpTo.Font = New Font("Segoe UI", 9.0F)
            _dtpTo.Format = DateTimePickerFormat.Short
            _dtpTo.Value = DateTime.Today

            _btnFilter.Text = "Apply"
            _btnFilter.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnFilter.Size = New Size(75, 34)
            _btnFilter.Location = New Point(660, 5)
            AddHandler _btnFilter.Click, Sub() RefreshData()

            _btnReset.Text = "Reset"
            _btnReset.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnReset.Size = New Size(75, 34)
            _btnReset.Location = New Point(745, 5)
            AddHandler _btnReset.Click, AddressOf ResetFilters

            _lblTotalIncome.Text = "Total: 0.00 EGP"
            _lblTotalIncome.Font = New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold)
            _lblTotalIncome.ForeColor = ThemeColors.Success
            _lblTotalIncome.Location = New Point(830, 10)
            _lblTotalIncome.AutoSize = True

            pnlFilters.Controls.AddRange({_txtSearch, _cboFilterCategory, _dtpFrom, _dtpTo, _btnFilter, _btnReset, _lblTotalIncome})

            pnlTop.Controls.AddRange({lblTitle, _btnAdd, _btnEdit, _btnDelete, pnlFilters})

            Dim pnlGrid As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 10, 25, 20)
            }

            _gridIncome.Dock = DockStyle.Fill
            SetupGridColumns()
            pnlGrid.Controls.Add(_gridIncome)

            Controls.Add(pnlGrid)
            Controls.Add(pnlTop)
        End Sub

        Private Sub SetupGridColumns()
            _gridIncome.Columns.Clear()

            _gridIncome.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColId",
                .HeaderText = "ID",
                .Width = 60,
                .DataPropertyName = "Id"
            })
            _gridIncome.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColDate",
                .HeaderText = "Date",
                .Width = 110,
                .DataPropertyName = "IncomeDate"
            })
            _gridIncome.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColSource",
                .HeaderText = "Source",
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                .DataPropertyName = "Source"
            })
            _gridIncome.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColCategory",
                .HeaderText = "Category",
                .Width = 160,
                .DataPropertyName = "CategoryName"
            })
            _gridIncome.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColAmount",
                .HeaderText = "Amount (EGP)",
                .Width = 130,
                .DataPropertyName = "Amount"
            })
            _gridIncome.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColCreatedBy",
                .HeaderText = "Created By",
                .Width = 130,
                .DataPropertyName = "CreatorName"
            })
            _gridIncome.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColNotes",
                .HeaderText = "Notes",
                .Width = 180,
                .DataPropertyName = "Notes"
            })

            AddHandler _gridIncome.CellFormatting, AddressOf GridCellFormatting
            AddHandler _gridIncome.CellDoubleClick, Sub() OpenEditDialog(Nothing, EventArgs.Empty)
        End Sub

        Private Sub GridCellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
            If e.RowIndex < 0 Then Return

            If _gridIncome.Columns(e.ColumnIndex).Name = "ColDate" AndAlso TypeOf e.Value Is DateTime Then
                e.Value = DirectCast(e.Value, DateTime).ToString("yyyy-MM-dd")
                e.FormattingApplied = True
            ElseIf _gridIncome.Columns(e.ColumnIndex).Name = "ColAmount" AndAlso TypeOf e.Value Is Decimal Then
                e.Value = $"{CDec(e.Value):N2} EGP"
                e.CellStyle.ForeColor = ThemeColors.Success
                e.CellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                e.FormattingApplied = True
            End If
        End Sub

        Private Sub LoadCategoryFilter()
            Dim cats = _categoryService.GetAllCategories(CategoryType.Income)
            Dim filterList As New List(Of Category)()
            filterList.Add(New Category() With {.Id = 0, .Name = "All Categories"})
            filterList.AddRange(cats)

            _cboFilterCategory.DisplayMember = "Name"
            _cboFilterCategory.ValueMember = "Id"
            _cboFilterCategory.DataSource = filterList
        End Sub

        Private Sub ResetFilters(sender As Object, e As EventArgs)
            _txtSearch.Text = ""
            _cboFilterCategory.SelectedIndex = 0
            _dtpFrom.Value = DateTime.Today.AddMonths(-1)
            _dtpTo.Value = DateTime.Today
            RefreshData()
        End Sub

        Public Sub RefreshData()
            Dim catId As Nullable(Of Integer) = Nothing
            If _cboFilterCategory.SelectedValue IsNot Nothing AndAlso Convert.ToInt32(_cboFilterCategory.SelectedValue) > 0 Then
                catId = Convert.ToInt32(_cboFilterCategory.SelectedValue)
            End If

            Dim list = _incomeService.GetAllIncome(catId, _dtpFrom.Value.Date, _dtpTo.Value.Date, _txtSearch.Text)
            _gridIncome.DataSource = Nothing
            _gridIncome.DataSource = list

            Dim total = list.Sum(Function(x) x.Amount)
            _lblTotalIncome.Text = $"Total: {total:N2} EGP"
        End Sub

        Private Sub OpenAddDialog(sender As Object, e As EventArgs)
            Using dlg As New IncomeDialog()
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub OpenEditDialog(sender As Object, e As EventArgs)
            If _gridIncome.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select an income record to edit.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim inc = DirectCast(_gridIncome.SelectedRows(0).DataBoundItem, Income)
            Using dlg As New IncomeDialog(inc.Id)
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub DeleteSelected(sender As Object, e As EventArgs)
            If _gridIncome.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select an income record to delete.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim inc = DirectCast(_gridIncome.SelectedRows(0).DataBoundItem, Income)
            If CustomMessageBox.Confirm($"Are you sure you want to delete income '{inc.Source}' of ${inc.Amount:N2}?", "Confirm Delete", FindForm()) Then
                Dim res = _incomeService.DeleteIncome(inc.Id)
                If res.Success Then
                    RefreshData()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, FindForm())
                End If
            End If
        End Sub
    End Class
End Namespace
