Imports System.IO
Imports System.Text
Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models

Namespace Business.Services
    Public Class ReportService
        Private ReadOnly _expenseRepo As New ExpenseRepository()
        Private ReadOnly _incomeRepo As New IncomeRepository()
        Private ReadOnly _txRepo As New TransactionRepository()
        Private ReadOnly _categoryRepo As New CategoryRepository()

        Public Function GetReportData(startDate As DateTime, endDate As DateTime, Optional categoryId As Nullable(Of Integer) = Nothing) As ReportDataResult
            Dim result As New ReportDataResult()
            result.StartDate = startDate
            result.EndDate = endDate

            result.Expenses = _expenseRepo.GetAll(categoryId, startDate, endDate)
            result.Income = _incomeRepo.GetAll(categoryId, startDate, endDate)

            result.TotalExpenses = result.Expenses.Sum(Function(e) e.Amount)
            result.TotalIncome = result.Income.Sum(Function(i) i.Amount)
            result.NetBalance = result.TotalIncome - result.TotalExpenses

            result.CategoryBreakdown = _txRepo.GetCategoryBreakdown(startDate, endDate)

            Return result
        End Function

        Public Function ExportToCsv(data As ReportDataResult, filePath As String) As Boolean
            Try
                Dim sb As New StringBuilder()
                sb.AppendLine("MASA EXPENSE MANAGER - FINANCIAL REPORT")
                sb.AppendLine($"Period,{data.StartDate:yyyy-MM-dd} to {data.EndDate:yyyy-MM-dd}")
                sb.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                sb.AppendLine()

                sb.AppendLine("SUMMARY")
                sb.AppendLine($"Total Income,{data.TotalIncome:F2}")
                sb.AppendLine($"Total Expenses,{data.TotalExpenses:F2}")
                sb.AppendLine($"Net Balance,{data.NetBalance:F2}")
                sb.AppendLine()

                sb.AppendLine("EXPENSES")
                sb.AppendLine("ID,Date,Title,Category,Amount,Payment Method,Created By,Notes")
                For Each exp In data.Expenses
                    sb.AppendLine($"{exp.Id},{exp.ExpenseDate:yyyy-MM-dd},""{EscapeCsv(exp.Title)}"",""{EscapeCsv(exp.CategoryName)}"",{exp.Amount:F2},{exp.PaymentMethod},""{EscapeCsv(exp.CreatorName)}"",""{EscapeCsv(exp.Notes)}""")
                Next
                sb.AppendLine()

                sb.AppendLine("INCOME")
                sb.AppendLine("ID,Date,Source,Category,Amount,Created By,Notes")
                For Each inc In data.Income
                    sb.AppendLine($"{inc.Id},{inc.IncomeDate:yyyy-MM-dd},""{EscapeCsv(inc.Source)}"",""{EscapeCsv(inc.CategoryName)}"",{inc.Amount:F2},""{EscapeCsv(inc.CreatorName)}"",""{EscapeCsv(inc.Notes)}""")
                Next
                sb.AppendLine()

                sb.AppendLine("CATEGORY BREAKDOWN")
                sb.AppendLine("Category,Total Amount,Percentage")
                For Each cat In data.CategoryBreakdown
                    sb.AppendLine($"""{EscapeCsv(cat.CategoryName)}"",{cat.TotalAmount:F2},{cat.Percentage}%")
                Next

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8)
                Return True
            Catch
                Return False
            End Try
        End Function

        Public Function ExportToHtmlReport(data As ReportDataResult, filePath As String) As Boolean
            Try
                Dim sb As New StringBuilder()
                sb.AppendLine("<!DOCTYPE html>")
                sb.AppendLine("<html><head><meta charset='utf-8'><title>Financial Report</title>")
                sb.AppendLine("<style>")
                sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 30px; background-color: #f8f9fa; color: #212529; }")
                sb.AppendLine(".header { background-color: #1E1E2D; color: #fff; padding: 25px; border-radius: 8px; margin-bottom: 20px; }")
                sb.AppendLine(".header h1 { margin: 0; font-size: 24px; color: #3699FF; }")
                sb.AppendLine(".header p { margin: 5px 0 0; color: #92929F; }")
                sb.AppendLine(".kpi-container { display: flex; gap: 15px; margin-bottom: 25px; }")
                sb.AppendLine(".kpi-card { flex: 1; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); border-left: 4px solid #3699FF; }")
                sb.AppendLine(".kpi-title { font-size: 13px; color: #7E8299; text-transform: uppercase; font-weight: 600; }")
                sb.AppendLine(".kpi-value { font-size: 22px; font-weight: bold; margin-top: 5px; }")
                sb.AppendLine("table { width: 100%; border-collapse: collapse; background: #fff; border-radius: 8px; overflow: hidden; margin-bottom: 25px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }")
                sb.AppendLine("th, td { padding: 12px 15px; text-align: left; border-bottom: 1px solid #EBEDF3; font-size: 13px; }")
                sb.AppendLine("th { background-color: #F3F6F9; color: #3F4254; font-weight: 600; text-transform: uppercase; font-size: 12px; }")
                sb.AppendLine(".text-right { text-align: right; }")
                sb.AppendLine(".badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; color: #fff; }")
                sb.AppendLine(".section-title { font-size: 16px; font-weight: 600; margin: 20px 0 10px; color: #181C32; }")
                sb.AppendLine("@media print { body { background: #fff; margin: 0; } .header { background: #1E1E2D !important; -webkit-print-color-adjust: exact; } }")
                sb.AppendLine("</style></head><body>")

                sb.AppendLine("<div class='header'>")
                sb.AppendLine("<h1>MASA Expense Manager</h1>")
                sb.AppendLine($"<p>Financial Statement Period: <b>{data.StartDate:yyyy-MM-dd}</b> to <b>{data.EndDate:yyyy-MM-dd}</b> | Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>")
                sb.AppendLine("</div>")

                sb.AppendLine("<div class='kpi-container'>")
                sb.AppendLine($"<div class='kpi-card' style='border-left-color: #1BC5BD;'><div class='kpi-title'>Total Income</div><div class='kpi-value' style='color: #1BC5BD;'>{data.TotalIncome:N2} EGP</div></div>")
                sb.AppendLine($"<div class='kpi-card' style='border-left-color: #F64E60;'><div class='kpi-title'>Total Expenses</div><div class='kpi-value' style='color: #F64E60;'>{data.TotalExpenses:N2} EGP</div></div>")
                sb.AppendLine($"<div class='kpi-card' style='border-left-color: #3699FF;'><div class='kpi-title'>Net Balance</div><div class='kpi-value' style='color: #3699FF;'>{data.NetBalance:N2} EGP</div></div>")
                sb.AppendLine("</div>")

                sb.AppendLine("<div class='section-title'>Expense Items</div>")
                sb.AppendLine("<table><thead><tr><th>Date</th><th>Title</th><th>Category</th><th>Payment</th><th class='text-right'>Amount (EGP)</th></tr></thead><tbody>")
                For Each exp In data.Expenses
                    sb.AppendLine($"<tr><td>{exp.ExpenseDate:yyyy-MM-dd}</td><td>{exp.Title}</td><td><span class='badge' style='background-color: {exp.CategoryColor};'>{exp.CategoryName}</span></td><td>{exp.PaymentMethod}</td><td class='text-right' style='color: #F64E60; font-weight: 600;'>{exp.Amount:N2} EGP</td></tr>")
                Next
                sb.AppendLine("</tbody></table>")

                sb.AppendLine("<div class='section-title'>Income Items</div>")
                sb.AppendLine("<table><thead><tr><th>Date</th><th>Source</th><th>Category</th><th class='text-right'>Amount (EGP)</th></tr></thead><tbody>")
                For Each inc In data.Income
                    sb.AppendLine($"<tr><td>{inc.IncomeDate:yyyy-MM-dd}</td><td>{inc.Source}</td><td>{inc.CategoryName}</td><td class='text-right' style='color: #1BC5BD; font-weight: 600;'>{inc.Amount:N2} EGP</td></tr>")
                Next
                sb.AppendLine("</tbody></table>")

                sb.AppendLine("</body></html>")

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8)
                Return True
            Catch
                Return False
            End Try
        End Function

        Private Function EscapeCsv(str As String) As String
            If String.IsNullOrEmpty(str) Then Return String.Empty
            Return str.Replace("""", """""")
        End Function
    End Class

    Public Class ReportDataResult
        Public Property StartDate As DateTime
        Public Property EndDate As DateTime
        Public Property TotalExpenses As Decimal
        Public Property TotalIncome As Decimal
        Public Property NetBalance As Decimal
        Public Property Expenses As New List(Of Expense)()
        Public Property Income As New List(Of Income)()
        Public Property CategoryBreakdown As New List(Of CategoryBreakdownItem)()
    End Class
End Namespace
