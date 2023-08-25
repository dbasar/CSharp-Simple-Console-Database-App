using System.Data.SQLite;

namespace Simple_Console_Database_App
{
    internal class Program
    {
        // Method for manually handling user input
        static bool TryGetInput<T>(string message, bool isNumber, out T result)
        {
            Console.Write(message);

            string input = "";

            do
            {
                ConsoleKeyInfo currentChar = Console.ReadKey(true);

                if (currentChar.Key == ConsoleKey.Escape)
                {
                    // Escape button is pushed during the input

                    Console.WriteLine();
                    Console.WriteLine("Cancel process? 'Enter': Y / 'Esc': N");

                    ConsoleKeyInfo processKey = Console.ReadKey(true);
                    while (processKey.Key != ConsoleKey.Enter && processKey.Key != ConsoleKey.Escape)
                        processKey = Console.ReadKey(true);

                    if (processKey.Key == ConsoleKey.Enter)
                    {
                        if (isNumber)
                            result = (T)(object)0;
                        else
                            result = (T)(object)"";

                        Console.WriteLine("Exited the current process.");
                        Console.WriteLine();

                        return false;
                    }

                    else
                    {
                        return TryGetInput(message, isNumber, out result); // Start again
                    }
                }

                else if (currentChar.Key == ConsoleKey.Enter)
                {
                    //Enter button is pushed and the input is evaluated

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine();

                        if (isNumber)
                        {
                            if (int.TryParse(input, out int number)) // A last safety measure
                            {
                                result = (T)(object)number;

                                return true;
                            }
                            else
                            {
                                Console.WriteLine("Invalid input. Please try again.");

                                return TryGetInput(message, isNumber, out result);
                            }
                        }

                        else
                        {
                            result = (T)(object)input;

                            return true;
                        }
                    }
                }

                else if (currentChar.Key == ConsoleKey.Backspace)
                {
                    // Backspace button is pushed.

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        // Deletes a character from the end of the line if the line is not empty.

                        input = input.Remove(input.Length - 1);
                        Console.Write("\b \b");
                    }

                    // Does nothing and continues if the line is empty.
                }

                else if (isNumber)
                {
                    // If the required result is a number

                    if (!char.IsControl(currentChar.KeyChar) && char.IsDigit(currentChar.KeyChar))
                    {
                        input += currentChar.KeyChar;
                        Console.Write(currentChar.KeyChar);
                    }
                }

                else if (!char.IsControl(currentChar.KeyChar))
                {
                    // If the required result is a string

                    input += currentChar.KeyChar;
                    Console.Write(currentChar.KeyChar);
                }
            }
            while (true);
        }

        // Methods for opening and closing database connection
        static void OpenConnection(SQLiteConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
        }
        static void CloseConnection(SQLiteConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Closed)
                connection.Close();
        }

        // A quick method to check to see if the database is empty
        static bool IsDatabaseEmpty(SQLiteConnection connection)
        {
            OpenConnection(connection);
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Albums";
            SQLiteDataReader reader = command.ExecuteReader();
            bool readerHasRows = reader.HasRows;
            reader.Close();
            CloseConnection(connection);

            return !readerHasRows;
        }


        static void Main()
        {
            Console.WriteLine("SQLite3 database demo app by Doruk Başar.");
            Console.WriteLine();

            // Create the connection to the database
            var connection = new SQLiteConnection("Data Source=database.sqlite3;Version=3;DateTimeKind=Utc;");


            // Create the database file if it doesn't exist
            if (!File.Exists("./database.sqlite3"))
            {
                SQLiteConnection.CreateFile("database.sqlite3");
                Console.WriteLine("Database created.");
                Console.WriteLine();

                OpenConnection(connection);

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Albums (id INTEGER PRIMARY KEY, title VARCHAR(255), artist VARCHAR(255))";
                command.ExecuteNonQuery();

                CloseConnection(connection);
            }


            while (true)
            {
                // Inside the main menu 


                // Provide the list of commands before every command
                Console.WriteLine();
                Console.WriteLine("Press a button for a command ('Esc': Exit):");
                Console.WriteLine("List records: Q\t\tSearch: S\t\tAdd record: A");
                Console.WriteLine("Edit record: E\t\tDelete record: D\tReset records: X");

                // Get a key input for the command
                ConsoleKeyInfo key = Console.ReadKey(true);
                while (key.Key != ConsoleKey.Q &&
                       key.Key != ConsoleKey.S &&
                       key.Key != ConsoleKey.A &&
                       key.Key != ConsoleKey.E &&
                       key.Key != ConsoleKey.D &&
                       key.Key != ConsoleKey.X &&
                       key.Key != ConsoleKey.Escape)
                {
                    key = Console.ReadKey(true);
                }

                Console.WriteLine(); // To make an empty line after each command

                if (key.Key == ConsoleKey.Escape) // Command to quit the app
                {
                    Console.WriteLine("Are you sure? 'Enter': Y / 'Esc': N");

                    key = Console.ReadKey(true);
                    while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
                        key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Enter)
                        break;
                }

                else if (key.Key == ConsoleKey.Q) // Listing command
                {
                    OpenConnection(connection);
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Albums";
                    SQLiteDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        Console.WriteLine("All records are served:");
                        int i = 1;
                        while (reader.Read())
                        {
                            Console.WriteLine("{0}: #{1} - {2} - {3}", i++, reader["id"], reader["title"], reader["artist"]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No record in the database.");
                    }

                    reader.Close();
                    CloseConnection(connection);
                }

                else if (key.Key == ConsoleKey.S) // Search for records command
                {
                    if (IsDatabaseEmpty(connection))
                    {
                        Console.WriteLine("No record in the database.");
                    }

                    else
                    {
                        if (TryGetInput("Provide an input for search ('Esc': Exit): ", false, out string query))
                        {
                            OpenConnection(connection);
                            SQLiteCommand command = connection.CreateCommand();
                            command.CommandText = "SELECT * FROM Albums WHERE id = '" + query + "' OR title LIKE '%" + query + "%' OR artist LIKE '%" + query + "%';";
                            SQLiteDataReader reader = command.ExecuteReader();

                            if (reader.HasRows)
                            {
                                Console.WriteLine("Matching queries are served:");
                                int i = 1;
                                while (reader.Read())
                                {
                                    Console.WriteLine("{0}: #{1} - {2} - {3}", i++, reader["id"], reader["title"], reader["artist"]);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No record found.");
                            }
                            reader.Close();
                            CloseConnection(connection);
                        }
                    }
                }

                else if (key.Key == ConsoleKey.A) // Add new record command
                {
                    Console.WriteLine("You are adding a new record.");
                    Console.WriteLine();

                    if (TryGetInput("Type the name of the album ('Esc': Exit): ", false, out string titleString))
                    {
                        if (TryGetInput("Type the name of the artist ('Esc': Exit): ", false, out string artistString))
                        {
                            string commandString = "INSERT INTO Albums ('title', 'artist') VALUES ('" + titleString + "', '" + artistString + "');";

                            OpenConnection(connection);
                            SQLiteCommand command = connection.CreateCommand();
                            command.CommandText = commandString;
                            command.ExecuteNonQuery();
                            CloseConnection(connection);

                            Console.WriteLine("New record added.");
                        }
                    }
                }

                else if (key.Key == ConsoleKey.E) // Edit record command
                {
                    if (IsDatabaseEmpty(connection))
                    {
                        Console.WriteLine("No record in the database.");
                    }

                    else
                    {
                        if (TryGetInput("Type the ID of the record you wish to edit ('Esc': Exit): ", true, out int recordId))
                        {
                            OpenConnection(connection);
                            SQLiteCommand command = connection.CreateCommand();
                            command.CommandText = "SELECT * FROM Albums WHERE id = " + recordId + ";";
                            SQLiteDataReader reader = command.ExecuteReader();

                            if (reader.HasRows)
                            {
                                reader.Read();

                                Console.WriteLine("You are editing the record #{0}: {1} - {2}",
                                    reader["id"],
                                    reader["title"],
                                    reader["artist"]);
                                Console.WriteLine();

                                reader.Close();
                                CloseConnection(connection);

                                if (TryGetInput("Type the new name of the album ('Esc': Exit): ", false, out string titleString))
                                {
                                    if (TryGetInput("Type the new name of the artist ('Esc': Exit): ", false, out string artistString))
                                    {
                                        string commandString = "UPDATE Albums SET title = '" + titleString + "', artist = '" + artistString + "' WHERE id = " + recordId + ";";

                                        OpenConnection(connection);
                                        command = connection.CreateCommand();
                                        command.CommandText = commandString;
                                        command.ExecuteNonQuery();
                                        CloseConnection(connection);

                                        Console.WriteLine("Record updated.");
                                    }
                                }
                            }

                            else
                            {
                                reader.Close();
                                CloseConnection(connection);

                                Console.WriteLine("Record with the specified ID is not found.");
                            }
                        }
                    }
                }

                else if (key.Key == ConsoleKey.D) // Delete record command
                {
                    if (IsDatabaseEmpty(connection))
                    {
                        Console.WriteLine("No record in the database.");
                    }

                    else
                    {
                        if (TryGetInput("Type the ID of the record you wish to delete ('Esc': Exit): ", true, out int recordId))
                        {
                            OpenConnection(connection);
                            SQLiteCommand command = connection.CreateCommand();
                            command.CommandText = "SELECT * FROM Albums WHERE id = " + recordId + ";";
                            SQLiteDataReader reader = command.ExecuteReader();
                            if (reader.HasRows)
                            {
                                reader.Close();
                                command.CommandText = "DELETE FROM Albums WHERE id = " + recordId + ";";
                                command.ExecuteNonQuery();
                                Console.WriteLine("Record with the ID #{0} is deleted.", recordId);
                            }
                            else
                            {
                                reader.Close();
                                Console.WriteLine("Record with the specified ID is not found.");
                            }
                            CloseConnection(connection);
                        }
                    }
                }

                else if (key.Key == ConsoleKey.X) // Delete all records command
                {
                    if (IsDatabaseEmpty(connection))
                    {
                        Console.WriteLine("No record in the database.");
                    }

                    else
                    {
                        Console.WriteLine("Are you sure? 'Enter': Y / 'Esc': N");

                        key = Console.ReadKey(true);
                        while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
                            key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.Enter)
                        {
                            OpenConnection(connection);
                            SQLiteCommand command = connection.CreateCommand();
                            command.CommandText = "DELETE FROM Albums";
                            command.ExecuteNonQuery();
                            CloseConnection(connection);

                            Console.WriteLine("Database cleared.");
                        }
                    }
                }

                Console.WriteLine();
            }
        }
    }
}