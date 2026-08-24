Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports MasaExpenseManager.Models
Imports MasaExpenseManager.UI.Theme

Namespace UI.Controls
    Public Class DonutChartControl
        Inherits Control

        Private _items As New List(Of CategoryBreakdownItem)()

        Public Property Items As List(Of CategoryBreakdownItem)
            Get
                Return _items
            End Get
            Set(value As List(Of CategoryBreakdownItem))
                _items = If(value, New List(Of CategoryBreakdownItem)())
                Invalidate()
            End Set
        End Property

        Public Property Title As String = "Expense Breakdown"

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            BackColor = ThemeColors.CardBackground
            Size = New Size(320, 260)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            Using bgBrush As New SolidBrush(ThemeColors.CardBackground)
                g.FillRectangle(bgBrush, ClientRectangle)
            End Using

            Using pen As New Pen(ThemeColors.CardBorder, 1.0F)
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1)
            End Using

            Using titleFont As New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold)
                Using titleBrush As New SolidBrush(ThemeColors.TextPrimary)
                    g.DrawString(Title, titleFont, titleBrush, New PointF(16, 14))
                End Using
            End Using

            If _items.Count = 0 Then
                Using emptyFont As New Font("Segoe UI", 9.5F)
                    Using emptyBrush As New SolidBrush(ThemeColors.TextMuted)
                        Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                        g.DrawString("No expense data for this period", emptyFont, emptyBrush, New RectangleF(0, 40, Width, Height - 40), sf)
                    End Using
                End Using
                Return
            End If

            Dim chartSize As Integer = Math.Min(Height - 70, 160)
            Dim chartRect As New Rectangle(20, 55, chartSize, chartSize)

            Dim totalVal As Double = _items.Sum(Function(x) CDbl(x.TotalAmount))
            If totalVal <= 0 Then totalVal = 1

            Dim startAngle As Single = -90.0F
            For Each item In _items
                Dim sweepAngle As Single = CSng((CDbl(item.TotalAmount) / totalVal) * 360.0)
                If sweepAngle > 0 Then
                    Dim col As Color = ColorTranslator.FromHtml(item.ColorHex)
                    Using br As New SolidBrush(col)
                        g.FillPie(br, chartRect, startAngle, sweepAngle)
                    End Using
                    startAngle += sweepAngle
                End If
            Next

            Dim holeSize As Integer = CInt(chartSize * 0.58)
            Dim holeRect As New Rectangle(chartRect.X + (chartSize - holeSize) \ 2, chartRect.Y + (chartSize - holeSize) \ 2, holeSize, holeSize)
            Using holeBrush As New SolidBrush(ThemeColors.CardBackground)
                g.FillEllipse(holeBrush, holeRect)
            End Using

            Dim legendX As Integer = chartRect.Right + 20
            Dim legendY As Integer = 55
            Dim maxLegendItems As Integer = Math.Min(_items.Count, 5)

            Using legendFont As New Font("Segoe UI", 8.5F)
                For i As Integer = 0 To maxLegendItems - 1
                    Dim item = _items(i)
                    Dim col As Color = ColorTranslator.FromHtml(item.ColorHex)
                    Using colBrush As New SolidBrush(col)
                        g.FillRectangle(colBrush, legendX, legendY + 3, 10, 10)
                    End Using

                    Using textBrush As New SolidBrush(ThemeColors.TextPrimary)
                        Dim label As String = $"{item.CategoryName} ({item.Percentage:F1}%)"
                        If label.Length > 20 Then label = label.Substring(0, 17) & "..."
                        g.DrawString(label, legendFont, textBrush, New PointF(legendX + 16, legendY))
                    End Using
                    legendY += 22
                Next
            End Using
        End Sub
    End Class

    Public Class BarChartControl
        Inherits Control

        Private _trends As New List(Of MonthlyTrendItem)()

        Public Property Trends As List(Of MonthlyTrendItem)
            Get
                Return _trends
            End Get
            Set(value As List(Of MonthlyTrendItem))
                _trends = If(value, New List(Of MonthlyTrendItem)())
                Invalidate()
            End Set
        End Property

        Public Property Title As String = "Income vs Expenses (6-Month Trend)"

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            BackColor = ThemeColors.CardBackground
            Size = New Size(460, 260)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            Using bgBrush As New SolidBrush(ThemeColors.CardBackground)
                g.FillRectangle(bgBrush, ClientRectangle)
            End Using

            Using pen As New Pen(ThemeColors.CardBorder, 1.0F)
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1)
            End Using

            Using titleFont As New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold)
                Using titleBrush As New SolidBrush(ThemeColors.TextPrimary)
                    g.DrawString(Title, titleFont, titleBrush, New PointF(16, 14))
                End Using
            End Using

            Dim legX As Integer = Width - 180
            Using legFont As New Font("Segoe UI", 8.5F)
                Using incBrush As New SolidBrush(ThemeColors.Success)
                    g.FillRectangle(incBrush, legX, 16, 10, 10)
                End Using
                Using tBrush As New SolidBrush(ThemeColors.TextSecondary)
                    g.DrawString("Income", legFont, tBrush, New PointF(legX + 14, 13))
                End Using

                Using expBrush As New SolidBrush(ThemeColors.Danger)
                    g.FillRectangle(expBrush, legX + 75, 16, 10, 10)
                End Using
                Using tBrush As New SolidBrush(ThemeColors.TextSecondary)
                    g.DrawString("Expense", legFont, tBrush, New PointF(legX + 89, 13))
                End Using
            End Using

            If _trends.Count = 0 Then
                Using emptyFont As New Font("Segoe UI", 9.5F)
                    Using emptyBrush As New SolidBrush(ThemeColors.TextMuted)
                        Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                        g.DrawString("No trend data available", emptyFont, emptyBrush, New RectangleF(0, 40, Width, Height - 40), sf)
                    End Using
                End Using
                Return
            End If

            Dim plotX As Integer = 45
            Dim plotY As Integer = 50
            Dim plotW As Integer = Width - 65
            Dim plotH As Integer = Height - 95

            Dim maxVal As Decimal = 100
            For Each item In _trends
                If item.IncomeAmount > maxVal Then maxVal = item.IncomeAmount
                If item.ExpenseAmount > maxVal Then maxVal = item.ExpenseAmount
            Next
            maxVal = Math.Ceiling(maxVal * 1.15D)

            Using gridPen As New Pen(ThemeColors.CardBorder, 1.0F) With {.DashStyle = DashStyle.Dash}
                For stepIdx As Integer = 0 To 3
                    Dim lineY As Single = plotY + (plotH / 3.0F) * stepIdx
                    g.DrawLine(gridPen, plotX, lineY, plotX + plotW, lineY)
                    Dim valAtLine As Decimal = maxVal - (maxVal / 3.0D) * stepIdx
                    Using f As New Font("Segoe UI", 7.5F)
                        Using b As New SolidBrush(ThemeColors.TextMuted)
                            g.DrawString($"{valAtLine:N0} EGP", f, b, New PointF(2, lineY - 6))
                        End Using
                    End Using
                Next
            End Using

            Dim groupCount As Integer = _trends.Count
            Dim groupWidth As Single = plotW / CSng(groupCount)
            Dim barWidth As Single = Math.Min(groupWidth * 0.32F, 22.0F)

            For i As Integer = 0 To groupCount - 1
                Dim item = _trends(i)
                Dim centerX As Single = plotX + (i * groupWidth) + (groupWidth / 2.0F)

                Dim incHeight As Single = CSng((item.IncomeAmount / maxVal) * plotH)
                Dim expHeight As Single = CSng((item.ExpenseAmount / maxVal) * plotH)

                Dim incRect As New RectangleF(centerX - barWidth - 2, plotY + plotH - incHeight, barWidth, incHeight)
                Using incBrush As New SolidBrush(ThemeColors.Success)
                    g.FillRectangle(incBrush, incRect)
                End Using

                Dim expRect As New RectangleF(centerX + 2, plotY + plotH - expHeight, barWidth, expHeight)
                Using expBrush As New SolidBrush(ThemeColors.Danger)
                    g.FillRectangle(expBrush, expRect)
                End Using

                Using mFont As New Font("Segoe UI", 8.0F)
                    Using mBrush As New SolidBrush(ThemeColors.TextSecondary)
                        Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center}
                        g.DrawString(item.MonthName, mFont, mBrush, New PointF(centerX, plotY + plotH + 6), sf)
                    End Using
                End Using
            Next
        End Sub
    End Class
End Namespace
