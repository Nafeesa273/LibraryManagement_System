
📚 Library Management System 
A Comprehensive Full-Stack Console Application
Developed as a robust solution for academic and public libraries, this system leverages C# and Relational Database Management Systems (RDBMS) to automate the entire lifecycle of book management, from inventory control to financial tracking of fines.

📑 Table of Contents
1.	Introduction
2.	Core Architecture
3.	Technical Features
4.	Database Design (Schema)
5.	Business Logic & Rules
6.	UI/UX Design Philosophy
7.	Installation & Setup
8.	Future Enhancements

📖 Introduction
The Advanced Library Management System is designed to solve the common issues of manual record-keeping. It provides a secure, multi-user environment where administrative tasks (like cataloging) and user-centric tasks (like book issuance) are handled seamlessly. The system ensures Data Integrity and Operational Efficiency.

🏗️ Core Architecture
The project follows a Layered Architecture to ensure clean code and maintainability:
•	Presentation Layer (Program.cs): Handles user interactions, input validation, and rendering of the CLI using Spectre.Console.
•	Data Access Layer (DataManager.cs): Manages all communication with the MySQL database using the MySql.Data driver.
•	Model Layer (Models/): Defines the data structures (User, Book, Transaction) that mirror the database tables.

🚀 Technical Features
🛡️ 1. Authentication & Role-Based Security
•	Multi-Role Access: Distinct menus and permissions for Admin and User.
•	Password Protection: Implements TextPrompt.Secret for secure password entry (input is hidden during typing).
•	Session Management: Keeps track of the loggedInUser throughout the application lifecycle.
📊 2. Inventory & CRUD Operations
•	Full Lifecycle Management: Admins can Create, Read, Update, and Delete book records.
•	Real-time Stock Tracking: The Quantity field automatically decrements on issuance and increments on return.
•	Availability Status: Dynamic visual cues (Green/Red) indicating book availability.
💰 3. Automated Transaction & Fine Engine
•	Audit Trail: Every book movement is recorded in the issued_books table with precise timestamps.
•	Dynamic Fine Calculation: A specialized algorithm calculates late fees based on the difference between issue_date and the current date.

🗄️ Database Design (Schema)
The system is powered by a relational schema consisting of three primary tables:
1.	users Table: Stores account details (id, username, password, role).
2.	books Table: Stores the catalog (id, book_name, author, quantity, price, category).
3.	issued_books Table: The transaction log linking users to books (id, username, book_id, issue_date, return_date, status).

⚖️ Business Logic & Rules
To maintain library discipline, the following rules are hardcoded into the system:
•	Return Policy: Books must be returned within 7 days.
•	Penalty Clause: A fine of Rs. 100 per day is automatically applied after the 7th day.
•	Issuance Limit: A book cannot be issued if the quantity in the database is 0.

🎨 UI/UX Design Philosophy
Unlike traditional "black and white" console apps, this system uses Spectre.Console to provide:
•	Responsive Tables: Data is organized in auto-scaling, colorful grids.
•	Progress Indicators: Uses Thread.Sleep and status bars for a realistic "loading" experience.
•	Color-Coded Feedback: Green for success, Red for errors, and Yellow for warnings/titles.

⚙️ Installation & Setup
Prerequisites
•	.NET 10.0 SDK
•	MySQL Server (Local or Cloud)
•	NuGet Packages: Spectre.Console, MySql.Data
Steps
1.	Clone the Repository.
2.	Configure DB: Run the provided SQL scripts to initialize tables.
3.	Connection String: Update the connString in DataManager.cs.
4.	Build & Run: ```bash
dotnet build
dotnet run

________________________________________
🛤️ Future Enhancements
The system is designed to be scalable. Future updates could include:
•	Barcode/QR Scanner Integration: For rapid book scanning.
•	Email Notifications: Automated alerts for late book returns.
•	GUI Transition: Migrating the core logic to a Web (ASP.NET Core) or Desktop (WPF) interface.
________________________________________
👩💻 Developed By
Nafeesa Haroon Computer Science Student | Semester 6

