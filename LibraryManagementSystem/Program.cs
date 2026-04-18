using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Spectre.Console;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System;

List<User> userList = new List<User>();
User? loggedInUser = null;

while (true)
{
    AnsiConsole.Clear();
    var headerText = new Text("WELCOME TO LIBRARY MANAGEMENT SYSTEM", new Style(Color.Yellow)).Centered();
    var header = new Panel(headerText);
    header.Header = new PanelHeader("KICSIT LIBRARY DATABASE EDITION");
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
            AnsiConsole.MarkupLine("[cyan]Developed by Nafeesa Haroon\nInstitute of Space Technology (KICSIT)[/]");
            Console.ReadKey();
        }
        else break;
    }
    else
    {
        AnsiConsole.Write(new Panel(new Markup($"[bold white]USER:[/] [cyan]{loggedInUser.Username.ToUpper()}[/] | [bold white]ROLE:[/] [yellow]{loggedInUser.Role}[/]")).BorderColor(Color.Grey));

        if (loggedInUser.Role == "Admin")
        {
            var tb = DataManager.GetTotalBooks();
            var tu = DataManager.GetTotalUsers();
            AnsiConsole.Write(new Columns(
                new Panel(Align.Center(new Markup($"[bold yellow]{tb}[/]\n[grey]TOTAL COPIES[/]"))).Header(new PanelHeader("Inventory")).BorderColor(Color.Green),
                new Panel(Align.Center(new Markup($"[bold yellow]{tu}[/]\n[grey]USERS[/]"))).Header(new PanelHeader("Community")).BorderColor(Color.Cyan)
            ));
        }

        var menuChoices = new List<string> { "View All Books", "Search Book" };
        if (loggedInUser.Role == "Admin")
        {
            menuChoices.Add("Update Book");
            menuChoices.Add("Add Book");
            menuChoices.Add("Delete Book");
        }
        menuChoices.Add("Logout");

        var action = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Actions:").AddChoices(menuChoices));

        if (action == "Logout") loggedInUser = null;
        else if (action == "View All Books") ShowBooksTable(DataManager.LoadBooks());
        else if (action == "Search Book")
        {
            var k = AnsiConsole.Ask<string>("Search Title, Author or ISBN:");
            ShowBooksTable(DataManager.SearchBooks(k));
        }
        else if (action == "Add Book" && loggedInUser.Role == "Admin")
        {
            var t = AnsiConsole.Ask<string>("Title:");
            var a = AnsiConsole.Ask<string>("Author:");
            var i = AnsiConsole.Ask<string>("ISBN:");
            var c = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Category:").AddChoices("Computer Science", "Mathematics", "Islamic", "Literature", "General"));
            var q = AnsiConsole.Ask<int>("Quantity:");
            var p = AnsiConsole.Ask<int>("Price:");
            DataManager.AddBookToDb(new Book { Title = t, Author = a, Quantity = q, ISBN = i, Category = c, Price = p });
            AnsiConsole.MarkupLine("[bold green]✔ Added![/]"); Thread.Sleep(1000);
        }
        else if (action == "Update Book" && loggedInUser.Role == "Admin")
        {
            var id = AnsiConsole.Ask<int>("[yellow]Enter Book ID to Update:[/]");
            var field = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("What would you like to update?").AddChoices("Title", "Author", "ISBN", "Category", "Quantity", "Price", "Back"));

            if (field == "Title")
            {
                var val = AnsiConsole.Ask<string>("New Title:");
                DataManager.UpdateBookField(id, "book_name", val);
            }
            else if (field == "Author")
            {
                var val = AnsiConsole.Ask<string>("New Author:");
                DataManager.UpdateBookField(id, "author", val);
            }
            else if (field == "ISBN")
            {
                var val = AnsiConsole.Ask<string>("New ISBN:");
                DataManager.UpdateBookField(id, "isbn", val);
            }
            else if (field == "Category")
            {
                var val = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Select Category:").AddChoices("Computer Science", "Mathematics", "Islamic", "Literature", "General"));
                DataManager.UpdateBookField(id, "category", val);
            }
            else if (field == "Quantity")
            {
                var val = AnsiConsole.Ask<int>("New Quantity:");
                DataManager.UpdateBookField(id, "quantity", val);
            }
            else if (field == "Price")
            {
                var val = AnsiConsole.Ask<int>("New Price:");
                DataManager.UpdateBookField(id, "price", val);
            }

            if (field != "Back")
            {
                AnsiConsole.MarkupLine("[bold green]✔ Updated Successfully![/]");
                Thread.Sleep(1000);
            }
        }
        else if (action == "Delete Book" && loggedInUser.Role == "Admin")
        {
            var id = AnsiConsole.Ask<int>("ID to delete:");
            if (AnsiConsole.Confirm("Delete this book?"))
            {
                DataManager.DeleteBookFromDb(id);
                AnsiConsole.MarkupLine("[bold red]✔ Deleted![/]");
            }
            Thread.Sleep(1000);
        }
    }
}

void ShowBooksTable(List<Book> books)
{
    var table = new Table().BorderColor(Color.DeepSkyBlue1);
    table.AddColumns("[yellow]ID[/]", "[yellow]Title[/]", "[yellow]Author[/]", "[yellow]ISBN[/]", "[yellow]Category[/]", "[yellow]Price[/]", "[yellow]Qty[/]");
    foreach (var b in books) table.AddRow(b.Id.ToString(), b.Title, b.Author, b.ISBN ?? "N/A", b.Category ?? "Gen", b.Price.ToString("N0"), b.Quantity.ToString());
    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("\n[grey]Press any key...[/]"); Console.ReadKey();
}