using System;
using System.Collections.Generic;

class Book
{
    public int Id;
    public string Title;
    public string Author;
    public bool IsBorrowed;
    public string BorrowedBy;
    public DateTime? BorrowedDate;

    public Book(int id, string title, string author)
    {
        Id = id;
        Title = title;
        Author = author;
        IsBorrowed = false;
        BorrowedBy = null;
        BorrowedDate = null;
    }
}

class Transaction
{
    public int BookId;
    public string Title;
    public string Action;
    public string Username;
    public DateTime Timestamp;

    public Transaction(int bookId, string title, string action, string username)
    {
        BookId = bookId;
        Title = title;
        Action = action;
        Username = username;
        Timestamp = DateTime.Now;
    }
}

class User
{
    public string Username;
    public string Password;

    public User(string username, string password)
    {
        Username = username;
        Password = password;
    }
}

class Library
{
    private List<Book> books = new List<Book>();
    private List<Transaction> transactions = new List<Transaction>();
    private List<User> users = new List<User>();
    private User currentUser = null;
    private Dictionary<string, int> userFines = new Dictionary<string, int>();

    public Library()
    {
        // seed some books
        books.Add(new Book(1, "The Hobbit", "J.R.R. Tolkien"));
        books.Add(new Book(2, "1984", "George Orwell"));
        books.Add(new Book(3, "Pride and Prejudice", "Jane Austen"));
        books.Add(new Book(4, "To Kill a Mockingbird", "Harper Lee"));
        books.Add(new Book(5, "The Great Gatsby", "F. Scott Fitzgerald"));

        // seed users: admin and a regular user
        users.Add(new User("admin", "admin123"));
        users.Add(new User("user", "user123"));
    }

    public bool IsLoggedIn()
    {
        return currentUser != null;
    }

    public bool IsAdminLoggedIn()
    {
        return currentUser != null && currentUser.Username == "admin";
    }

    public void Login()
    {
        Console.Clear();
        Console.Write("Enter username: ");
        string uname = Console.ReadLine();
        Console.Write("Enter password: ");
        string pwd = Console.ReadLine();

        Console.Clear();
        var user = users.Find(u => u.Username == uname && u.Password == pwd);
        if (user != null)
        {
            currentUser = user;
            Console.WriteLine($"Welcome, {currentUser.Username}!\n");
        }
        else
        {
            Console.WriteLine("Invalid credentials.\n");
        }
    }

    public void Register()
    {
        Console.Clear();
        Console.Write("Enter a new username: ");
        string newUsername = Console.ReadLine();
        if (users.Exists(u => u.Username == newUsername))
        {
            Console.WriteLine("Username already exists.\n");
            return;
        }

        Console.Write("Enter a password: ");
        string newPassword = Console.ReadLine();
        users.Add(new User(newUsername, newPassword));
        Console.Clear();
        Console.WriteLine("Registration successful. You can now log in.\n");
    }

    public void Logout()
    {
        if (currentUser != null)
        {
            Console.Clear();
            Console.WriteLine($"Goodbye, {currentUser.Username}!\n");
            currentUser = null;
        }
    }

    public void ViewBooks()
    {
        Console.Clear();
        Console.WriteLine("--- List of Books ---");
        foreach (var book in books)
        {
            string status = book.IsBorrowed
                ? $"Borrowed by {book.BorrowedBy}"
                : "Available";
            Console.WriteLine($"ID: {book.Id}, Title: {book.Title}, Author: {book.Author}, Status: {status}");
        }
        Console.WriteLine();
    }

    public void BorrowBook()
    {
        if (!IsLoggedIn()) return;

        ViewBooks();
        int borrowedCount = books.FindAll(b => b.BorrowedBy == currentUser.Username).Count;
        if (borrowedCount >= 3)
        {
            Console.WriteLine("You cannot borrow more than 3 books.\n");
            return;
        }

        Console.Write("Enter Book ID to borrow: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.\n");
            return;
        }

        var book = books.Find(b => b.Id == id);
        if (book != null && !book.IsBorrowed)
        {
            book.IsBorrowed = true;
            book.BorrowedBy = currentUser.Username;
            book.BorrowedDate = DateTime.Now;
            transactions.Add(new Transaction(id, book.Title, "Borrowed", currentUser.Username));
            Console.Clear();
            Console.WriteLine("Book borrowed successfully.\n");
        }
        else
        {
            Console.WriteLine("Book not available or not found.\n");
        }
    }

    public void ReturnBook()
    {
        if (!IsLoggedIn()) return;

        var borrowedBooks = books.FindAll(b => b.IsBorrowed && b.BorrowedBy == currentUser.Username);
        Console.Clear();
        Console.WriteLine("--- Your Borrowed Books ---");
        if (borrowedBooks.Count == 0)
        {
            Console.WriteLine("You haven't borrowed any books.\n");
            return;
        }
        foreach (var b in borrowedBooks)
            Console.WriteLine($"ID: {b.Id}, Title: {b.Title}, Borrowed Date: {b.BorrowedDate.Value.ToShortDateString()}");

        Console.Write("\nEnter Book ID to return: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.\n");
            return;
        }

        var book = books.Find(b => b.Id == id);
        if (book != null && book.IsBorrowed && book.BorrowedBy == currentUser.Username)
        {
            Console.Write($"Return \"{book.Title}\"? (y/n): ");
            string confirm = Console.ReadLine().Trim().ToLower();
            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Return cancelled.\n");
                return;
            }

            int overdueDays = (int)(DateTime.Now - book.BorrowedDate.Value).TotalDays - 7;
            if (overdueDays > 0)
            {
                Console.WriteLine($"Book is overdue by {overdueDays} days. Fine: ${overdueDays}");
                if (!userFines.ContainsKey(currentUser.Username))
                    userFines[currentUser.Username] = 0;
                userFines[currentUser.Username] += overdueDays;
            }

            book.IsBorrowed = false;
            book.BorrowedBy = null;
            book.BorrowedDate = null;
            transactions.Add(new Transaction(id, book.Title, "Returned", currentUser.Username));
            Console.WriteLine("Book returned successfully.\n");
        }
        else
        {
            Console.WriteLine("Book not found or not borrowed by you.\n");
        }
    }

    public void ViewTransactions()
    {
        Console.Clear();
        Console.WriteLine("--- Transaction History ---");
        foreach (var t in transactions)
            Console.WriteLine($"[{t.Timestamp}] Book ID: {t.BookId}, Title: {t.Title}, Action: {t.Action}, User: {t.Username}");
        Console.WriteLine();
    }

    public void DonateBook()
    {
        if (!IsLoggedIn()) return;

        Console.Write("Enter Book Title: ");
        string title = Console.ReadLine();
        Console.Write("Enter Book Author: ");
        string author = Console.ReadLine();

        int newId = books.Count > 0 ? books[books.Count - 1].Id + 1 : 1;
        books.Add(new Book(newId, title, author));
        transactions.Add(new Transaction(newId, title, "Donated", currentUser.Username));
        Console.Clear();
        Console.WriteLine("Thank you for donating the book!\n");
    }


    public void ViewMyFines()
    {
        if (!IsLoggedIn()) return;

        Console.Clear();
        Console.WriteLine("--- Your Fine Details ---");
        if (userFines.TryGetValue(currentUser.Username, out int fine))
            Console.WriteLine($"Total fine: ${fine}\n");
        else
            Console.WriteLine("You have no fines.\n");
    }

    public void AddBook()
    {
        if (!IsAdminLoggedIn())
        {
            Console.WriteLine("Admin only.\n");
            return;
        }

        Console.Write("Enter new Book ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.\n");
            return;
        }
        if (books.Exists(b => b.Id == id))
        {
            Console.WriteLine("ID already exists.\n");
            return;
        }

        Console.Write("Enter Title: ");
        string title = Console.ReadLine();
        Console.Write("Enter Author: ");
        string author = Console.ReadLine();

        books.Add(new Book(id, title, author));
        transactions.Add(new Transaction(id, title, "Added by Admin", currentUser.Username));
        Console.WriteLine("Book added.\n");
    }

    public void AdminDashboard()
    {
        if (!IsAdminLoggedIn())
        {
            Console.WriteLine("Admin only.\n");
            return;
        }

        Console.Clear();
        Console.WriteLine("--- Admin Dashboard ---\nBorrowed Books:");
        bool any = false;
        foreach (var b in books)
            if (b.IsBorrowed)
            {
                Console.WriteLine($"ID {b.Id}: {b.Title} borrowed by {b.BorrowedBy} on {b.BorrowedDate}");
                any = true;
            }
        if (!any) Console.WriteLine("None.");

        Console.WriteLine("\nUser Fines:");
        if (userFines.Count == 0)
            Console.WriteLine("No fines recorded.");
        else
            foreach (var kvp in userFines)
                Console.WriteLine($"{kvp.Key}: ${kvp.Value}");

        Console.WriteLine("\nRegistered Users:");
        foreach (var u in users)
            Console.WriteLine(u.Username);
        Console.WriteLine();
    }
}

class Program
{
    static void Main()
    {
        var library = new Library();
        int choice;
        do
        {
            Console.WriteLine("=-= WELCOME TO BOO KING =-=");
            if (!library.IsLoggedIn())
            {
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
            }

            else if (library.IsAdminLoggedIn())
            {
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. View Books");
                Console.WriteLine("2. Add Book");
                Console.WriteLine("3. Admin Dashboard");
                Console.WriteLine("4. Logout");
            }
            else
            {
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. View Books");
                Console.WriteLine("2. Borrow Book");
                Console.WriteLine("3. Return Book");
                Console.WriteLine("4. View Transaction History");
                Console.WriteLine("5. Donate Book");
                Console.WriteLine("6. View My Fines");
                Console.WriteLine("7. Logout");
            }

            Console.Write("\nEnter your choice: ");
            int.TryParse(Console.ReadLine(), out choice);
            Console.Clear();

            if (!library.IsLoggedIn())
            {
                switch (choice)
                {
                    case 0: Console.WriteLine("Exiting..."); break;
                    case 1: library.Login(); break;
                    case 2: library.Register(); break;
                    default: Console.WriteLine("Invalid choice.\n"); break;
                }
            }
            else if (library.IsAdminLoggedIn())
            {
                switch (choice)
                {
                    case 0: Console.WriteLine("Exiting..."); break;
                    case 1: library.ViewBooks(); break;
                    case 2: library.AddBook(); break;
                    case 3: library.AdminDashboard(); break;
                    case 4: library.Logout(); break;
                    default: Console.WriteLine("Invalid choice.\n"); break;
                }
            }
            else
            {
                switch (choice)
                {
                    case 0: Console.WriteLine("Exiting..."); break;
                    case 1: library.ViewBooks(); break;
                    case 2: library.BorrowBook(); break;
                    case 3: library.ReturnBook(); break;
                    case 4: library.ViewTransactions(); break;
                    case 5: library.DonateBook(); break;
                    case 6: library.ViewMyFines(); break;
                    case 7: library.Logout(); break;
                    default: Console.WriteLine("Invalid choice.\n"); break;
                }
            }

        }
        while (choice != 0);
    }
}
