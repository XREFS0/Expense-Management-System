Imports MasaExpenseManager.DataAccess
Imports MasaExpenseManager.Models

Namespace Business.Services
    Public Class DashboardService
        Private ReadOnly _expenseRepo As New ExpenseRepository()
        Private ReadOnly _incomeRepo As New IncomeRepository()
        Private ReadOnly _txRepo As New TransactionRepository()

        Public Function GetDashboardSummary() As DashboardSummary
            Dim summary As New DashboardSummary()
            Dim today As DateTime = DateTime.Today
            Dim startOfMonth As New DateTime(today.Year, today.Month, 1)
            Dim endOfMonth As DateTime = startOfMonth.AddMonths(1).AddDays(-1)

            summary.TotalExpenses = _expenseRepo.GetTotalExpenses()
            summary.TotalIncome = _incomeRepo.GetTotalIncome()
            summary.CurrentBalance = summary.TotalIncome - summary.TotalExpenses

            summary.MonthlyExpenseTotal = _expenseRepo.GetTotalExpenses(startOfMonth, endOfMonth)
            summary.MonthlyIncomeTotal = _incomeRepo.GetTotalIncome(startOfMonth, endOfMonth)

            Dim monthExpenses = _expenseRepo.GetAll(startDate:=startOfMonth, endDate:=endOfMonth)
            Dim monthIncome = _incomeRepo.GetAll(startDate:=startOfMonth, endDate:=endOfMonth)
            summary.ExpenseCountThisMonth = monthExpenses.Count
            summary.IncomeCountThisMonth = monthIncome.Count

            summary.CategoryBreakdown = _txRepo.GetCategoryBreakdown(startOfMonth, endOfMonth)
            If summary.CategoryBreakdown.Count > 0 Then
                summary.TopExpenseCategory = summary.CategoryBreakdown(0).CategoryName
            Else
                summary.TopExpenseCategory = "None"
            End If

            summary.MonthlyTrends = _txRepo.GetMonthlyTrends(6)
            summary.RecentTransactions = _txRepo.GetRecent(10)

            Return summary
        End Function
    End Class
End Namespace
