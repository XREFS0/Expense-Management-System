Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Views
    Public Class ExpenseDialog
        Inherits Form

        Private ReadOnly _categoryService As New CategoryService()
        Private ReadOnly _expenseService As New ExpenseService()
        Private ReadOnly _txtTitle As New CustomTextBox()
        Private ReadOnly _cboCategory As New ModernComboBox()
        Private ReadOnly _txtAmount As New CustomTextBox()
        Private ReadOnly _cboPayment As New ModernComboBox()
        Private ReadOnly _dtpDate As New DateTimePicker()
        Private ReadOnly _txtNotes As New CustomTextBox()
        Private ReadOnly _btnSave As New CustomButton()
        Private ReadOnly _btnCancel As New CustomButton()
        Private _editingId As Nullable(Of Integer) = Nothing

        Public Sub New(Optional expenseId As Nullable(Of Integer) = Nothing)
            _editingId = expenseId
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.CenterParent
            BackColor = ThemeColors.CardBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(480, 520)
            ShowInTaskbar = False

            InitializeUI()
            LoadCategories()
            LoadPaymentMethods()

            If _editingId.HasValue Then
                LoadExpenseData(_editingId.Value)
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
                .Text = If(_editingId.HasValue, "Edit Expense Record", "Record New Expense"),
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

            Dim lblT As New Label() With {.Text = "Expense Title *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtTitle.Location = New Point(25, y)
            _txtTitle.Size = New Size(430, 36)
            _txtTitle.PlaceholderText = "e.g. Server hosting, Team lunch"
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

            Dim lblPay As New Label() With {.Text = "Payment Method", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            Dim lblDate As New Label() With {.Text = "Expense Date", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(245, y), .AutoSize = True}
            y += 22

            _cboPayment.Location = New Point(25, y)
            _cboPayment.Size = New Size(205, 36)

            _dtpDate.Location = New Point(245, y)
            _dtpDate.Size = New Size(210, 36)
            _dtpDate.Font = New Font("Segoe UI", 9.5F)
            _dtpDate.Format = DateTimePickerFormat.Short
            y += 48

            Dim lblNotes As New Label() With {.Text = "Notes / Description", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtNotes.Location = New Point(25, y)
            _txtNotes.Size = New Size(430, 75)
            _txtNotes.Multiline = True
            _txtNotes.PlaceholderText = "Additional details or invoice reference..."

            pnlBody.Controls.AddRange({lblT, _txtTitle, lblCat, lblAmt, _cboCategory, _txtAmount, lblPay, lblDate, _cboPayment, _dtpDate, lblNotes, _txtNotes})

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

            _btnSave.Text = "Save Expense"
            _btnSave.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnSave.Size = New Size(120, 36)
            _btnSave.Location = New Point(355, 12)
            AddHandler _btnSave.Click, AddressOf SaveExpense

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
            Dim cats = _categoryService.GetAllCategories(CategoryType.Expense)
            _cboCategory.DisplayMember = "Name"
            _cboCategory.ValueMember = "Id"
            _cboCategory.DataSource = cats
        End Sub

        Private Sub LoadPaymentMethods()
            _cboPayment.Items.Clear()
            _cboPayment.Items.Add(PaymentMethod.Cash)
            _cboPayment.Items.Add(PaymentMethod.CreditCard)
            _cboPayment.Items.Add(PaymentMethod.DebitCard)
            _cboPayment.Items.Add(PaymentMethod.BankTransfer)
            _cboPayment.Items.Add(PaymentMethod.EWallet)
            _cboPayment.Items.Add(PaymentMethod.Other)
            _cboPayment.SelectedIndex = 0
        End Sub

        Private Sub LoadExpenseData(id As Integer)
            Dim exp As Expense = _expenseService.GetExpenseById(id)
            If exp IsNot Nothing Then
                _txtTitle.Text = exp.Title
                _txtAmount.Text = exp.Amount.ToString("0.00")
                _dtpDate.Value = exp.ExpenseDate
                _txtNotes.Text = exp.Notes
                _cboCategory.SelectedValue = exp.CategoryId
                _cboPayment.SelectedItem = exp.PaymentMethod
            End If
        End Sub

        Private Sub SaveExpense(sender As Object, e As EventArgs)
            Dim title As String = _txtTitle.Text.Trim()
            If String.IsNullOrWhiteSpace(title) Then
                CustomMessageBox.Show("Please enter an expense title.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            If _cboCategory.SelectedValue Is Nothing Then
                CustomMessageBox.Show("Please select a category.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            Dim catId As Integer = Convert.ToInt32(_cboCategory.SelectedValue)
            Dim amount As Decimal = 0
            If Not Decimal.TryParse(_txtAmount.Text.Trim(), amount) OrElse amount <= 0 Then
                CustomMessageBox.Show("Please enter a valid positive expense amount.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            Dim payMethod As PaymentMethod = If(_cboPayment.SelectedItem IsNot Nothing, CType(_cboPayment.SelectedItem, PaymentMethod), PaymentMethod.Cash)
            Dim expDate As DateTime = _dtpDate.Value.Date
            Dim notes As String = _txtNotes.Text.Trim()

            If _editingId.HasValue Then
                Dim res = _expenseService.UpdateExpense(_editingId.Value, title, catId, amount, payMethod, expDate, notes)
                If res.Success Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            Else
                Dim userId As Integer = If(AuthService.CurrentUser IsNot Nothing, AuthService.CurrentUser.Id, 1)
                Dim res = _expenseService.AddExpense(title, catId, amount, payMethod, expDate, notes, userId)
                If res.Success Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            End If
        End Sub
    End Class

    Public Class ExpensesView
        Inherits UserControl

        Private ReadOnly _expenseService As New ExpenseService()
        Private ReadOnly _categoryService As New CategoryService()
        Private ReadOnly _gridExpenses As New ModernDataGridView()
        Private ReadOnly _txtSearch As New CustomTextBox()
        Private ReadOnly _cboFilterCategory As New ModernComboBox()
        Private ReadOnly _dtpFrom As New DateTimePicker()
        Private ReadOnly _dtpTo As New DateTimePicker()
        Private ReadOnly _btnAdd As New CustomButton()
        Private ReadOnly _btnEdit As New CustomButton()
        Private ReadOnly _btnDelete As New CustomButton()
        Private ReadOnly _btnFilter As New CustomButton()
        Private ReadOnly _btnReset As New CustomButton()
        Private ReadOnly _lblTotalExpenses As New Label()

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
                .Text = "Expense Management",
                .Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(25, 12)
            }

            _btnAdd.Text = "Add Expense"
            _btnAdd.ButtonStyle = CustomButton.ButtonStyleType.Primary
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

            _txtSearch.PlaceholderText = "Search by title, notes..."
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

            _lblTotalExpenses.Text = "Total: 0.00 EGP"
            _lblTotalExpenses.Font = New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold)
            _lblTotalExpenses.ForeColor = ThemeColors.Danger
            _lblTotalExpenses.Location = New Point(830, 10)
            _lblTotalExpenses.AutoSize = True

            pnlFilters.Controls.AddRange({_txtSearch, _cboFilterCategory, _dtpFrom, _dtpTo, _btnFilter, _btnReset, _lblTotalExpenses})

            pnlTop.Controls.AddRange({lblTitle, _btnAdd, _btnEdit, _btnDelete, pnlFilters})

            Dim pnlGrid As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 10, 25, 20)
            }

            _gridExpenses.Dock = DockStyle.Fill
            SetupGridColumns()
            pnlGrid.Controls.Add(_gridExpenses)

            Controls.Add(pnlGrid)
            Controls.Add(pnlTop)
        End Sub

        Private Sub SetupGridColumns()
            _gridExpenses.Columns.Clear()

            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColId",
                .HeaderText = "ID",
                .Width = 60,
                .DataPropertyName = "Id"
            })
            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColDate",
                .HeaderText = "Date",
                .Width = 110,
                .DataPropertyName = "ExpenseDate"
            })
            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColTitle",
                .HeaderText = "Title",
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                .DataPropertyName = "Title"
            })
            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColCategory",
                .HeaderText = "Category",
                .Width = 160,
                .DataPropertyName = "CategoryName"
            })
            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColAmount",
                .HeaderText = "Amount (EGP)",
                .Width = 130,
                .DataPropertyName = "Amount"
            })
            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColPayment",
                .HeaderText = "Payment Method",
                .Width = 140,
                .DataPropertyName = "PaymentMethod"
            })
            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColCreatedBy",
                .HeaderText = "Created By",
                .Width = 130,
                .DataPropertyName = "CreatorName"
            })
            _gridExpenses.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColNotes",
                .HeaderText = "Notes",
                .Width = 180,
                .DataPropertyName = "Notes"
            })

            AddHandler _gridExpenses.CellFormatting, AddressOf GridCellFormatting
            AddHandler _gridExpenses.CellDoubleClick, Sub() OpenEditDialog(Nothing, EventArgs.Empty)
        End Sub

        Private Sub GridCellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
            If e.RowIndex < 0 Then Return

            If _gridExpenses.Columns(e.ColumnIndex).Name = "ColDate" AndAlso TypeOf e.Value Is DateTime Then
                e.Value = DirectCast(e.Value, DateTime).ToString("yyyy-MM-dd")
                e.FormattingApplied = True
            ElseIf _gridExpenses.Columns(e.ColumnIndex).Name = "ColAmount" AndAlso TypeOf e.Value Is Decimal Then
                e.Value = $"{CDec(e.Value):N2} EGP"
                e.CellStyle.ForeColor = ThemeColors.Danger
                e.CellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                e.FormattingApplied = True
            End If
        End Sub

        Private Sub LoadCategoryFilter()
            Dim cats = _categoryService.GetAllCategories(CategoryType.Expense)
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

            Dim list = _expenseService.GetAllExpenses(catId, _dtpFrom.Value.Date, _dtpTo.Value.Date, _txtSearch.Text)
            _gridExpenses.DataSource = Nothing
            _gridExpenses.DataSource = list

            Dim total = list.Sum(Function(x) x.Amount)
            _lblTotalExpenses.Text = $"Total: {total:N2} EGP"
        End Sub

        Private Sub OpenAddDialog(sender As Object, e As EventArgs)
            Using dlg As New ExpenseDialog()
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub OpenEditDialog(sender As Object, e As EventArgs)
            If _gridExpenses.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select an expense to edit.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim exp = DirectCast(_gridExpenses.SelectedRows(0).DataBoundItem, Expense)
            Using dlg As New ExpenseDialog(exp.Id)
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub DeleteSelected(sender As Object, e As EventArgs)
            If _gridExpenses.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select an expense to delete.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim exp = DirectCast(_gridExpenses.SelectedRows(0).DataBoundItem, Expense)
            If CustomMessageBox.Confirm($"Are you sure you want to delete expense '{exp.Title}' of ${exp.Amount:N2}?", "Confirm Delete", FindForm()) Then
                Dim res = _expenseService.DeleteExpense(exp.Id)
                If res.Success Then
                    RefreshData()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, FindForm())
                End If
            End If
        End Sub
    End Class
End Namespace
