Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports MasaExpenseManager.UI.Theme

Namespace UI.Controls
    Public Class CustomMessageBox
        Inherits Form

        Public Enum MessageType
            Information
            Success
            Warning
            [Error]
            Question
        End Enum

        Private _result As DialogResult = DialogResult.OK

        Public Sub New(message As String, title As String, msgType As MessageType, Optional isConfirmation As Boolean = False)
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.CenterParent
            BackColor = ThemeColors.CardBackground
            ForeColor = ThemeColors.TextPrimary
            Size = New Size(420, 210)
            ShowInTaskbar = False

            Dim pnlHeader As New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 44,
                .BackColor = ThemeColors.HeaderBackground
            }

            Dim lblTitle As New Label() With {
                .Text = title,
                .Font = New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold),
                .ForeColor = ThemeColors.TextPrimary,
                .Location = New Point(18, 12),
                .AutoSize = True
            }
            pnlHeader.Controls.Add(lblTitle)

            Dim pnlContent As New Panel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(20)
            }

            Dim iconColor As Color = ThemeColors.Primary
            Select Case msgType
                Case MessageType.Success
                    iconColor = ThemeColors.Success
                Case MessageType.Warning
                    iconColor = ThemeColors.Warning
                Case MessageType.Error
                    iconColor = ThemeColors.Danger
                Case MessageType.Question
                    iconColor = ThemeColors.Info
            End Select

            Dim lblMsg As New Label() With {
                .Text = message,
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = ThemeColors.TextPrimary,
                .Location = New Point(25, 20),
                .Size = New Size(370, 75)
            }
            pnlContent.Controls.Add(lblMsg)

            Dim pnlFooter As New Panel() With {
                .Dock = DockStyle.Bottom,
                .Height = 55,
                .BackColor = ThemeColors.HeaderBackground
            }

            If isConfirmation Then
                Dim btnCancel As New CustomButton() With {
                    .Text = "Cancel",
                    .ButtonStyle = CustomButton.ButtonStyleType.Secondary,
                    .Size = New Size(95, 34),
                    .Location = New Point(Width - 215, 10)
                }
                AddHandler btnCancel.Click, Sub()
                                                _result = DialogResult.Cancel
                                                Close()
                                            End Sub
                pnlFooter.Controls.Add(btnCancel)

                Dim btnYes As New CustomButton() With {
                    .Text = "Confirm",
                    .ButtonStyle = CustomButton.ButtonStyleType.Danger,
                    .Size = New Size(95, 34),
                    .Location = New Point(Width - 110, 10)
                }
                AddHandler btnYes.Click, Sub()
                                             _result = DialogResult.Yes
                                             Close()
                                         End Sub
                pnlFooter.Controls.Add(btnYes)
            Else
                Dim btnOk As New CustomButton() With {
                    .Text = "OK",
                    .ButtonStyle = CustomButton.ButtonStyleType.Primary,
                    .Size = New Size(95, 34),
                    .Location = New Point(Width - 115, 10)
                }
                AddHandler btnOk.Click, Sub()
                                            _result = DialogResult.OK
                                            Close()
                                        End Sub
                pnlFooter.Controls.Add(btnOk)
            End If

            Controls.Add(pnlContent)
            Controls.Add(pnlFooter)
            Controls.Add(pnlHeader)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Using p As New Pen(ThemeColors.CardBorder, 1.5F)
                e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1)
            End Using
        End Sub

        Public Shared Shadows Function Show(message As String, Optional title As String = "Message", Optional msgType As MessageType = MessageType.Information, Optional parent As Form = Nothing) As DialogResult
            Using box As New CustomMessageBox(message, title, msgType, False)
                Return box.ShowDialog(parent)
            End Using
        End Function

        Public Shared Function Confirm(message As String, Optional title As String = "Confirmation", Optional parent As Form = Nothing) As Boolean
            Using box As New CustomMessageBox(message, title, MessageType.Question, True)
                Return box.ShowDialog(parent) = DialogResult.Yes
            End Using
        End Function
    End Class
End Namespace
