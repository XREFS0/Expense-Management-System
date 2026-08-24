Imports Microsoft.Data.Sqlite
Imports MasaExpenseManager.Business.Security

Namespace DataAccess
    Public Class DatabaseInitializer
        Public Shared Sub Initialize()
            Using conn As SqliteConnection = DatabaseContext.CreateConnection()
                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "
                        PRAGMA journal_mode = WAL;
                        PRAGMA foreign_keys = ON;

                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL,
                            Salt TEXT NOT NULL,
                            FullName TEXT NOT NULL,
                            Email TEXT NOT NULL,
                            Role INTEGER NOT NULL DEFAULT 3,
                            IsActive INTEGER NOT NULL DEFAULT 1,
                            CreatedAt TEXT NOT NULL,
                            LastLogin TEXT
                        );

                        CREATE TABLE IF NOT EXISTS Categories (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL UNIQUE,
                            Type INTEGER NOT NULL,
                            ColorHex TEXT NOT NULL DEFAULT '#3699FF',
                            Icon TEXT NOT NULL DEFAULT 'tag',
                            Description TEXT,
                            CreatedAt TEXT NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Expenses (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Title TEXT NOT NULL,
                            CategoryId INTEGER NOT NULL,
                            Amount DECIMAL(18, 2) NOT NULL,
                            PaymentMethod INTEGER NOT NULL DEFAULT 1,
                            ExpenseDate TEXT NOT NULL,
                            Notes TEXT,
                            CreatedBy INTEGER NOT NULL,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT,
                            FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE RESTRICT
                        );

                        CREATE TABLE IF NOT EXISTS Income (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Source TEXT NOT NULL,
                            CategoryId INTEGER NOT NULL,
                            Amount DECIMAL(18, 2) NOT NULL,
                            IncomeDate TEXT NOT NULL,
                            Notes TEXT,
                            CreatedBy INTEGER NOT NULL,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT,
                            FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE RESTRICT
                        );

                        CREATE TABLE IF NOT EXISTS Settings (
                            Key TEXT PRIMARY KEY,
                            Value TEXT NOT NULL,
                            Description TEXT,
                            UpdatedAt TEXT NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS AuditLogs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            UserId INTEGER NOT NULL,
                            Username TEXT NOT NULL,
                            Action INTEGER NOT NULL,
                            EntityName TEXT NOT NULL,
                            EntityId INTEGER,
                            Details TEXT,
                            IpAddress TEXT,
                            Timestamp TEXT NOT NULL
                        );

                        CREATE INDEX IF NOT EXISTS idx_expenses_date ON Expenses(ExpenseDate);
                        CREATE INDEX IF NOT EXISTS idx_income_date ON Income(IncomeDate);
                        CREATE INDEX IF NOT EXISTS idx_audit_timestamp ON AuditLogs(Timestamp);
                    "
                    cmd.ExecuteNonQuery()
                End Using

                SeedDefaultData(conn)
            End Using
        End Sub

        Private Shared Sub SeedDefaultData(conn As SqliteConnection)
            Dim userCount As Long = 0
            Using cmd As SqliteCommand = conn.CreateCommand()
                cmd.CommandText = "SELECT COUNT(*) FROM Users;"
                userCount = Convert.ToInt64(cmd.ExecuteScalar())
            End Using

            If userCount = 0 Then
                Dim adminSalt As String = PasswordHasher.GenerateSalt()
                Dim adminHash As String = PasswordHasher.HashPassword("admin123", adminSalt)

                Dim managerSalt As String = PasswordHasher.GenerateSalt()
                Dim managerHash As String = PasswordHasher.HashPassword("manager123", managerSalt)

                Dim accountantSalt As String = PasswordHasher.GenerateSalt()
                Dim accountantHash As String = PasswordHasher.HashPassword("user123", accountantSalt)

                Dim sampleUsers As (Username As String, Hash As String, Salt As String, FullName As String, Email As String, Role As Integer) () = {
                    ("admin", adminHash, adminSalt, "Ahmed Hassan", "ahmed.hassan@masa.eg", 1),
                    ("mahmoud.ali", managerHash, managerSalt, "Mahmoud Ali", "mahmoud.ali@masa.eg", 2),
                    ("tarek.ibrahim", accountantHash, accountantSalt, "Tarek Ibrahim", "tarek.ibrahim@masa.eg", 3),
                    ("sarah.elmasry", accountantHash, accountantSalt, "Sarah El-Masry", "sarah.elmasry@masa.eg", 3)
                }

                For Each u In sampleUsers
                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "
                            INSERT INTO Users (Username, PasswordHash, Salt, FullName, Email, Role, IsActive, CreatedAt)
                            VALUES (@username, @passwordHash, @salt, @fullName, @email, @role, 1, @createdAt);
                        "
                        cmd.Parameters.AddWithValue("@username", u.Username)
                        cmd.Parameters.AddWithValue("@passwordHash", u.Hash)
                        cmd.Parameters.AddWithValue("@salt", u.Salt)
                        cmd.Parameters.AddWithValue("@fullName", u.FullName)
                        cmd.Parameters.AddWithValue("@email", u.Email)
                        cmd.Parameters.AddWithValue("@role", u.Role)
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.ExecuteNonQuery()
                    End Using
                Next
            End If

            Dim categoryCount As Long = 0
            Using cmd As SqliteCommand = conn.CreateCommand()
                cmd.CommandText = "SELECT COUNT(*) FROM Categories;"
                categoryCount = Convert.ToInt64(cmd.ExecuteScalar())
            End Using

            If categoryCount = 0 Then
                Dim defaultCategories As (Name As String, Type As Integer, Color As String, Icon As String) () = {
                    ("Food & Catering", 1, "#F64E60", "cutlery"),
                    ("Electricity & Utilities", 1, "#FFA800", "flash"),
                    ("Transportation & Fuel", 1, "#3699FF", "car"),
                    ("Office & HQ Rent", 1, "#8950FC", "home"),
                    ("Staff Activities & Entertainment", 1, "#E83E8C", "gamepad"),
                    ("Medical & Health Insurance", 1, "#1BC5BD", "heartbeat"),
                    ("Software & Cloud Hosting", 1, "#0BB783", "desktop"),
                    ("Salaries & Staff Payroll", 2, "#1BC5BD", "money"),
                    ("Software Projects & Consulting", 2, "#3699FF", "laptop"),
                    ("Investments & Bank Dividends", 2, "#8950FC", "line-chart"),
                    ("Commercial Commissions & Other", 2, "#FFA800", "plus-circle")
                }

                For Each cat In defaultCategories
                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "INSERT INTO Categories (Name, Type, ColorHex, Icon, Description, CreatedAt) VALUES (@name, @type, @color, @icon, @desc, @createdAt);"
                        cmd.Parameters.AddWithValue("@name", cat.Name)
                        cmd.Parameters.AddWithValue("@type", cat.Type)
                        cmd.Parameters.AddWithValue("@color", cat.Color)
                        cmd.Parameters.AddWithValue("@icon", cat.Icon)
                        cmd.Parameters.AddWithValue("@desc", cat.Name & " category")
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.ExecuteNonQuery()
                    End Using
                Next
            End If

            Dim expenseCount As Long = 0
            Using cmd As SqliteCommand = conn.CreateCommand()
                cmd.CommandText = "SELECT COUNT(*) FROM Expenses;"
                expenseCount = Convert.ToInt64(cmd.ExecuteScalar())
            End Using

            If expenseCount = 0 Then
                Dim today As DateTime = DateTime.Today
                Dim sampleExpenses As (Title As String, CategoryId As Integer, Amount As Decimal, PayMethod As Integer, DaysAgo As Integer, User As Integer, Notes As String) () = {
                    ("WE Egypt High-Speed Fiber Internet", 2, 2850.0D, 4, 2, 1, "HQ Smart Village high-speed internet subscription"),
                    ("Business Dinner with Client Amr Mostafa", 1, 3600.0D, 2, 3, 2, "Zamalek restaurant meeting for new project deal"),
                    ("New Cairo Branch Office Monthly Rent", 4, 65000.0D, 4, 7, 1, "5th Settlement commercial office lease"),
                    ("Egypt Gas & South Cairo Electricity Bill", 2, 8450.0D, 4, 10, 1, "Office air conditioning and utilities invoice"),
                    ("AWS Cloud & Microsoft Azure Servers", 7, 24800.0D, 2, 14, 3, "Cloud infrastructure hosting for client ERP apps"),
                    ("Uber Rides for Client Support Visits (Mohamed Nagy)", 3, 1450.0D, 5, 18, 2, "Transportation for on-site technical installations"),
                    ("Fawry & Vodafone Cash Merchant Fees", 7, 3200.0D, 5, 21, 3, "Payment gateway monthly processing fees"),
                    ("Company Quarterly Team Event & Lunch", 5, 9500.0D, 1, 24, 1, "Team bonding lunch in Maadi for engineering staff"),
                    ("AXA Corporate Health Insurance Installment", 6, 18200.0D, 4, 28, 1, "Quarterly medical insurance premium for team"),
                    ("Dell Laptops & Office Ergonomic Chairs", 7, 48000.0D, 4, 35, 1, "Hardware purchase from Silicon Oasis Mall"),
                    ("Gasoline & Fleet Maintenance (Super 95)", 3, 4200.0D, 1, 45, 2, "Company logistics vehicle maintenance in Giza"),
                    ("Alexandria Branch Client Presentation Travel", 3, 5600.0D, 2, 60, 2, "Train tickets & hotel stay in Alexandria")
                }

                For Each exp In sampleExpenses
                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "
                            INSERT INTO Expenses (Title, CategoryId, Amount, PaymentMethod, ExpenseDate, Notes, CreatedBy, CreatedAt, UpdatedAt)
                            VALUES (@title, @catId, @amt, @pm, @ed, @notes, @cb, @ca, @ua);
                        "
                        cmd.Parameters.AddWithValue("@title", exp.Title)
                        cmd.Parameters.AddWithValue("@catId", exp.CategoryId)
                        cmd.Parameters.AddWithValue("@amt", exp.Amount)
                        cmd.Parameters.AddWithValue("@pm", exp.PayMethod)
                        cmd.Parameters.AddWithValue("@ed", today.AddDays(-exp.DaysAgo).ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("@notes", exp.Notes)
                        cmd.Parameters.AddWithValue("@cb", exp.User)
                        cmd.Parameters.AddWithValue("@ca", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("@ua", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.ExecuteNonQuery()
                    End Using
                Next
            End If

            Dim incomeCount As Long = 0
            Using cmd As SqliteCommand = conn.CreateCommand()
                cmd.CommandText = "SELECT COUNT(*) FROM Income;"
                incomeCount = Convert.ToInt64(cmd.ExecuteScalar())
            End Using

            If incomeCount = 0 Then
                Dim today As DateTime = DateTime.Today
                Dim sampleIncome As (Source As String, CategoryId As Integer, Amount As Decimal, DaysAgo As Integer, User As Integer, Notes As String) () = {
                    ("ERP System Implementation - El-Araby Group Contract", 9, 185000.0D, 3, 1, "Milestone #2 delivered and approved via bank transfer"),
                    ("Monthly Executive Management Salary", 8, 75000.0D, 5, 1, "CIB bank payroll deposit for Ahmed Hassan"),
                    ("E-Commerce Web Portal Delivery - Al-Mansour Trading", 9, 95000.0D, 12, 2, "Full project signoff and final payment release"),
                    ("National Bank of Egypt (NBE) Deposit Certificates Yield", 10, 28500.0D, 18, 1, "Monthly return on 23.5% Egyptian Pound savings certificate"),
                    ("Mobile App Maintenance - Cairo Logistics Co.", 9, 32000.0D, 25, 3, "Quarterly SLA maintenance retainer invoice"),
                    ("Previous Month Executive Salary", 8, 75000.0D, 35, 1, "CIB bank payroll direct deposit"),
                    ("Software Architecture Consultation - Hassan Allam Holding", 9, 60000.0D, 50, 1, "Cloud migration technical consultancy fee"),
                    ("Commercial Real Estate Rental Return in Sheikh Zayed", 11, 45000.0D, 65, 1, "Quarterly commercial space lease income")
                }

                For Each inc In sampleIncome
                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "
                            INSERT INTO Income (Source, CategoryId, Amount, IncomeDate, Notes, CreatedBy, CreatedAt, UpdatedAt)
                            VALUES (@source, @catId, @amt, @idate, @notes, @cb, @ca, @ua);
                        "
                        cmd.Parameters.AddWithValue("@source", inc.Source)
                        cmd.Parameters.AddWithValue("@catId", inc.CategoryId)
                        cmd.Parameters.AddWithValue("@amt", inc.Amount)
                        cmd.Parameters.AddWithValue("@idate", today.AddDays(-inc.DaysAgo).ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("@notes", inc.Notes)
                        cmd.Parameters.AddWithValue("@cb", inc.User)
                        cmd.Parameters.AddWithValue("@ca", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("@ua", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.ExecuteNonQuery()
                    End Using
                Next
            End If

            Dim settingsCount As Long = 0
            Using cmd As SqliteCommand = conn.CreateCommand()
                cmd.CommandText = "SELECT COUNT(*) FROM Settings;"
                settingsCount = Convert.ToInt64(cmd.ExecuteScalar())
            End Using

            If settingsCount = 0 Then
                Dim defaultSettings As (Key As String, Value As String, Description As String) () = {
                    ("CurrencySymbol", "EGP", "Active currency symbol"),
                    ("CurrencyCode", "EGP", "Active currency code"),
                    ("CompanyName", "MASA Solutions Egypt", "Company or organization name"),
                    ("DateFormat", "yyyy-MM-dd", "System date format"),
                    ("AutoBackupOnExit", "false", "Backup database automatically on exit"),
                    ("BackupFolder", "", "Default backup destination folder")
                }

                For Each s In defaultSettings
                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "INSERT INTO Settings (Key, Value, Description, UpdatedAt) VALUES (@key, @val, @desc, @updatedAt);"
                        cmd.Parameters.AddWithValue("@key", s.Key)
                        cmd.Parameters.AddWithValue("@val", s.Value)
                        cmd.Parameters.AddWithValue("@desc", s.Description)
                        cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.ExecuteNonQuery()
                    End Using
                Next
            End If
        End Sub
    End Class
End Namespace
