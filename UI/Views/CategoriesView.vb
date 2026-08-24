Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.Business.Services
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Controls
Imports MasaExpenseManager.UI.Theme

Namespace UI.Views
    Public Class CategoryDialog
        Inherits Form

        Private ReadOnly _categoryService As New CategoryService()
        Private ReadOnly _txtName As New CustomTextBox()
        Private ReadOnly _cboType As New ModernComboBox()
        Private ReadOnly _txtColorHex As New CustomTextBox()
        Private ReadOnly _btnPickColor As New CustomButton()
        Private ReadOnly _txtDescription As New CustomTextBox()
        Private ReadOnly _btnSave As New CustomButton()
        Private ReadOnly _btnCancel As New CustomButton()
        Private ReadOnly _pnlColorPreview As New Panel()
        Private _editingId As Nullable(Of Integer) = Nothing

        Public Sub New(Optional categoryId As Nullable(Of Integer) = Nothing)
            _editingId = categoryId
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.CenterParent
            BackColor = ThemeColors.CardBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(440, 420)
            ShowInTaskbar = False

            InitializeUI()
            LoadCategoryTypes()

            If _editingId.HasValue Then
                LoadCategoryData(_editingId.Value)
            Else
                _txtColorHex.Text = "#3699FF"
                UpdateColorPreview("#3699FF")
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
                .Text = If(_editingId.HasValue, "Edit Category", "New Category"),
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

            Dim lblN As New Label() With {.Text = "Category Name *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtName.Location = New Point(25, y)
            _txtName.Size = New Size(390, 36)
            _txtName.PlaceholderText = "e.g. Travel, Cloud Hosting, Marketing"
            y += 48

            Dim lblType As New Label() With {.Text = "Category Type *", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _cboType.Location = New Point(25, y)
            _cboType.Size = New Size(390, 36)
            y += 48

            Dim lblColor As New Label() With {.Text = "Badge Color (HEX)", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22

            _pnlColorPreview.Location = New Point(25, y + 2)
            _pnlColorPreview.Size = New Size(32, 32)
            _pnlColorPreview.BackColor = Color.FromArgb(54, 153, 255)

            _txtColorHex.Location = New Point(65, y)
            _txtColorHex.Size = New Size(220, 36)
            _txtColorHex.PlaceholderText = "#3699FF"
            AddHandler _txtColorHex.TextChanged, Sub() UpdateColorPreview(_txtColorHex.Text)

            _btnPickColor.Text = "Pick..."
            _btnPickColor.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnPickColor.Size = New Size(95, 36)
            _btnPickColor.Location = New Point(295, y)
            AddHandler _btnPickColor.Click, AddressOf PickColor
            y += 48

            Dim lblDesc As New Label() With {.Text = "Description", .ForeColor = ThemeColors.TextSecondary, .Font = New Font("Segoe UI", 9.0F), .Location = New Point(25, y), .AutoSize = True}
            y += 22
            _txtDescription.Location = New Point(25, y)
            _txtDescription.Size = New Size(390, 60)
            _txtDescription.Multiline = True
            _txtDescription.PlaceholderText = "Category usage notes..."

            pnlBody.Controls.AddRange({lblN, _txtName, lblType, _cboType, lblColor, _pnlColorPreview, _txtColorHex, _btnPickColor, lblDesc, _txtDescription})

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

            _btnSave.Text = "Save"
            _btnSave.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnSave.Size = New Size(110, 36)
            _btnSave.Location = New Point(315, 12)
            AddHandler _btnSave.Click, AddressOf SaveCategory

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

        Private Sub LoadCategoryTypes()
            _cboType.Items.Clear()
            _cboType.Items.Add(CategoryType.Expense)
            _cboType.Items.Add(CategoryType.Income)
            _cboType.SelectedIndex = 0
        End Sub

        Private Sub LoadCategoryData(id As Integer)
            Dim cat As Category = _categoryService.GetCategoryById(id)
            If cat IsNot Nothing Then
                _txtName.Text = cat.Name
                _cboType.SelectedItem = cat.Type
                _txtColorHex.Text = cat.ColorHex
                _txtDescription.Text = cat.Description
                UpdateColorPreview(cat.ColorHex)
            End If
        End Sub

        Private Sub UpdateColorPreview(hex As String)
            Try
                _pnlColorPreview.BackColor = ColorTranslator.FromHtml(hex)
            Catch
            End Try
        End Sub

        Private Sub PickColor(sender As Object, e As EventArgs)
            Using cd As New ColorDialog()
                If cd.ShowDialog() = DialogResult.OK Then
                    Dim hex As String = $"#{cd.Color.R:X2}{cd.Color.G:X2}{cd.Color.B:X2}"
                    _txtColorHex.Text = hex
                    _pnlColorPreview.BackColor = cd.Color
                End If
            End Using
        End Sub

        Private Sub SaveCategory(sender As Object, e As EventArgs)
            Dim name As String = _txtName.Text.Trim()
            If String.IsNullOrWhiteSpace(name) Then
                CustomMessageBox.Show("Please enter a category name.", "Validation Error", CustomMessageBox.MessageType.Warning, Me)
                Return
            End If

            Dim catType As CategoryType = If(_cboType.SelectedItem IsNot Nothing, CType(_cboType.SelectedItem, CategoryType), CategoryType.Expense)
            Dim hex As String = If(String.IsNullOrWhiteSpace(_txtColorHex.Text), "#3699FF", _txtColorHex.Text.Trim())
            Dim desc As String = _txtDescription.Text.Trim()

            If _editingId.HasValue Then
                Dim res = _categoryService.UpdateCategory(_editingId.Value, name, catType, hex, "tag", desc)
                If res.Success Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            Else
                Dim res = _categoryService.CreateCategory(name, catType, hex, "tag", desc)
                If res.Success Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    CustomMessageBox.Show(res.Message, "Error", CustomMessageBox.MessageType.Error, Me)
                End If
            End If
        End Sub
    End Class

    Public Class CategoriesView
        Inherits UserControl

        Private ReadOnly _categoryService As New CategoryService()
        Private ReadOnly _gridCategories As New ModernDataGridView()
        Private ReadOnly _cboTypeFilter As New ModernComboBox()
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
                .Height = 75,
                .Padding = New Padding(25, 15, 25, 0)
            }

            Dim lblTitle As New Label() With {
                .Text = "Category Management",
                .Font = New Font("Segoe UI Semibold", 14.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .AutoSize = True,
                .Location = New Point(25, 12)
            }

            _cboTypeFilter.Location = New Point(25, 40)
            _cboTypeFilter.Size = New Size(180, 32)
            _cboTypeFilter.Items.AddRange({"All Types", "Expense Categories", "Income Categories"})
            _cboTypeFilter.SelectedIndex = 0
            AddHandler _cboTypeFilter.SelectedIndexChanged, Sub() RefreshData()

            _btnAdd.Text = "Add Category"
            _btnAdd.ButtonStyle = CustomButton.ButtonStyleType.Primary
            _btnAdd.Size = New Size(140, 36)
            _btnAdd.Location = New Point(Width - 375, 18)
            _btnAdd.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnAdd.Click, AddressOf OpenAddDialog

            _btnEdit.Text = "Edit"
            _btnEdit.ButtonStyle = CustomButton.ButtonStyleType.Secondary
            _btnEdit.Size = New Size(80, 36)
            _btnEdit.Location = New Point(Width - 225, 18)
            _btnEdit.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnEdit.Click, AddressOf OpenEditDialog

            _btnDelete.Text = "Delete"
            _btnDelete.ButtonStyle = CustomButton.ButtonStyleType.Danger
            _btnDelete.Size = New Size(80, 36)
            _btnDelete.Location = New Point(Width - 135, 18)
            _btnDelete.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            AddHandler _btnDelete.Click, AddressOf DeleteSelected

            pnlTop.Controls.AddRange({lblTitle, _cboTypeFilter, _btnAdd, _btnEdit, _btnDelete})

            Dim pnlGrid As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(25, 10, 25, 20)
            }

            _gridCategories.Dock = DockStyle.Fill
            SetupGridColumns()
            pnlGrid.Controls.Add(_gridCategories)

            Controls.Add(pnlGrid)
            Controls.Add(pnlTop)
        End Sub

        Private Sub SetupGridColumns()
            _gridCategories.Columns.Clear()

            _gridCategories.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColId",
                .HeaderText = "ID",
                .Width = 60,
                .DataPropertyName = "Id"
            })
            _gridCategories.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColName",
                .HeaderText = "Category Name",
                .Width = 220,
                .DataPropertyName = "Name"
            })
            _gridCategories.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColType",
                .HeaderText = "Type",
                .Width = 140,
                .DataPropertyName = "Type"
            })
            _gridCategories.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColColor",
                .HeaderText = "Color Tag",
                .Width = 120,
                .DataPropertyName = "ColorHex"
            })
            _gridCategories.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColDesc",
                .HeaderText = "Description",
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                .DataPropertyName = "Description"
            })
            _gridCategories.Columns.Add(New DataGridViewTextBoxColumn() With {
                .Name = "ColCreated",
                .HeaderText = "Created Date",
                .Width = 130,
                .DataPropertyName = "CreatedAt"
            })

            AddHandler _gridCategories.CellFormatting, AddressOf GridCellFormatting
            AddHandler _gridCategories.CellDoubleClick, Sub() OpenEditDialog(Nothing, EventArgs.Empty)
        End Sub

        Private Sub GridCellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
            If e.RowIndex < 0 Then Return

            If _gridCategories.Columns(e.ColumnIndex).Name = "ColCreated" AndAlso TypeOf e.Value Is DateTime Then
                e.Value = DirectCast(e.Value, DateTime).ToString("yyyy-MM-dd")
                e.FormattingApplied = True
            ElseIf _gridCategories.Columns(e.ColumnIndex).Name = "ColType" AndAlso TypeOf e.Value Is CategoryType Then
                Dim t As CategoryType = DirectCast(e.Value, CategoryType)
                e.Value = t.ToString()
                e.CellStyle.ForeColor = If(t = CategoryType.Expense, ThemeColors.Danger, ThemeColors.Success)
                e.CellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                e.FormattingApplied = True
            ElseIf _gridCategories.Columns(e.ColumnIndex).Name = "ColColor" AndAlso e.Value IsNot Nothing Then
                Dim hex As String = e.Value.ToString()
                Try
                    e.CellStyle.ForeColor = ColorTranslator.FromHtml(hex)
                    e.CellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                Catch
                End Try
            End If
        End Sub

        Public Sub RefreshData()
            Dim filterType As Nullable(Of CategoryType) = Nothing
            If _cboTypeFilter.SelectedIndex = 1 Then
                filterType = CategoryType.Expense
            ElseIf _cboTypeFilter.SelectedIndex = 2 Then
                filterType = CategoryType.Income
            End If

            Dim list = _categoryService.GetAllCategories(filterType)
            _gridCategories.DataSource = Nothing
            _gridCategories.DataSource = list
        End Sub

        Private Sub OpenAddDialog(sender As Object, e As EventArgs)
            Using dlg As New CategoryDialog()
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub OpenEditDialog(sender As Object, e As EventArgs)
            If _gridCategories.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select a category to edit.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim cat = DirectCast(_gridCategories.SelectedRows(0).DataBoundItem, Category)
            Using dlg As New CategoryDialog(cat.Id)
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    RefreshData()
                End If
            End Using
        End Sub

        Private Sub DeleteSelected(sender As Object, e As EventArgs)
            If _gridCategories.SelectedRows.Count = 0 Then
                CustomMessageBox.Show("Please select a category to delete.", "Selection Required", CustomMessageBox.MessageType.Information, FindForm())
                Return
            End If

            Dim cat = DirectCast(_gridCategories.SelectedRows(0).DataBoundItem, Category)
            If CustomMessageBox.Confirm($"Are you sure you want to delete category '{cat.Name}'?", "Confirm Delete", FindForm()) Then
                Dim res = _categoryService.DeleteCategory(cat.Id)
                If res.Success Then
                    RefreshData()
                Else
                    CustomMessageBox.Show(res.Message, "Deletion Error", CustomMessageBox.MessageType.Warning, FindForm())
                End If
            End If
        End Sub
    End Class
End Namespace
