using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Spectre.Console;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Data;

// --- Global Variables ---
List<User> userList = new List<User>();
User? loggedInUser = null;

while (true)
{
    AnsiConsole.Clear();

    // DECORATIVE HEADER
    var header = new Panel(new Text("LIBRARY MANAGEMENT SYSTEM", new Style(Color.Yellow)).Centered());
    header.Header = new PanelHeader("LMS | DIGITAL EDITION");
    header.Border = BoxBorder.Double;
    header.BorderColor(Color.DeepSkyBlue1);
    header.Expand = true;
    AnsiConsole.Write(header);

    if (loggedInUser == null)
    {
        var choice = AnsiConsole.Prompt(
          new SelectionPrompt<string>()
          .Title("Please [bold cyan]Login[/] to access.")
          .AddChoices("Login", "Register", "About System", "Exit"));

        if (choice == "Login")
        {
            userList = DataManager.LoadUsers(); // Refresh users from DB
            var name = AnsiConsole.Ask<string>("Username:");
            var pass = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

            loggedInUser = userList.FirstOrDefault(u => u.Username.Equals(name, StringComparison.OrdinalIgnoreCase) && u.Password == pass);

            if (loggedInUser != null)
            {
                AnsiConsole.MarkupLine("[bold green]✔ Access Granted![/]");
                DataManager.SaveLog("LOGIN", $"User {name} logged into the system.");
                Thread.Sleep(1000);
            }
            else
            {
                AnsiConsole.MarkupLine("[bold red]❌ Invalid Login![/]");
                Thread.Sleep(2000);
            }
        }
        else if (choice == "Register")
        {
            var name = AnsiConsole.Ask<string>("New Username:");
            var pass = AnsiConsole.Prompt(new TextPrompt<string>("New Password:").Secret());
            var role = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Role:").AddChoices("Admin", "User"));

            DataManager.RegisterUser(new User { Username = name, Password = pass, Role = role });
            DataManager.SaveLog("REGISTRATION", $"New user '{name}' registered as {role}.");

            AnsiConsole.MarkupLine("[bold green]✔ Registered Successfully! You can now login.[/]");
            Thread.Sleep(1500);
        }
        else if (choice == "About System")
        {
            AnsiConsole.Write(new FigletText("LMS").Color(Color.Cyan));
            AnsiConsole.MarkupLine("[cyan]Developed by Nafeesa Haroon\nKICSIT - BSCS 5th Semester[/]");
            AnsiConsole.MarkupLine("\n[grey]Press any key to go back...[/]");
            Console.ReadKey();
        }
        else break;
    }
    else
    {
        // USER INFO BAR
        AnsiConsole.Write(new Panel(new Markup($"[bold white]USER:[/] [cyan]{loggedInUser.Username.ToUpper()}[/] | [bold white]ROLE:[/] [yellow]{loggedInUser.Role}[/]")).BorderColor(Color.Grey));

        if (loggedInUser.Role == "Admin")
        {
            AnsiConsole.Write(new Columns(
              new Panel(Align.Center(new Markup($"[bold yellow]{DataManager.GetTotalBooks()}[/]\n[grey]TOTAL COPIES[/]"))).Header(new PanelHeader("Inventory")).BorderColor(Color.Green),
              new Panel(Align.Center(new Markup($"[bold yellow]{DataManager.GetTotalUsers()}[/]\n[grey]USERS[/]"))).Header(new PanelHeader("Community")).BorderColor(Color.Cyan)
            ));
        }

        var menuChoices = new List<string> { "View All Books", "Search Book" };
        if (loggedInUser.Role == "Admin")
        {
            menuChoices.AddRange(new[] { "Add Book", "Update Book", "Delete Book", "Restore Center", "View Transaction History", "View Activity Logs" });
        }
        menuChoices.AddRange(new[] { "Issue a Book", "Return a Book", "Logout" });

        var action = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Actions:").PageSize(12).AddChoices(menuChoices));

        if (action == "Logout") { loggedInUser = null; }

        else if (action == "View All Books") ShowBooksTable(DataManager.LoadBooks());

        else if (action == "Search Book")
        {
            var k = AnsiConsole.Ask<string>("Enter Title, Author or Category:");
            ShowBooksTable(DataManager.SearchBooks(k));
        }

        else if (action == "Add Book" && loggedInUser.Role == "Admin")
        {
            var book = new Book
            {
                Title = AnsiConsole.Ask<string>("Title:"),
                Author = AnsiConsole.Ask<string>("Author:"),
                ISBN = AnsiConsole.Ask<string>("ISBN:"),
                Category = AnsiConsole.Ask<string>("Category:"),
                Quantity = AnsiConsole.Ask<int>("Quantity:"),
                Price = AnsiConsole.Ask<int>("Price:")
            };
            DataManager.AddBookToDb(book);
            DataManager.SaveLog("ADD_BOOK", $"Admin added '{book.Title}' to inventory.");
            AnsiConsole.MarkupLine("[bold green]✔ Book Added Successfully![/]");
            Thread.Sleep(1000);
        }

        else if (action == "Update Book" && loggedInUser.Role == "Admin")
        {
            var id = AnsiConsole.Ask<int>("Book ID to Update:");
            var col = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Field:").AddChoices("book_name", "author", "quantity", "price", "category", "isbn"));
            var newVal = AnsiConsole.Ask<string>("New Value:");
            DataManager.UpdateBookField(id, col, newVal);
            DataManager.SaveLog("UPDATE", $"Book ID {id} updated: {col} changed to {newVal}");
            AnsiConsole.MarkupLine("[bold blue]✔ Update Successful![/]");
            Thread.Sleep(1000);
        }

        else if (action == "Delete Book" && loggedInUser.Role == "Admin")
        {
            int id = AnsiConsole.Ask<int>("ID to Delete:");
            DataManager.DeleteBookFromDb(id);
            DataManager.SaveLog("DELETE_SOFT", $"Book ID {id} moved to Recycle Bin.");
            AnsiConsole.Write(new Panel(new Markup("[bold red]⚠ SYSTEM NOTICE:[/] Book moved to Recycle Bin")).BorderColor(Color.Red));
            Thread.Sleep(1500);
        }

        else if (action == "Restore Center" && loggedInUser.Role == "Admin")
        {
            var deleted = DataManager.GetDeletedBooks();
            if (deleted.Count == 0) { AnsiConsole.MarkupLine("[grey]Recycle Bin is empty.[/]"); Thread.Sleep(1000); continue; }

            var res = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Select Book:").AddChoices(deleted.Select(b => $"{b.Id}: {b.Title}").Append("Back")));

            if (res != "Back")
            {
                int id = int.Parse(res.Split(':')[0]);
                var subAction = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Action:").AddChoices("Restore", "Hard Delete", "Cancel"));

                if (subAction == "Restore")
                {
                    DataManager.RestoreBook(id);
                    DataManager.SaveLog("RESTORE", $"Book ID {id} restored to library.");
                    AnsiConsole.MarkupLine("[bold green]✔ Restored![/]");
                }
                else if (subAction == "Hard Delete")
                {
                    DataManager.HardDeleteBook(id);
                    DataManager.SaveLog("DELETE_HARD", $"Book ID {id} permanently deleted.");
                    AnsiConsole.MarkupLine("[bold red]✔ Permanently Deleted![/]");
                }
            }
            Thread.Sleep(1500);
        }

        else if (action == "Issue a Book")
        {
            var id = AnsiConsole.Ask<int>("Book ID:");
            string target = (loggedInUser.Role == "Admin") ? AnsiConsole.Ask<string>("Issue to (Username):") : loggedInUser.Username;

            if (DataManager.UpdateBookQuantity(id, -1))
            {
                DataManager.RecordTransaction(target, id, "Issue");
                DataManager.SaveLog("ISSUE", $"Book ID {id} issued to {target}");
                AnsiConsole.MarkupLine("[bold green]✔ Book Issued Successfully![/]");
            }
            else AnsiConsole.MarkupLine("[bold red]❌ Error: Out of Stock![/]");
            Thread.Sleep(1500);
        }

        else if (action == "Return a Book")
        {
            var id = AnsiConsole.Ask<int>("Book ID:");
            string target = (loggedInUser.Role == "Admin") ? AnsiConsole.Ask<string>("Return for (Username):") : loggedInUser.Username;

            DataManager.UpdateBookQuantity(id, 1);
            int fine = DataManager.RecordTransaction(target, id, "Return");
            DataManager.SaveLog("RETURN", $"Book ID {id} returned by {target}. Fine: {fine}");

            if (fine > 0)
            {
                AnsiConsole.Write(new Panel(new Markup($"[bold white on red] ⚠ LATE RETURN! FINE: {fine} PKR [/]")).BorderColor(Color.Red));
                DataManager.SaveLog("FINE_GEN", $"Fine of {fine} PKR generated for {target}");
            }
            else
            {
                AnsiConsole.MarkupLine("[bold green]✔ Book Returned Successfully![/]");
            }
            Thread.Sleep(2000);
        }

        else if (action == "View Transaction History" && loggedInUser.Role == "Admin")
        {
            var table = new Table().Title("[bold yellow]📜 TRANSACTION RECORDS[/]").BorderColor(Color.Grey);
            table.AddColumns("User", "Book ID", "Issue Date", "Return Date", "Status");

            foreach (DataRow r in DataManager.GetTransactionHistory().Rows)
            {
                string status = r["status"].ToString() ?? "-";
                string color = status == "Issued" ? "yellow" : "green";
                table.AddRow(r[0].ToString(), r[1].ToString(), r[2].ToString(), r[3].ToString(), $"[{color}]{status}[/]");
            }
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("[grey]Press any key to go back...[/]");
            Console.ReadKey();
        }

        else if (action == "View Activity Logs" && loggedInUser.Role == "Admin")
        {
            var table = new Table().Title("[bold cyan]⚙ SYSTEM ACTIVITY LOGS[/]").BorderColor(Color.DeepSkyBlue1);
            table.AddColumns("Action", "Details", "Timestamp");

            foreach (DataRow r in DataManager.GetLogs().Rows)
            {
                string type = r["action_type"].ToString();
                string color = type.Contains("DELETE") ? "red" : (type.Contains("ADD") ? "green" : "white");
                table.AddRow($"[{color}]{type}[/]", r["details"].ToString(), r["log_date"].ToString());
            }
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("[grey]Press any key to go back...[/]");
            Console.ReadKey();
        }
    }
}

// HELPER METHOD FOR TABLE DISPLAY
void ShowBooksTable(List<Book> books)
{
    var table = new Table().BorderColor(Color.DeepSkyBlue1).Title("[bold blue]📚 LIBRARY INVENTORY[/]");
    table.AddColumns("ID", "Title", "Author", "Qty", "Price");
    if (books.Count == 0) table.AddRow("[grey]No books found[/]", "-", "-", "-", "-");
    foreach (var b in books)
    {
        string qtyColor = b.Quantity > 0 ? "green" : "red";
        table.AddRow(b.Id.ToString(), $"[cyan]{b.Title}[/]", b.Author, $"[{qtyColor}]{b.Quantity}[/]", $"[yellow]{b.Price}[/]");
    }
    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
    Console.ReadKey();
}