
BankAccount bankAccount = new BankAccount();
BankTransfer bankTransfer = new BankTransfer();
EWallet eWallet = new EWallet();
CreditCard creditCard = new CreditCard();

bool repeat = true;
while (repeat)
{
    var config = new MenuConfig
    {
        // A slightly longer prompt to test the array handling
        InitialPrompt = [
        "Check Out Simulation",
        "------------------------------",
        ],
        // Options reflect the merged program structure
        Options = [
        "1. Bank Transfer",
        "2. E-Wallet",
        "3. Credit Card",
        "4. Quit Application"
        ],
        // Starting the menu at row 3 (after 2 lines of prompt + 1 blank line)
        Locat = 3,
        ShowMouseCursor = false,

        // --- Customization ---
        // Make the selector stand out with bright colors and a distinct pointer
        SelectedColor = ConsoleColor.Yellow,
        DefaultColor = ConsoleColor.White,
        Selector = "=> ",       // New pointer for selected item
        NonSelector = "   "      // Must match the length of Selector ("=> ") to prevent shifting
    };
    int choice = ConsoleMenu.Menu(config);

    switch (choice)
    {
        case 0:
            bankTransfer.Checkout(100);
            break;
        case 1:
            eWallet.Checkout(100);
            break;
        case 2:
            creditCard.Checkout(100);
            break;
        default:
            Console.WriteLine("ERROR: OUT OF BOUNDS");
            break;
    }
}
class BankAccount
{
    public virtual void Checkout(int amount)
    { 
        Console.WriteLine("PROCEED");
    }
}

class BankTransfer : BankAccount
{
    public override void Checkout(int amount)
    {
        Console.WriteLine($"Proceed to transfer {amount} KROMER to the credit card number 1219-7455-2597");
    }
}   

class EWallet : BankAccount
{
    public override void Checkout(int amount)
    {
        Console.WriteLine($"Transaction of {amount} KROMER has been successful");
    }
}

class CreditCard : BankAccount
{
    public override void Checkout(int amount)
    {
        Console.WriteLine($"Amount deducted by {amount} KROMER");
    }
}


// HorizonicLib
// --- 1. MENU CONFIGURATION STRUCT ---
public struct MenuConfig
{
    // Menu content
    public string[] InitialPrompt { get; set; }
    public string[] Options { get; set; }

    // Display settings: Locat is now a vertical offset from the current cursor position
    public int Locat { get; set; }
    public bool ShowMouseCursor { get; set; }

    // Customizable Formats
    public ConsoleColor SelectedColor { get; set; }
    public ConsoleColor DefaultColor { get; set; }
    public string Selector { get; set; }
    public string NonSelector { get; set; }
}

// --- 2. CONSOLE MENU CLASS ---
public class ConsoleMenu
{
    /// <summary>
    /// Displays a navigable console menu without clearing the screen.
    /// The menu is drawn starting at the current cursor position + config.Locat.
    /// </summary>
    /// <param name="config">The configuration settings for the menu.</param>
    /// <returns>The 0-based index of the selected option.</returns>
    public static int Menu(MenuConfig config)
    {
        // Capture the cursor position where the menu should start drawing.
        int currentCursorRow = Console.CursorTop + config.Locat;

        // --- Parameters from the Struct, providing sensible defaults ---
        string[] initialPrompt = config.InitialPrompt ?? Array.Empty<string>();
        string[] opts = config.Options ?? Array.Empty<string>();

        if (opts.Length == 0) return -1; // Handle empty options list

        bool showMouseCursor = config.ShowMouseCursor;
        ConsoleColor selectedColor = config.SelectedColor != 0 ? config.SelectedColor : ConsoleColor.DarkYellow;
        ConsoleColor defaultColor = config.DefaultColor != 0 ? config.DefaultColor : ConsoleColor.Gray;
        string selector = string.IsNullOrEmpty(config.Selector) ? ">> " : config.Selector;
        string nonSelector = string.IsNullOrEmpty(config.NonSelector) ? "   " : config.NonSelector;

        // Ensure alignment: nonSelector must match the visual width of selector
        // (This is a simplified assumption; ideal code would calculate character width)
        if (nonSelector.Length != selector.Length)
        {
            // Forcing alignment based on selector width
            nonSelector = new string(' ', selector.Length);
        }

        Console.CursorVisible = false;

        bool run = true;
        int lctn = 0; // Stores selected index. (0-based)


        // Types out the starting prompt.
        for (int i = 0; i < initialPrompt.Length; i++)
        {
            Console.SetCursorPosition(0, currentCursorRow + i);
            Console.Write(initialPrompt[i]);
        }

        int menuStartRow = currentCursorRow + initialPrompt.Length;
        int totalMenuLines = opts.Length;
        int cleanupEndRow = menuStartRow + totalMenuLines; // Line after the last option

        // Main Loop for Typing and Input
        while (run)
        {
            // Update the Menu
            for (int i = 0; i < opts.Length; i++)
            {
                Console.SetCursorPosition(0, menuStartRow + i);

                // Check if currently selected.
                if (i == lctn)
                {
                    Console.ForegroundColor = selectedColor;
                    Console.Write($"{selector}{opts[i]}");
                }
                else
                {
                    Console.ForegroundColor = defaultColor;
                    // Write spaces for alignment before the option text
                    Console.Write($"{nonSelector}{opts[i]}");
                }
                // Clear the rest of the line to prevent artifacts
                Console.Write(new string(' ', Console.WindowWidth - (selector.Length + opts[i].Length)));
            }
            Console.ResetColor();

            // Check for Input.
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                // Handle movement (Wrap-around navigation)
                if (key.Key == ConsoleKey.UpArrow)
                {
                    lctn = (lctn > 0) ? lctn - 1 : opts.Length - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    lctn = (lctn < opts.Length - 1) ? lctn + 1 : 0;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    run = false;
                }
            }
            // Prevents excessive CPU usage.
            Thread.Sleep(50);
        }

        // --- Cleanup: Clear the Menu Itself ---
        // Erase the prompt and the menu lines to make the output look clean.
        for (int i = 0; i < initialPrompt.Length + totalMenuLines; i++)
        {
            Console.SetCursorPosition(0, currentCursorRow + i);
            Console.Write(new string(' ', Console.WindowWidth)); // Overwrite with spaces
        }

        // --- Cleanup and Return ---
        Console.CursorVisible = showMouseCursor;
        // Move cursor below the area where the menu *was* drawn
        Console.SetCursorPosition(0, cleanupEndRow);
        return lctn; // Return 0-based index for the option.
    }

    public static void EndMessage(string[] message, string thx)
    {
        Thread.Sleep(1000);

        var config = new MenuConfig
        {
            InitialPrompt = message,
            Options = ["Insert Card", "Leave"],
            Locat = 0,
            ShowMouseCursor = false,

        };
        int choice = ConsoleMenu.Menu(config);

        if (choice == 1)
        {
            Console.Clear();
        }
        else
        {
            Console.WriteLine($"\n{thx}");
            Thread.Sleep(1000);
        }
    }

    public static string ReadWrite(string prompt)
    {
        // 1. Display the menu/prompt to the user
        Console.Write(prompt);

        // 2. Read the user's input from the console
        string input = Console.ReadLine();

        // 3. Return the input (or an empty string if null)
        return input ?? string.Empty;
    }
}