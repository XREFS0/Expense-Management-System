Imports System.Windows.Forms
Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.UI.Forms
Imports MasaExpenseManager.UI.Controls

Namespace Global
    Friend Module Program
        <STAThread>
        Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            AddHandler Application.ThreadException, Sub(s, e)
                                                        CustomMessageBox.Show($"An unexpected application error occurred: {e.Exception.Message}", "System Error", CustomMessageBox.MessageType.Error)
                                                    End Sub

            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))

            Try
                DatabaseInitializer.Initialize()
            Catch ex As Exception
                MessageBox.Show($"Failed to initialize database: {ex.Message}", "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End Try

            Using login As New LoginForm()
                If login.ShowDialog() = DialogResult.OK Then
                    Application.Run(New MainForm())
                End If
            End Using
        End Sub
    End Module
End Namespace
