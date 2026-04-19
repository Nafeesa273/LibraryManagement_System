# 📚 Advanced Library Management System 

### **A Comprehensive Full-Stack Console Application**
Developed as a robust solution for academic and public libraries, this system leverages **C#** and **Relational Database Management Systems (RDBMS)** to automate the entire lifecycle of book management, from inventory control to financial tracking of fines.

---

## 📑 Table of Contents
1. [Introduction](#-introduction)
2. [Core Architecture](#-core-architecture)
3. [Technical Features](#-technical-features)
4. [Database Design](#-database-design-schema)
5. [Business Logic & Rules](#-business-logic--rules)
6. [UI/UX Design Philosophy](#-uiux-design-philosophy)
7. [Installation & Setup](#-installation--setup)
8. [Future Enhancements](#-future-enhancements)

---

## 📖 Introduction
The **Advanced Library Management System** is designed to solve the common issues of manual record-keeping. It provides a secure, multi-user environment where administrative tasks (like cataloging) and user-centric tasks (like book issuance) are handled seamlessly. The system ensures **Data Integrity** and **Operational Efficiency**.

---

## 🏗️ Core Architecture
The project follows a **Layered Architecture** to ensure clean code and maintainability:
* **Presentation Layer (`Program.cs`):** Handles user interactions and rendering of the CLI using **Spectre.Console**.
* **Data Access Layer (`DataManager.cs`):** Manages all communication with the **MySQL database**.
* **Model Layer (`Models/`):** Defines the data structures (User, Book, Transaction).

---

## 🚀 Technical Features

### 🛡️ 1. Authentication & Role-Based Security
* **Multi-Role Access:** Distinct menus and permissions for **Admin** and **User**.
* **Password Protection:** Secure password entry (hidden during typing).
* **Session Management:** Keeps track of the `loggedInUser` throughout the app.

### 📊 2. Inventory & CRUD Operations
* **Full Lifecycle Management:** Admins can Create, Read, Update, and Delete book records.
* **Real-time Stock Tracking:** Quantity auto-decrements on issuance and increments on return.
* **Visual Cues:** Dynamic Green/Red status for availability.

### 💰 3. Automated Transaction & Fine Engine
* **Audit Trail:** Every movement is recorded in the `issued_books` table.
* **Dynamic Fine Calculation:** Late fees are calculated at **Rs. 100 per day** after the 7th day.

---

## 🗄️ Database Design (Schema)
The system is powered by three primary tables:
1.  **`users` Table:** Stores account details (`id`, `username`, `password`, `role`).
2.  **`books` Table:** Stores the catalog (`id`, `book_name`, `author`, `quantity`, `price`).
3.  **`issued_books` Table:** Transaction log linking users to books.

---

## ⚖️ Business Logic & Rules
* **Return Policy:** Books must be returned within **7 days**.
* **Penalty Clause:** A fine of **Rs. 100 per day** is applied after the 7th day.
* **Issuance Limit:** Cannot issue books if quantity is **0**.

---

## 🎨 UI/UX Design Philosophy
* **Responsive Tables:** Colorful grids for better data readability.
* **Progress Indicators:** Realistic "loading" experience using status bars.
* **Color-Coded Feedback:** Green (Success), Red (Error), Yellow (Warnings).

---

## ⚙️ Installation & Setup

### **Prerequisites**
* .NET SDK
* MySQL Server
* NuGet Packages: `Spectre.Console`, `MySql.Data`

### **Steps**
1. Clone the Repository.
2. Run SQL scripts to initialize tables.
3. Update `connString` in `DataManager.cs`.
4. Execute:
   ```bash
   dotnet build
   dotnet run

   ---

## 👩‍💻 Developed By
**Nafeesa Haroon** *Computer Science Student | Semester 6*
