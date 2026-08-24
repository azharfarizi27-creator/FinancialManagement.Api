# 💰 Dompetin — Financial Management API

RESTful API untuk aplikasi **manajemen keuangan pribadi** yang dibangun menggunakan **ASP.NET Core Web API**.

API ini menyediakan fitur untuk mengelola wallet/saldo, transaksi pemasukan dan pengeluaran, kategori transaksi, budget, serta dashboard keuangan. Sistem juga dilengkapi dengan **JWT Authentication** untuk mengamankan endpoint API.

---

## 📌 Project Overview

**Dompetin** merupakan backend API untuk aplikasi financial management yang memungkinkan pengguna untuk:

* Membuat akun
* Login menggunakan email dan password
* Menggunakan JWT Authentication
* Mengelola wallet
* Melihat saldo
* Membuat transaksi pemasukan
* Membuat transaksi pengeluaran
* Mengelola kategori transaksi
* Mengatur budget
* Melihat dashboard keuangan
* Melihat riwayat transaksi
* Menggunakan pagination pada data transaksi

Project ini dibuat sebagai implementasi backend menggunakan arsitektur yang terstruktur dengan pemisahan antara:

* Controller
* Service
* Repository
* Model
* DTO
* Database Context

---

# 🚀 Features

### 🔐 Authentication

* Register user
* Login user
* JWT Authentication
* Protected API menggunakan `[Authorize]`
* User identification menggunakan JWT Claims

### 👛 Wallet

* Melihat wallet user
* Melihat saldo
* Mengelola data wallet

### 💸 Transaction

* Membuat transaksi
* Melihat transaksi
* Melihat detail transaksi
* Menghapus transaksi
* Filter transaksi berdasarkan tipe
* Pagination transaksi

### 🏷️ Category

* Membuat kategori
* Melihat kategori
* Mengelola kategori transaksi

### 🎯 Budget

* Membuat budget
* Melihat budget
* Mengelola budget
* Monitoring budget

### 📊 Dashboard

* Total saldo
* Total pemasukan
* Total pengeluaran
* Ringkasan transaksi
* Informasi keuangan user

### 📖 API Documentation

* Swagger UI
* JWT Bearer Authentication pada Swagger
* Request & Response documentation

---

# 🛠️ Tech Stack

| Technology            | Description          |
| --------------------- | -------------------- |
| C#                    | Programming Language |
| .NET 8                | Backend Framework    |
| ASP.NET Core Web API  | REST API             |
| Entity Framework Core | ORM                  |
| SQL Server            | Database             |
| JWT                   | Authentication       |
| Swagger / OpenAPI     | API Documentation    |
| LINQ                  | Data Query           |
| Git                   | Version Control      |
| GitHub                | Repository           |

---

# 🏗️ Architecture

Project menggunakan pendekatan berlapis agar kode lebih mudah dikembangkan dan dipelihara.

```text
Client
   │
   ▼
Controller
   │
   ▼
Service
   │
   ▼
Repository
   │
   ▼
Entity Framework Core
   │
   ▼
SQL Server
```

### Controller

Bertanggung jawab menerima HTTP Request dan mengembalikan HTTP Response.

Contoh:

```text
AuthController
WalletController
TransactionController
CategoryController
BudgetController
DashboardController
```

### Service

Berisi business logic dari aplikasi.

Contoh:

```text
AuthService
WalletService
TransactionService
CategoryService
BudgetService
DashboardService
```

### Repository

Bertanggung jawab terhadap akses data menggunakan Entity Framework Core.

Contoh:

```text
TransactionRepository
WalletRepository
CategoryRepository
BudgetRepository
```

### Model

Merepresentasikan entity yang digunakan oleh database.

Contoh:

```text
User
Wallet
Transaction
Category
Budget
```

### DTO

DTO digunakan untuk mengatur data yang dikirim dan diterima oleh API tanpa langsung mengekspos entity database.

---

# 📁 Project Structure

Struktur project secara umum:

```text
FinancialManagement.Api/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── WalletController.cs
│   ├── TransactionController.cs
│   ├── CategoryController.cs
│   ├── BudgetController.cs
│   └── DashboardController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── DTOs/
│   ├── Auth/
│   ├── Wallet/
│   ├── Transaction/
│   ├── Category/
│   ├── Budget/
│   └── Dashboard/
│
├── Models/
│   ├── User.cs
│   ├── Wallet.cs
│   ├── Transaction.cs
│   ├── Category.cs
│   └── Budget.cs
│
├── Repositories/
│   ├── Interfaces/
│   │   ├── IWalletRepository.cs
│   │   ├── ITransactionRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   └── IBudgetRepository.cs
│   │
│   └── Impl/
│       ├── WalletRepository.cs
│       ├── TransactionRepository.cs
│       ├── CategoryRepository.cs
│       └── BudgetRepository.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IWalletService.cs
│   │   ├── ITransactionService.cs
│   │   ├── ICategoryService.cs
│   │   ├── IBudgetService.cs
│   │   └── IDashboardService.cs
│   │
│   └── Impl/
│       ├── AuthService.cs
│       ├── WalletService.cs
│       ├── TransactionService.cs
│       ├── CategoryService.cs
│       ├── BudgetService.cs
│       └── DashboardService.cs
│
├── Migrations/
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── FinancialManagement.Api.csproj
```

> Struktur folder dapat berkembang mengikuti kebutuhan project.

---

# 🗄️ Database

Project menggunakan:

```text
SQL Server
```

Entity utama yang digunakan:

```text
Users
Wallets
Transactions
Categories
Budgets
```

Relasi secara umum:

```text
User
 │
 ├── Wallet
 │
 ├── Transactions
 │       │
 │       └── Category
 │
 ├── Categories
 │
 └── Budgets
```

---

# ⚙️ Requirements

Sebelum menjalankan project, pastikan sudah terinstall:

* .NET 8 SDK
* SQL Server
* SQL Server Management Studio / Azure Data Studio
* Visual Studio 2022 atau VS Code
* Git
* Postman atau Swagger untuk testing API

Cek versi .NET:

```bash
dotnet --version
```

---

# 📥 Installation

Clone repository:

```bash
git clone https://github.com/azharfarizi27-creator/FinancialManagrment.Api.git
```

Masuk ke directory:

```bash
cd FinancialManagrment.Api
```

Restore dependency:

```bash
dotnet restore
```

Build project:

```bash
dotnet build
```

---

# 🔧 Database Configuration

Buka:

```text
appsettings.json
```

Kemudian konfigurasi connection string SQL Server.

Contoh:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FinancialManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Jika menggunakan SQL Server Authentication:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FinancialManagementDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

Sesuaikan konfigurasi dengan SQL Server yang digunakan.

---

# 🔐 JWT Configuration

JWT digunakan untuk mengamankan endpoint API.

Contoh konfigurasi:

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "FinancialManagement.Api",
    "Audience": "FinancialManagement.Client",
    "ExpiresInMinutes": 60
  }
}
```

> Jangan menggunakan secret key sederhana pada production.

Untuk production, gunakan environment variable atau secret management.

---

# 🗃️ Entity Framework Core

Setelah database configuration selesai, jalankan migration.

Jika migration sudah tersedia:

```bash
dotnet ef database update
```

Jika perlu membuat migration baru:

```bash
dotnet ef migrations add InitialCreate
```

Kemudian:

```bash
dotnet ef database update
```

---

# ▶️ Running the API

Jalankan project:

```bash
dotnet run
```

Atau menggunakan Visual Studio:

```text
F5
```

atau:

```text
Ctrl + F5
```

Setelah aplikasi berjalan, API dapat diakses melalui URL yang ditampilkan oleh ASP.NET Core.

Contoh:

```text
https://localhost:xxxx
```

---

# 📖 Swagger

Swagger digunakan untuk melihat dan melakukan testing terhadap endpoint API.

Buka:

```text
https://localhost:xxxx/swagger
```

Swagger menyediakan dokumentasi endpoint seperti:

```text
Authentication
Wallet
Transaction
Category
Budget
Dashboard
```

---

# 🔐 Authentication

API menggunakan:

```text
JWT Bearer Token
```

Endpoint yang membutuhkan authentication harus mengirimkan:

```http
Authorization: Bearer <TOKEN>
```

---

# 👤 Register

Endpoint:

```http
POST /api/Auth/register
```

Contoh request:

```json
{
  "name": "Azhar",
  "email": "azhar@example.com",
  "password": "Password123!"
}
```

Contoh response:

```json
{
  "message": "User registered successfully"
}
```

---

# 🔑 Login

Endpoint:

```http
POST /api/Auth/login
```

Contoh request:

```json
{
  "email": "azhar@example.com",
  "password": "Password123!"
}
```

Contoh response:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2026-08-21T15:00:00Z"
}
```

Token tersebut digunakan untuk mengakses endpoint yang membutuhkan authentication.

---

# 🛡️ Authorization

Pada Swagger:

1. Login melalui endpoint `/api/Auth/login`
2. Copy token dari response
3. Klik tombol **Authorize**
4. Masukkan:

```text
Bearer YOUR_TOKEN
```

5. Klik **Authorize**
6. Klik **Close**

Setelah itu Swagger akan otomatis mengirim token pada request endpoint yang membutuhkan authorization.

---

# 👛 Wallet API

Wallet digunakan untuk menyimpan informasi saldo user.

## Get Wallet

```http
GET /api/Wallet
```

Header:

```http
Authorization: Bearer <TOKEN>
```

Contoh response:

```json
{
  "id": 1,
  "balance": 5000000
}
```

---

# 💸 Transaction API

Transaction digunakan untuk menyimpan pemasukan dan pengeluaran.

Contoh tipe transaksi:

```text
Income
Expense
```

---

## Get Transactions

```http
GET /api/Transaction
```

Header:

```http
Authorization: Bearer <TOKEN>
```

Contoh response:

```json
[
  {
    "id": 1,
    "amount": 5000000,
    "type": "Income",
    "description": "Salary",
    "categoryId": 1
  },
  {
    "id": 2,
    "amount": 50000,
    "type": "Expense",
    "description": "Lunch",
    "categoryId": 2
  }
]
```

---

# 📄 Transaction Pagination

Transaction API mendukung pagination menggunakan:

```text
page
pageSize
```

Contoh:

```http
GET /api/Transaction?page=1&pageSize=2
```

Request halaman kedua:

```http
GET /api/Transaction?page=2&pageSize=2
```

Contoh response:

```json
{
  "data": [
    {
      "id": 1,
      "amount": 5000000,
      "type": "Income",
      "description": "Salary"
    },
    {
      "id": 2,
      "amount": 50000,
      "type": "Expense",
      "description": "Lunch"
    }
  ],
  "page": 1,
  "pageSize": 2,
  "totalItems": 10,
  "totalPages": 5
}
```

Pagination digunakan agar API tidak perlu mengambil seluruh data transaksi sekaligus.

---

# ➕ Create Transaction

Endpoint:

```http
POST /api/Transaction
```

Header:

```http
Authorization: Bearer <TOKEN>
Content-Type: application/json
```

Contoh transaksi pemasukan:

```json
{
  "amount": 5000000,
  "type": "Income",
  "description": "Salary",
  "categoryId": 1
}
```

Contoh transaksi pengeluaran:

```json
{
  "amount": 50000,
  "type": "Expense",
  "description": "Lunch",
  "categoryId": 2
}
```

---

# 🔎 Get Transaction By ID

Endpoint:

```http
GET /api/Transaction/{id}
```

Contoh:

```http
GET /api/Transaction/1
```

Response:

```json
{
  "id": 1,
  "amount": 5000000,
  "type": "Income",
  "description": "Salary",
  "categoryId": 1
}
```

---

# 🗑️ Delete Transaction

Endpoint:

```http
DELETE /api/Transaction/{id}
```

Contoh:

```http
DELETE /api/Transaction/1
```

Header:

```http
Authorization: Bearer <TOKEN>
```

---

# 🏷️ Category API

Category digunakan untuk mengelompokkan transaksi.

Contoh kategori:

```text
Food
Transport
Salary
Shopping
Entertainment
Bills
Health
Education
```

---

## Get Categories

```http
GET /api/Category
```

Header:

```http
Authorization: Bearer <TOKEN>
```

Contoh response:

```json
[
  {
    "id": 1,
    "name": "Salary"
  },
  {
    "id": 2,
    "name": "Food"
  },
  {
    "id": 3,
    "name": "Transport"
  }
]
```

---

## Create Category

```http
POST /api/Category
```

Request:

```json
{
  "name": "Entertainment"
}
```

Response:

```json
{
  "id": 4,
  "name": "Entertainment"
}
```

---

# 🎯 Budget API

Budget digunakan untuk menentukan batas pengeluaran user.

Contoh:

```text
Food       → Rp1.000.000
Transport  → Rp500.000
Shopping   → Rp750.000
```

---

## Get Budgets

```http
GET /api/Budget
```

Header:

```http
Authorization: Bearer <TOKEN>
```

Contoh response:

```json
[
  {
    "id": 1,
    "categoryId": 2,
    "amount": 1000000
  },
  {
    "id": 2,
    "categoryId": 3,
    "amount": 500000
  }
]
```

---

## Create Budget

```http
POST /api/Budget
```

Request:

```json
{
  "categoryId": 2,
  "amount": 1000000
}
```

---

# 📊 Dashboard API

Dashboard digunakan untuk menampilkan ringkasan kondisi keuangan user.

Endpoint:

```http
GET /api/Dashboard
```

Header:

```http
Authorization: Bearer <TOKEN>
```

Contoh response:

```json
{
  "balance": 4500000,
  "totalIncome": 5000000,
  "totalExpense": 500000,
  "transactionCount": 10
}
```

Dashboard mengambil data berdasarkan user yang sedang login.

User tidak perlu mengirimkan `userId` secara manual karena identitas user diperoleh dari JWT Claims.

---

# 🔄 Request Flow

Alur request pada API:

```text
Client
  │
  │ HTTP Request
  ▼
Controller
  │
  │ Validate Request
  ▼
Service
  │
  │ Business Logic
  ▼
Repository
  │
  │ EF Core
  ▼
SQL Server
  │
  │ Data
  ▼
Repository
  │
  ▼
Service
  │
  ▼
Controller
  │
  │ HTTP Response
  ▼
Client
```

---

# 🔐 JWT Authentication Flow

```text
User
 │
 │ Register
 ▼
Auth API
 │
 ▼
Database
 │
 └── User Created
     
User
 │
 │ Login
 ▼
Auth API
 │
 ▼
Validate Email & Password
 │
 ▼
Generate JWT
 │
 ▼
Return Token
 │
 ▼
Client
 │
 │ Authorization: Bearer Token
 ▼
Protected API
 │
 ▼
Validate JWT
 │
 ▼
Get UserId from Claims
 │
 ▼
Service
 │
 ▼
Database
```

---

# 👤 User Identification

Setiap request yang sudah ter-authenticate memiliki JWT Claims.

User ID dapat diperoleh dari:

```csharp
User.FindFirstValue(ClaimTypes.NameIdentifier);
```

Dengan pendekatan ini, endpoint seperti transaction, budget, wallet, dan dashboard dapat mengambil data berdasarkan user yang sedang login.

Contoh:

```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
```

Sehingga client tidak perlu mengirim:

```json
{
  "userId": 1
}
```

pada setiap request.

Hal ini juga membantu mencegah user mengakses data milik user lain.

---

# ❌ Error Response

API menggunakan HTTP Status Code untuk memberikan informasi mengenai hasil request.

Contoh:

### 400 Bad Request

Request tidak valid.

```json
{
  "message": "Invalid request"
}
```

### 401 Unauthorized

Token tidak tersedia atau token tidak valid.

```json
{
  "message": "Unauthorized"
}
```

### 403 Forbidden

User tidak memiliki permission untuk mengakses resource.

```json
{
  "message": "Forbidden"
}
```

### 404 Not Found

Data tidak ditemukan.

```json
{
  "message": "Data not found"
}
```

### 500 Internal Server Error

Terjadi kesalahan pada server.

```json
{
  "message": "Internal server error"
}
```

---

# 🧪 API Testing

API dapat dites menggunakan:

* Swagger
* Postman
* Insomnia

Urutan testing yang direkomendasikan:

```text
1. Register
      ↓
2. Login
      ↓
3. Copy JWT Token
      ↓
4. Authorize
      ↓
5. Create Category
      ↓
6. Create Wallet
      ↓
7. Create Transaction
      ↓
8. Get Transactions
      ↓
9. Test Pagination
      ↓
10. Create Budget
      ↓
11. Get Dashboard
```

---

# 🧪 Pagination Testing

Untuk melakukan testing pagination:

### Page 1

```http
GET /api/Transaction?page=1&pageSize=2
```

### Page 2

```http
GET /api/Transaction?page=2&pageSize=2
```

Jika terdapat 4 data:

```text
Page 1
├── Transaction 1
└── Transaction 2

Page 2
├── Transaction 3
└── Transaction 4
```

---

# 🔒 Security Considerations

Beberapa security practice yang diterapkan:

* JWT Authentication
* `[Authorize]` pada protected endpoint
* User ID diambil dari JWT Claims
* Tidak menerima user ID dari client untuk menentukan ownership
* Password tidak disimpan dalam bentuk plaintext
* Database access menggunakan Entity Framework Core
* JWT secret tidak sebaiknya disimpan langsung pada source code production

Untuk production, konfigurasi sensitive seperti JWT secret sebaiknya menggunakan:

```text
Environment Variables
```

atau secret manager.

---

# 🌱 Environment Configuration

Untuk development:

```text
appsettings.Development.json
```

Untuk production:

```text
Environment Variables
```

Contoh:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
```

---

# 📌 API Endpoint Summary

| Method | Endpoint                | Auth | Description             |
| ------ | ----------------------- | ---: | ----------------------- |
| POST   | `/api/Auth/register`    |    ❌ | Register user           |
| POST   | `/api/Auth/login`       |    ❌ | Login user              |
| GET    | `/api/Wallet`           |    ✅ | Get wallet              |
| GET    | `/api/Transaction`      |    ✅ | Get transactions        |
| GET    | `/api/Transaction/{id}` |    ✅ | Get transaction detail  |
| POST   | `/api/Transaction`      |    ✅ | Create transaction      |
| DELETE | `/api/Transaction/{id}` |    ✅ | Delete transaction      |
| GET    | `/api/Category`         |    ✅ | Get categories          |
| POST   | `/api/Category`         |    ✅ | Create category         |
| GET    | `/api/Budget`           |    ✅ | Get budgets             |
| POST   | `/api/Budget`           |    ✅ | Create budget           |
| GET    | `/api/Dashboard`        |    ✅ | Get financial dashboard |

> Endpoint dapat bertambah seiring pengembangan project.

---

# 📊 Example Financial Flow

Contoh penggunaan aplikasi:

```text
User Login
    │
    ▼
Wallet Balance
Rp5.000.000
    │
    ├── Income
    │     └── Salary
    │         + Rp5.000.000
    │
    ├── Expense
    │     ├── Food
    │     │    - Rp500.000
    │     │
    │     ├── Transport
    │     │    - Rp300.000
    │     │
    │     └── Shopping
    │          - Rp200.000
    │
    ▼
Dashboard
    │
    ├── Total Income
    ├── Total Expense
    ├── Current Balance
    └── Transaction Summary
```

---

# 🧩 Dependency Injection

Service dan repository didaftarkan melalui Dependency Injection pada `Program.cs`.

Contoh:

```csharp
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
```

Pendekatan ini membuat dependency lebih mudah dikelola dan memudahkan proses testing.

---

# 🗃️ Entity Framework Core

Database interaction menggunakan Entity Framework Core.

Contoh:

```csharp
private readonly AppDbContext _context;
```

Query dilakukan menggunakan LINQ:

```csharp
var transactions = await _context.Transactions
    .Where(x => x.UserId == userId)
    .ToListAsync();
```

---

# 📄 Pagination Implementation

Pagination transaction menggunakan:

```text
Skip()
Take()
```

Konsep:

```csharp
var transactions = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

Contoh:

```text
page = 1
pageSize = 2

Skip = (1 - 1) × 2
     = 0

Take = 2
```

Sehingga mengambil data:

```text
1, 2
```

Untuk:

```text
page = 2
pageSize = 2
```

Maka:

```text
Skip = (2 - 1) × 2
     = 2

Take = 2
```

Sehingga mengambil:

```text
3, 4
```

---

# 🧱 Design Pattern

Project menggunakan beberapa pattern:

### Repository Pattern

Memisahkan akses database dari business logic.

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

### Service Layer

Business logic ditempatkan pada service.

```text
Controller
    ↓
Service
```

Controller tidak menangani seluruh business logic secara langsung.

### Dependency Injection

Dependency diberikan oleh ASP.NET Core melalui DI container.

---

# 📈 Future Development

Beberapa fitur yang dapat dikembangkan selanjutnya:

* [ ] Refresh Token
* [ ] Forgot Password
* [ ] Email Verification
* [ ] Change Password
* [ ] Update Profile
* [ ] Update Wallet
* [ ] Update Transaction
* [ ] Advanced Transaction Filtering
* [ ] Transaction Search
* [ ] Transaction Sorting
* [ ] Budget Progress
* [ ] Monthly Financial Report
* [ ] Weekly Financial Report
* [ ] Financial Statistics
* [ ] Export Transaction to Excel
* [ ] Export Transaction to PDF
* [ ] Notification
* [ ] Recurring Transaction
* [ ] Multiple Wallet
* [ ] Multiple Currency
* [ ] Admin Role
* [ ] Unit Testing
* [ ] Integration Testing
* [ ] Docker
* [ ] CI/CD
* [ ] Production Deployment

---

# 🚀 Roadmap

Development roadmap:

```text
Phase 1
├── Project Setup
├── Database
├── Entity
└── Repository

Phase 2
├── Service Layer
├── Controller
└── CRUD API

Phase 3
├── Authentication
├── JWT
└── Authorization

Phase 4
├── Transaction
├── Pagination
├── Category
└── Budget

Phase 5
├── Dashboard
└── Financial Summary

Phase 6
├── Validation
├── Error Handling
└── API Documentation

Phase 7
├── Unit Testing
├── Integration Testing
└── API Optimization

Phase 8
├── Docker
├── CI/CD
└── Production Deployment
```

---

# 🧑‍💻 Author

**Azhar Farizi**

Backend Developer / .NET Developer

Interested in:

```text
C#
.NET
ASP.NET Core
REST API
SQL Server
Entity Framework Core
Backend Development
```

---

# 📜 License

This project is developed for learning, portfolio, and development purposes.

---

# ⭐ Project Status

```text
Status: Active Development
Version: 1.0
Framework: .NET 8
Database: SQL Server
Authentication: JWT Bearer
```

---

# 📞 API Development Notes

Project ini masih dalam tahap pengembangan dan beberapa endpoint dapat berubah mengikuti kebutuhan aplikasi.

Untuk dokumentasi API yang paling aktual selama development, gunakan:

```text
Swagger UI
```

Swagger menyediakan interface untuk:

* Melihat endpoint
* Melihat HTTP method
* Melihat parameter
* Melihat request body
* Melihat response
* Melakukan authentication
* Melakukan API testing

---

## 🎯 Final Architecture

Secara keseluruhan, architecture project:

```text
                    ┌─────────────────────┐
                    │       Client        │
                    │ Web / Mobile / App  │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │     Controller      │
                    ├─────────────────────┤
                    │ Auth                │
                    │ Wallet              │
                    │ Transaction         │
                    │ Category            │
                    │ Budget              │
                    │ Dashboard           │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │       Service      │
                    ├─────────────────────┤
                    │ AuthService         │
                    │ WalletService       │
                    │ TransactionService  │
                    │ CategoryService     │
                    │ BudgetService       │
                    │ DashboardService    │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │     Repository      │
                    ├─────────────────────┤
                    │ WalletRepository    │
                    │ TransactionRepo     │
                    │ CategoryRepository  │
                    │ BudgetRepository    │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │   Entity Framework  │
                    │        Core         │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │      SQL Server     │
                    ├─────────────────────┤
                    │ Users               │
                    │ Wallets             │
                    │ Transactions        │
                    │ Categories          │
                    │ Budgets             │
                    └─────────────────────┘
```

---

## 💡 Summary

**Dompetin — Financial Management API** adalah backend REST API berbasis **ASP.NET Core .NET 8** yang menyediakan sistem manajemen keuangan pribadi.

Project menerapkan:

```text
ASP.NET Core Web API
        +
Entity Framework Core
        +
SQL Server
        +
Repository Pattern
        +
Service Layer
        +
Dependency Injection
        +
JWT Authentication
        +
Swagger
        +
Pagination
```

Backend dirancang agar dapat digunakan oleh berbagai jenis client seperti:

```text
Web Application
Mobile Application
Desktop Application
React / Next.js Frontend
```

Dengan architecture yang terstruktur, project dapat dikembangkan lebih lanjut menjadi aplikasi financial management yang lebih lengkap.
