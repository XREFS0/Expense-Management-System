Imports System.Drawing
Imports System.Windows.Forms
Imports MasaExpenseManager.UI.Theme

Namespace UI.Controls
    Public Class ModernDataGridView
        Inherits DataGridView

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            DoubleBuffered = True
            BackgroundColor = ThemeColors.CardBackground
            BorderStyle = BorderStyle.None
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            GridColor = ThemeColors.GridBorder
            EnableHeadersVisualStyles = False
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            RowHeadersVisible = False
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
            MultiSelect = False
            AutoGenerateColumns = False
            AllowUserToAddRows = False
            AllowUserToDeleteRows = False
            AllowUserToResizeRows = False
            [ReadOnly] = True

            ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.GridHeaderBackground
            ColumnHeadersDefaultCellStyle.ForeColor = ThemeColors.TextSecondary
            ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
            ColumnHeadersDefaultCellStyle.Padding = New Padding(12, 10, 12, 10)
            ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeColors.GridHeaderBackground
            ColumnHeadersDefaultCellStyle.SelectionForeColor = ThemeColors.TextSecondary
            ColumnHeadersHeight = 42
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing

            DefaultCellStyle.BackColor = ThemeColors.GridRowBackground
            DefaultCellStyle.ForeColor = ThemeColors.TextPrimary
            DefaultCellStyle.Font = New Font("Segoe UI", 9.0F)
            DefaultCellStyle.Padding = New Padding(12, 6, 12, 6)
            DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 245, 255)
            DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59)
            RowTemplate.Height = 44

            AlternatingRowsDefaultCellStyle.BackColor = ThemeColors.GridRowAltBackground
            AlternatingRowsDefaultCellStyle.ForeColor = ThemeColors.TextPrimary
            AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 245, 255)
            AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59)
        End Sub
    End Class

    Public Class ModernComboBox
        Inherits ComboBox

        Public Sub New()
            DropDownStyle = ComboBoxStyle.DropDownList
            FlatStyle = FlatStyle.Flat
            BackColor = ThemeColors.InputBackground
            ForeColor = ThemeColors.TextPrimary
            Font = New Font("Segoe UI", 9.5F)
            DrawMode = DrawMode.OwnerDrawFixed
            ItemHeight = 28
        End Sub

        Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
            If e.Index < 0 Then Return

            Dim isSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
            Dim bgCol As Color = If(isSelected, ThemeColors.SidebarHover, ThemeColors.InputBackground)
            Dim textCol As Color = If(isSelected, Color.White, ThemeColors.TextPrimary)

            Using b As New SolidBrush(bgCol)
                e.Graphics.FillRectangle(b, e.Bounds)
            End Using

            Dim itemText As String = GetItemText(Items(e.Index))
            Using tb As New SolidBrush(textCol)
                e.Graphics.DrawString(itemText, Font, tb, New PointF(e.Bounds.X + 8, e.Bounds.Y + 5))
            End Using
        End Sub
    End Class
End Namespace
