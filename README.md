FinanceTracker API
A robust, production-ready Manual Ledger Backend built with ASP.NET Core 10. This engine is designed for high-integrity personal finance management, featuring smart transaction handling, automated balance adjustments, and a multi-layered security system.

Key Technical Features
1. Smart Transaction Engine
Unlike simple CRUD apps, this system manages real-time financial integrity:

Balance Rollbacks: Automatically restores account balances when transactions are deleted or edited.

Manual Reconciliation: A dedicated workflow to synchronize the digital ledger with physical cash/bank balances.

Transfer Logic: Atomic operations for moving funds between accounts (e.g., Bank to Cash).

2. Manual Bill & Subscription Management
Pay/Skip Logic: Built-in support for recurring commitments where users can manually mark a bill as "Paid" (creating a transaction) or "Skipped" (updating the next due date without spending).

Overdue Tracking: Real-time calculation of days until due.

3. Intelligence & Reporting
Monthly Trends: Aggregated data for income vs. expenses.

Category Analytics: Deep dive into spending habits by category.

Advanced Search: Keyword and date-range filtering for audit trails.

CSV Export: Standardized data export for external backups.

4. Security & Privacy
Hashed Passwords: Implemented using BCrypt.

App-Level PIN: Supports 4-6 digit hashed PINs for quick, secure mobile access (Flutter-ready).

JWT Authentication: Secure stateless session management.

🛠 Tech Stack
Runtime: .NET 10 (C#)

Framework: ASP.NET Core Web API

Database: SQLite (Code-First)

ORM: Entity Framework Core

Security: JWT, BCrypt

Testing: Postman

📂 Project Structure
Plaintext
THEPIAPI/
├── postman/        # Consolidated Collection and Environment JSON
├── sqllight/       # Database Schema (init_database.sql)
└── thepiapi/       # Source Code (Controllers, Models, Data)
⚙️ Setup & Installation
Clone the Repository:

Bash
git clone https://github.com/your-username/FinanceTracker.git
Initialize Database:

Navigate to the thepiapi folder.

Run migrations: dotnet ef database update

(Alternatively, use the script in /sqllight to create the schema manually).

Run the API:

Bash
dotnet run
Test with Postman:

Import the files from the /postman folder.

Use the base_url variable to point to your local port.

📝 Contact
Wasim Younes – Full Stack Developer
Damascus 🇸🇾 ➡️ Minsk 🇧🇾

## 📝 Contact
**Wasim Younes** – Full Stack Developer  
📍 Currently based in: **Minsk, Belarus** 🇧🇾 (Relocated from Damascus 🇸🇾)  
📧 [wasimyounes33@gmail.com]  
🔗 [www.linkedin.com/in/wasim-younes-a80913350]