Imports System

Namespace Models
    Public Class User
        Public Property Id As Integer
        Public Property Username As String = String.Empty
        Public Property PasswordHash As String = String.Empty
        Public Property Salt As String = String.Empty
        Public Property FullName As String = String.Empty
        Public Property Email As String = String.Empty
        Public Property Role As UserRole = UserRole.User
        Public Property IsActive As Boolean = True
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property LastLogin As Nullable(Of DateTime)
    End Class

    Public Class Category
        Public Property Id As Integer
        Public Property Name As String = String.Empty
        Public Property Type As CategoryType = CategoryType.Expense
        Public Property ColorHex As String = "#3699FF"
        Public Property Icon As String = "tag"
        Public Property Description As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
    End Class

    Public Class Expense
        Public Property Id As Integer
        Public Property Title As String = String.Empty
        Public Property CategoryId As Integer
        Public Property CategoryName As String = String.Empty
        Public Property CategoryColor As String = "#3699FF"
        Public Property Amount As Decimal
        Public Property PaymentMethod As PaymentMethod = PaymentMethod.Cash
        Public Property ExpenseDate As DateTime = DateTime.Today
        Public Property Notes As String = String.Empty
        Public Property CreatedBy As Integer
        Public Property CreatorName As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property UpdatedAt As DateTime = DateTime.Now
    End Class

    Public Class Income
        Public Property Id As Integer
        Public Property Source As String = String.Empty
        Public Property CategoryId As Integer
        Public Property CategoryName As String = String.Empty
        Public Property Amount As Decimal
        Public Property IncomeDate As DateTime = DateTime.Today
        Public Property Notes As String = String.Empty
        Public Property CreatedBy As Integer
        Public Property CreatorName As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property UpdatedAt As DateTime = DateTime.Now
    End Class

    Public Class Transaction
        Public Property Id As Integer
        Public Property ReferenceId As Integer
        Public Property Type As TransactionType
        Public Property Title As String = String.Empty
        Public Property CategoryName As String = String.Empty
        Public Property Amount As Decimal
        Public Property TransactionDate As DateTime
        Public Property PaymentMethod As String = String.Empty
        Public Property Notes As String = String.Empty
        Public Property UserName As String = String.Empty
    End Class

    Public Class Setting
        Public Property Key As String = String.Empty
        Public Property Value As String = String.Empty
        Public Property Description As String = String.Empty
        Public Property UpdatedAt As DateTime = DateTime.Now
    End Class

    Public Class AuditLog
        Public Property Id As Integer
        Public Property UserId As Integer
        Public Property Username As String = String.Empty
        Public Property Action As AuditAction
        Public Property EntityName As String = String.Empty
        Public Property EntityId As Nullable(Of Integer)
        Public Property Details As String = String.Empty
        Public Property IpAddress As String = "127.0.0.1"
        Public Property Timestamp As DateTime = DateTime.Now
    End Class

    Public Class DashboardSummary
        Public Property TotalExpenses As Decimal
        Public Property TotalIncome As Decimal
        Public Property CurrentBalance As Decimal
        Public Property MonthlyExpenseTotal As Decimal
        Public Property MonthlyIncomeTotal As Decimal
        Public Property ExpenseCountThisMonth As Integer
        Public Property IncomeCountThisMonth As Integer
        Public Property TopExpenseCategory As String = "N/A"
        Public Property CategoryBreakdown As New List(Of CategoryBreakdownItem)()
        Public Property MonthlyTrends As New List(Of MonthlyTrendItem)()
        Public Property RecentTransactions As New List(Of Transaction)()
    End Class

    Public Class CategoryBreakdownItem
        Public Property CategoryName As String = String.Empty
        Public Property ColorHex As String = "#3699FF"
        Public Property TotalAmount As Decimal
        Public Property Percentage As Double
    End Class

    Public Class MonthlyTrendItem
        Public Property MonthName As String = String.Empty
        Public Property ExpenseAmount As Decimal
        Public Property IncomeAmount As Decimal
    End Class
End Namespace
