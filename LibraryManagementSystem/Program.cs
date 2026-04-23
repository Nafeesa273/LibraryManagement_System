using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Spectre.Console;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Data;

List<User> userList = new List<User>();
User? loggedInUser = null;

while (true)
{
    AnsiConsole.Clear();
    // Yahan Header Text change kiya hy
    var headerText = new Text("LIBRARY MANAGEMENT SYSTEM", new Style(Color.Yellow)).Centered();
    var header = new Panel(headerText);

    // KICSIT ki jagah LMS | DIGITAL EDITION set kar diya hy
    header.Header = new PanelHeader("LMS | DIGITAL EDITION");
    header.Border = BoxBorder.Double;
    header.BorderColor(Color.DeepSkyBlue1);
    header.Expand = true;
    AnsiConsole.Write(header);

    if (loggedInUser == null)
    {
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Please [bold cyan]Login[/] to access.").AddChoices("Login", "Register", "About System", "Exit"));
        if (choice == "Login")
        {
            userList = DataManager.LoadUsers();
            var name = AnsiConsole.Ask<string>("Username:");
            var pass = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());
            loggedInUser = userList.FirstOrDefault(u => u.Username == name && u.Password == pass);
            if (loggedInUser != null) { AnsiConsole.MarkupLine("[bold green]✔ Access Granted![/]"); Thread.Sleep(1000); }
            else { AnsiConsole.MarkupLine("[bold red]❌ Invalid Login![/]"); Thread.Sleep(2000); }
        }
        else if (choice == "Register")
        {
            var name = AnsiConsole.Ask<string>("New Username:");
            var pass = AnsiConsole.Prompt(new TextPrompt<string>("New Password:").Secret());
            var role = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Role:").AddChoices("Admin", "User"));
            DataManager.RegisterUser(new User { Username = name, Password = pass, Role = role });
            AnsiConsole.MarkupLine("[bold green]✔ Registered![/]"); Thread.Sleep(1000);
        }
        else if (choice == "About System")
        {
            // Developed by Nafeesa Haroon (KICSIT reference kept in credits)
            AnsiConsole.MarkupLine("[cyan]Developed by Nafeesa Haroon\nInstitute of Space Technology[/]");
            Console.ReadKey();
        }
        else break;
    }
    else
    {
        AnsiConsole.Write(new Panel(new Markup($"[bold white]USER:[/] [cyan]{loggedInUser.Username.ToUpper()}[/] | [bold white]ROLE:[/] [yellow]{loggedInUser.Role}[/]")).BorderColor(Color.Grey));

        var tb = DataManager.GetTotalBooks();
        if (loggedInUser.Role == "Admin")
        {
            var tu = DataManager.GetTotalUsers();
            AnsiConsole.Write(new Columns(
                new Panel(Align.Center(new Markup($"[bold yellow]{tb}[/]\n[grey]TOTAL COPIES[/]"))).Header(new PanelHeader("Inventory")).BorderColor(Color.Green),
                new Panel(Align.Center(new Markup($"[bold yellow]{tu}[/]\n[grey]USERS[/]"))).Header(new PanelHeader("Community")).BorderColor(Color.Cyan)
            ));
        }
        else
        {
            AnsiConsole.Write(new Panel(new Markup($"[bold green]WELCOME, {loggedInUser.Username.ToUpper()}![/] Currently, we have [yellow]{tb}[/] books.")).BorderColor(Color.Blue));
        }

        // --- UPDATED MENU ARRANGEMENT ---
        var menuChoices = new List<string> { "View All Books", "Search Book" };

        if (loggedInUser.Role == "Admin")
        {
            menuChoices.Add("Add Book");
            menuChoices.Add("Update Book");
            menuChoices.Add("Delete Book");
            menuChoices.Add("View Transaction History");
        }

        menuChoices.Add("Issue a Book");
        menuChoices.Add("Return a Book");
        menuChoices.Add("Logout");

        var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Actions:")
            .PageSize(10)
            .AddChoices(menuChoices));

        if (action == "Logout") loggedInUser = null;
        else if (action == "View All Books") ShowBooksTable(DataManager.LoadBooks());
        else if (action == "Search Book")
        {
            var k = AnsiConsole.Ask<string>("Search Title or Author:");
            ShowBooksTable(DataManager.SearchBooks(k));
        }
        else if (action == "Add Book" && loggedInUser.Role == "Admin")
        {
            var t = AnsiConsole.Ask<string>("Title:");
            var a = AnsiConsole.Ask<string>("Author:");
            var q = AnsiConsole.Ask<int>("Quantity:");
            var p = AnsiConsole.Ask<int>("Price:");
            DataManager.AddBookToDb(new Book { Title = t, Author = a, Quantity = q, Price = p });
            AnsiConsole.MarkupLine("[bold green]✔ Added![/]"); Thread.Sleep(1000);
        }
        else if (action == "Update Book" && loggedInUser.Role == "Admin")
        {
            var id = AnsiConsole.Ask<int>("Enter Book ID to update:");
            var col = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Field:").AddChoices("book_name", "author", "quantity", "price"));
            var val = AnsiConsole.Ask<string>($"New value for {col}:");
            DataManager.UpdateBookField(id, col, val);
            AnsiConsole.MarkupLine("[bold green]✔ Updated![/]"); Thread.Sleep(1000);
        }
        else if (action == "Delete Book" && loggedInUser.Role == "Admin")
        {
            var id = AnsiConsole.Ask<int>("ID to delete:");
            DataManager.DeleteBookFromDb(id);
            AnsiConsole.MarkupLine("[bold red]✔ Deleted![/]"); Thread.Sleep(1000);
        }
        else if (action == "Issue a Book")
        {
            var id = AnsiConsole.Ask<int>("[yellow]Enter Book ID to Issue:[/]");
            bool success = DataManager.UpdateBookQuantity(id, -1);
            if (success)
            {
                DataManager.RecordTransaction(loggedInUser.Username, id, "Issue");
                AnsiConsole.Write(new Panel(new Markup("[bold green]✔ ISSUED SUCCESSFULLY![/]\n[white]Return within 7 days to avoid Rs. 100/day fine.[/]")).BorderColor(Color.Green));
            }
            else AnsiConsole.MarkupLine("[bold red]❌ Out of Stock![/]");
            Thread.Sleep(2000);
        }
        else if (action == "Return a Book")
        {
            var id = AnsiConsole.Ask<int>("[yellow]Enter Book ID to Return:[/]");
            DataTable dt = DataManager.GetTransactionHistory();
            DataRow? transaction = dt.AsEnumerable()
                .FirstOrDefault(r => r.Field<string>("username") == loggedInUser.Username
                                && r.Field<int>("book_id") == id
                                && r.Field<string>("status") == "Issued");

            if (transaction != null)
            {
                DateTime issueDate = Convert.ToDateTime(transaction["issue_date"]);
                DateTime returnDate = DateTime.Now;
                int daysDiff = (returnDate - issueDate).Days;
                int fine = (daysDiff > 7) ? (daysDiff - 7) * 100 : 0;

                DataManager.UpdateBookQuantity(id, 1);
                DataManager.RecordTransaction(loggedInUser.Username, id, "Return");

                var summaryTable = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
                summaryTable.AddColumn("[bold yellow]Detail[/]");
                summaryTable.AddColumn("[bold yellow]Value[/]");
                summaryTable.AddRow("Days Kept", daysDiff.ToString());
                summaryTable.AddRow("Status", daysDiff > 7 ? "[red]LATE[/]" : "[green]ON TIME[/]");
                summaryTable.AddRow("Fine (Rs. 100/day)", fine > 0 ? $"[bold red]Rs. {fine}[/]" : "[bold green]Rs. 0[/]");

                AnsiConsole.Write(new Panel(summaryTable).Header("📊 RETURN SUMMARY").BorderColor(Color.Cyan1));
                AnsiConsole.MarkupLine("\n[grey]Press any key...[/]"); Console.ReadKey();
            }
            else { AnsiConsole.MarkupLine("[bold red]❌ No active issuance found![/]"); Thread.Sleep(2000); }
        }
        else if (action == "View Transaction History" && loggedInUser.Role == "Admin")
        {
            DataTable history = DataManager.GetTransactionHistory();
            var table = new Table().BorderColor(Color.Magenta1).Title("[bold yellow]📜 ISSUANCE LOGS[/]");
            table.AddColumns("User", "Book ID", "Issue Date", "Return Date", "Status");
            foreach (DataRow row in history.Rows)
                table.AddRow(row["username"].ToString(), row["book_id"].ToString(), row["issue_date"].ToString(), row["return_date"]?.ToString() ?? "-", row["status"].ToString());

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("\n[grey]Press any key...[/]"); Console.ReadKey();
        }
    }
}

void ShowBooksTable(List<Book> books)
{
    var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.DeepSkyBlue1).Title("[bold yellow]📚 LIBRARY COLLECTION[/]");
    table.AddColumns("[bold yellow]ID[/]", "[bold yellow]Title[/]", "[bold yellow]Author[/]", "[bold yellow]Price[/]", "[bold yellow]Status[/]");
    foreach (var b in books)
    {
        string statusText = b.Quantity > 0 ? $"[green]Available ({b.Quantity})[/]" : "[red]Out of Stock[/]";
        table.AddRow(b.Id.ToString(), b.Title, b.Author, b.Price.ToString("N0"), statusText);
    }
    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]"); Console.ReadKey();
}