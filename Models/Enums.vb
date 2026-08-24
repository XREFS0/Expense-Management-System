Namespace Models
    Public Enum UserRole
        Admin = 1
        Manager = 2
        User = 3
    End Enum

    Public Enum PaymentMethod
        Cash = 1
        CreditCard = 2
        DebitCard = 3
        BankTransfer = 4
        EWallet = 5
        Other = 6
    End Enum

    Public Enum CategoryType
        Expense = 1
        Income = 2
    End Enum

    Public Enum TransactionType
        Expense = 1
        Income = 2
    End Enum

    Public Enum AuditAction
        Create = 1
        Update = 2
        Delete = 3
        Login = 4
        Logout = 5
        Backup = 6
        Restore = 7
        Export = 8
    End Enum
End Namespace
