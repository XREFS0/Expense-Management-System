Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports MasaExpenseManager.UI.Theme

Namespace UI.Controls
    Public Class CustomButton
        Inherits Button

        Private _isHovered As Boolean = False
        Private _isPressed As Boolean = False
        Private _borderRadius As Integer = 6
        Private _buttonStyle As ButtonStyleType = ButtonStyleType.Primary

        Public Enum ButtonStyleType
            Primary
            Success
            Danger
            Warning
            Secondary
            Outline
        End Enum

        Public Property ButtonStyle As ButtonStyleType
            Get
                Return _buttonStyle
            End Get
            Set(value As ButtonStyleType)
                _buttonStyle = value
                Invalidate()
            End Set
        End Property

        Public Property BorderRadius As Integer
            Get
                Return _borderRadius
            End Get
            Set(value As Integer)
                _borderRadius = value
                Invalidate()
            End Set
        End Property

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            FlatStyle = FlatStyle.Flat
            FlatAppearance.BorderSize = 0
            Cursor = Cursors.Hand
            Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            ForeColor = Color.White
            Size = New Size(110, 36)
        End Sub

        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            MyBase.OnMouseEnter(e)
            _isHovered = True
            Invalidate()
        End Sub

        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            MyBase.OnMouseLeave(e)
            _isHovered = False
            _isPressed = False
            Invalidate()
        End Sub

        Protected Overrides Sub OnMouseDown(mevent As MouseEventArgs)
            MyBase.OnMouseDown(mevent)
            _isPressed = True
            Invalidate()
        End Sub

        Protected Overrides Sub OnMouseUp(mevent As MouseEventArgs)
            MyBase.OnMouseUp(mevent)
            _isPressed = False
            Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(pevent As PaintEventArgs)
            Dim g As Graphics = pevent.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            Dim baseColor As Color
            Dim hoverColor As Color

            Select Case _buttonStyle
                Case ButtonStyleType.Primary
                    baseColor = ThemeColors.Primary
                    hoverColor = ThemeColors.PrimaryHover
                    ForeColor = Color.White
                Case ButtonStyleType.Success
                    baseColor = ThemeColors.Success
                    hoverColor = ThemeColors.SuccessHover
                    ForeColor = Color.White
                Case ButtonStyleType.Danger
                    baseColor = ThemeColors.Danger
                    hoverColor = ThemeColors.DangerHover
                    ForeColor = Color.White
                Case ButtonStyleType.Warning
                    baseColor = ThemeColors.Warning
                    hoverColor = ThemeColors.WarningHover
                    ForeColor = Color.FromArgb(20, 20, 30)
                Case ButtonStyleType.Secondary
                    baseColor = ThemeColors.CardBorder
                    hoverColor = ThemeColors.SidebarHover
                    ForeColor = ThemeColors.TextPrimary
                Case ButtonStyleType.Outline
                    baseColor = Color.Transparent
                    hoverColor = Color.FromArgb(35, ThemeColors.Primary)
                    ForeColor = ThemeColors.Primary
            End Select

            Dim fillCol As Color = If(_isPressed, Color.FromArgb(220, baseColor), If(_isHovered, hoverColor, baseColor))
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)

            Using path As GraphicsPath = GetRoundedRectanglePath(rect, _borderRadius)
                If _buttonStyle = ButtonStyleType.Outline Then
                    Dim clearColor As Color = ThemeColors.CardBackground
                    If Parent IsNot Nothing Then clearColor = Parent.BackColor
                    g.Clear(clearColor)
                    If _isHovered Then
                        Using br As New SolidBrush(hoverColor)
                            g.FillPath(br, path)
                        End Using
                    End If
                    Using pen As New Pen(ThemeColors.Primary, 1.5F)
                        g.DrawPath(pen, path)
                    End Using
                Else
                    Using br As New SolidBrush(fillCol)
                        g.FillPath(br, path)
                    End Using
                End If
            End Using

            Dim sf As New StringFormat() With {
                .Alignment = StringAlignment.Center,
                .LineAlignment = StringAlignment.Center
            }
            Using textBrush As New SolidBrush(ForeColor)
                g.DrawString(Text, Font, textBrush, ClientRectangle, sf)
            End Using
        End Sub

        Private Function GetRoundedRectanglePath(rect As Rectangle, radius As Integer) As GraphicsPath
            Dim path As New GraphicsPath()
            Dim d As Integer = radius * 2
            If d > rect.Height Then d = rect.Height
            If d > rect.Width Then d = rect.Width

            path.AddArc(rect.X, rect.Y, d, d, 180, 90)
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
            path.CloseFigure()
            Return path
        End Function
    End Class

    Public Class CustomTextBox
        Inherits UserControl

        Private ReadOnly _innerTextBox As New TextBox()
        Private _isFocused As Boolean = False
        Private _borderRadius As Integer = 6
        Private _placeholder As String = String.Empty

        Public Property PlaceholderText As String
            Get
                Return _placeholder
            End Get
            Set(value As String)
                _placeholder = value
                Invalidate()
            End Set
        End Property

        Public Overrides Property Text As String
            Get
                Return _innerTextBox.Text
            End Get
            Set(value As String)
                _innerTextBox.Text = value
                Invalidate()
            End Set
        End Property

        Public Property UseSystemPasswordChar As Boolean
            Get
                Return _innerTextBox.UseSystemPasswordChar
            End Get
            Set(value As Boolean)
                _innerTextBox.UseSystemPasswordChar = value
            End Set
        End Property

        Public Property Multiline As Boolean
            Get
                Return _innerTextBox.Multiline
            End Get
            Set(value As Boolean)
                _innerTextBox.Multiline = value
                AdjustInnerSize()
            End Set
        End Property

        Public Shadows Event TextChanged As EventHandler

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            Padding = New Padding(10, 8, 10, 8)
            BackColor = ThemeColors.InputBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(220, 38)

            _innerTextBox.BorderStyle = BorderStyle.None
            _innerTextBox.BackColor = ThemeColors.InputBackground
            _innerTextBox.ForeColor = ThemeColors.TextPrimary
            _innerTextBox.Font = New Font("Segoe UI", 9.5F)
            _innerTextBox.Dock = DockStyle.Fill

            AddHandler _innerTextBox.GotFocus, AddressOf OnInnerGotFocus
            AddHandler _innerTextBox.LostFocus, AddressOf OnInnerLostFocus
            AddHandler _innerTextBox.TextChanged, Sub(s, e)
                                                      RaiseEvent TextChanged(Me, e)
                                                      Invalidate()
                                                  End Sub

            Controls.Add(_innerTextBox)
        End Sub

        Private Sub AdjustInnerSize()
            If Not Multiline Then
                Height = 38
            End If
        End Sub

        Private Sub OnInnerGotFocus(sender As Object, e As EventArgs)
            _isFocused = True
            Invalidate()
        End Sub

        Private Sub OnInnerLostFocus(sender As Object, e As EventArgs)
            _isFocused = False
            Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim borderCol As Color = If(_isFocused, ThemeColors.InputFocusBorder, ThemeColors.InputBorder)
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)

            Using bgBrush As New SolidBrush(ThemeColors.InputBackground)
                g.FillRectangle(bgBrush, ClientRectangle)
            End Using

            Using pen As New Pen(borderCol, If(_isFocused, 1.8F, 1.0F))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1)
            End Using

            If String.IsNullOrEmpty(_innerTextBox.Text) AndAlso Not _isFocused AndAlso Not String.IsNullOrEmpty(_placeholder) Then
                Using brush As New SolidBrush(ThemeColors.TextMuted)
                    g.DrawString(_placeholder, _innerTextBox.Font, brush, New PointF(10, 9))
                End Using
            End If
        End Sub
    End Class

    Public Class CustomCard
        Inherits Panel

        Public Property CardTitle As String = String.Empty
        Public Property CardValue As String = String.Empty
        Public Property Subtitle As String = String.Empty
        Public Property AccentColor As Color = ThemeColors.Primary

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            BackColor = ThemeColors.CardBackground
            Padding = New Padding(18)
            Size = New Size(220, 110)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            Using brush As New SolidBrush(ThemeColors.CardBackground)
                g.FillRectangle(brush, ClientRectangle)
            End Using

            Using pen As New Pen(ThemeColors.CardBorder, 1.0F)
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1)
            End Using

            Using accentBrush As New SolidBrush(AccentColor)
                g.FillRectangle(accentBrush, 0, 0, 4, Height)
            End Using

            If Not String.IsNullOrEmpty(CardTitle) Then
                Using titleFont As New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold)
                    Using titleBrush As New SolidBrush(ThemeColors.TextSecondary)
                        g.DrawString(CardTitle.ToUpper(), titleFont, titleBrush, New PointF(16, 16))
                    End Using
                End Using
            End If

            If Not String.IsNullOrEmpty(CardValue) Then
                Using valFont As New Font("Segoe UI", 18.0F, FontStyle.Bold)
                    Using valBrush As New SolidBrush(ThemeColors.TextPrimary)
                        g.DrawString(CardValue, valFont, valBrush, New PointF(15, 40))
                    End Using
                End Using
            End If

            If Not String.IsNullOrEmpty(Subtitle) Then
                Using subFont As New Font("Segoe UI", 8.5F)
                    Using subBrush As New SolidBrush(ThemeColors.TextMuted)
                        g.DrawString(Subtitle, subFont, subBrush, New PointF(16, 78))
                    End Using
                End Using
            End If
        End Sub
    End Class
End Namespace
