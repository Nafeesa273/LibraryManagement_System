

# 📚 Library Management System 

### **A Comprehensive Full-Stack Console Application**
Developed as a robust solution for academic and public libraries, this system leverages **C#** and **Relational Database Management Systems (RDBMS)** to automate the entire lifecycle of book management, from inventory control to automated fine tracking.

---

## 📑 Table of Contents
1. [Introduction](#-introduction)
2. [Core Architecture](#-core-architecture)
3. [Project Flow (Admin vs User)](#-project-flow)
4. [Technical Features](#-technical-features)
5. [Database Design](#-database-design-schema)
6. [Business Logic & Rules](#-business-logic--rules)
7. [UI/UX Design Philosophy](#-uiux-design-philosophy)

---

## 📖 Introduction
The **Advanced Library Management System** is designed to eliminate the inefficiencies of manual record-keeping. It provides a secure, multi-user environment where administrative oversight and user self-service tasks are handled seamlessly, ensuring data integrity and operational speed.

---

## 🏗️ Core Architecture
The project follows a **Layered Architecture** to ensure clean code and high maintainability:
*   **Presentation Layer (`Program.cs`):** Manages user interaction and UI rendering using **Spectre.Console**.
*   **Data Access Layer (`DataManager.cs`):** Handles all CRUD operations and communication with the **MySQL database**.
*   **Model Layer (`Models/`):** Defines core data structures such as Users, Books, and Transactions.

---

## 🚀 Project Flow

### **1. Admin Portal (Management & Control)**
The Admin holds full authority over the system to ensure smooth library operations:
*   **Inventory Management:** Permissions to add new arrivals, update existing book details, and perform soft deletes.
*   **Restore Center:** A specialized area to restore accidentally deleted books or perform permanent hard deletes.
*   **Monitoring:** Access to system activity logs and a complete history of all book transactions.
*   **Dashboard:** Real-time visualization of total book stock and registered user counts.

### **2. User Portal (Library Access)**
The User interface is optimized for simplicity and ease of access:
*   **Book Discovery:** Ability to browse the full catalog or search for specific titles and authors.
*   **Self Service:** Users can independently issue available books and return them through their personal portal.

---

## ⚡ Technical Features

### 🛡️ 1. Authentication & Role-Based Security
*   **Multi-Role Access:** Segregated menus and permissions for **Admin** and **User** roles.
*   **Secure Entry:** Password protection with hidden input fields during the login process.

### 📊 2. Inventory & CRUD Operations
*   **Full Lifecycle Management:** Robust Create, Read, Update, and Delete capabilities for book records.
*   **Real-time Stock Tracking:** Automated inventory adjustment where quantities decrement on issuance and increment on return.

### 💰 3. Automated Transaction & Fine Engine
* **Audit Trail:** Every movement is recorded in the `issued_books` table.
* **Dynamic Fine Calculation:** Late fees are calculated at **Rs. 100 per day** after the 7th day.

---

## 🗄️ Database Design (Schema)
The backend is powered by a relational structure consisting of three primary tables:
1.  **`users` Table:** Stores account credentials and permission levels (`id`, `username`, `password`, `role`).
2.  **`books` Table:** Maintains the central catalog (`id`, `book_name`, `author`, `quantity`, `price`).
3.  **`issued_books` Table:** Tracks the relationship between users and the books they have borrowed.

---

## ⚖️ Business Logic & Rules
* **Return Policy:** Books must be returned within **7 days**.
* **Penalty Clause:** A fine of **Rs. 100 per day** is applied after the 7th day.
* **Issuance Limit:** Cannot issue books if quantity is **0**.
---

## 🎨 UI/UX Design Philosophy
*   **Responsive Tables:** Utilization of colorful, structured grids for high data readability.
*   **Aesthetic Presentation:** Clean and professional dashboards that provide a modern CLI experience.
*   **Color-Coded Feedback:** Contextual alerts using Green (Success), Red (Error), and Yellow (Warnings).

---

## 👩‍💻 Developed By
**Nafeesa Haroon**  
*Computer Science Student | BSCS-VI (KICSIT)*
