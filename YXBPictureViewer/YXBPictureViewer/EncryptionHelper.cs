
using System;
using System.IO;

namespace YXBPictureViewer
{
    /// <summary>
    /// Interactive console-based helper for encryption tasks
    ///
    /// This class provides a user-friendly menu system for:
    /// - Generating new AES-256 keys
    /// - Converting single .ypg files to .xpg format
    /// - Batch converting entire directories
    ///
    /// Usage:
    /// Call EncryptionHelper.ShowMenu() from your code (e.g., from a button click or menu item)
    /// to launch the interactive console interface.
    ///
    /// Note: This was originally a standalone console app but was converted to a utility class
    /// to avoid conflicts with the main Windows Forms application entry point.
    /// </summary>
    public class EncryptionHelper
    {
        /// <summary>
        /// Displays an interactive menu for encryption operations
        /// </summary>
        /// <remarks>
        /// This method runs in a loop until the user chooses to exit.
        /// Call this from a button, menu item, or debug/maintenance code.
        ///
        /// Example:
        /// // Add to a button click handler or Tools menu
        /// EncryptionHelper.ShowMenu();
        /// </remarks>
        public static void ShowMenu()
        {
            Console.WriteLine("=== YXB Picture Viewer - Encryption Helper ===");
            Console.WriteLine();

            while (true)
            {
                // Display menu options
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Generate a new AES-256 key");
                Console.WriteLine("2. Convert a single .ypg file to .xpg");
                Console.WriteLine("3. Convert all .ypg files in a directory to .xpg");
                Console.WriteLine("4. Exit");
                Console.Write("\nYour choice (1-4): ");

                string choice = Console.ReadLine();

                // Handle user's choice
                switch (choice)
                {
                    case "1":
                        GenerateKey();
                        break;
                    case "2":
                        ConvertSingleFile();
                        break;
                    case "3":
                        ConvertDirectory();
                        break;
                    case "4":
                        Console.WriteLine("Goodbye!");
                        return; // Exit the menu loop
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Interactive key generation - prompts user and displays generated key
        /// </summary>
        /// <summary>
        /// Interactive key generation - prompts user and displays generated key
        /// </summary>
        private static void GenerateKey()
        {
            Console.WriteLine("\n--- Generate AES-256 Key ---");

            // Generate a new random AES-256 key
            string newKey = FileConverter.GenerateNewAESKey();

            // Display the key to the user
            Console.WriteLine($"\nYour new AES-256 key:");
            Console.WriteLine($"{newKey}");
            Console.WriteLine("\nIMPORTANT: Save this key securely! You'll need it to decrypt your files.");
            Console.WriteLine("Update Form1.cs with this key:");
            Console.WriteLine($"public static string aesKey = \"{newKey}\";");
        }

        /// <summary>
        /// Interactive single file conversion - prompts for file paths and keys
        /// </summary>
        private static void ConvertSingleFile()
        {
            Console.WriteLine("\n--- Convert Single File ---");

            // Get input file path from user
            Console.Write("Enter the path to the .ypg file: ");
            string inputFile = Console.ReadLine().Trim('"'); // Remove quotes if user copied path from Explorer

            // Validate file exists
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Error: File not found: {inputFile}");
                return;
            }

            // Validate file extension
            if (!inputFile.ToLower().EndsWith(".ypg"))
            {
                Console.WriteLine("Error: Input file must be a .ypg file");
                return;
            }

            // Auto-generate output filename (same name, .xpg extension)
            string outputFile = Path.Combine(
                Path.GetDirectoryName(inputFile),
                Path.GetFileNameWithoutExtension(inputFile) + ".xpg"
            );

            // Get DES key (with default)
            Console.Write($"Enter DES key (press Enter for default '{Form1.key}'): ");
            string desKey = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(desKey))
            {
                desKey = Form1.key; // Use default DES key
            }

            // Get AES key (required)
            Console.Write("Enter AES key: ");
            string aesKey = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(aesKey))
            {
                Console.WriteLine("Error: AES key cannot be empty");
                return;
            }

            // Perform conversion
            Console.WriteLine($"\nConverting: {Path.GetFileName(inputFile)} -> {Path.GetFileName(outputFile)}");

            try
            {
                bool success = FileConverter.ConvertYPGtoXPG(inputFile, outputFile, desKey, aesKey);

                if (success)
                {
                    Console.WriteLine("Conversion successful!");

                    // Ask if user wants to delete original
                    Console.Write("\nDelete the original .ypg file? (y/n): ");
                    string delete = Console.ReadLine().ToLower();
                    if (delete == "y" || delete == "yes")
                    {
                        try
                        {
                            File.Delete(inputFile);
                            Console.WriteLine($"Deleted: {Path.GetFileName(inputFile)}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Could not delete original file: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Conversion failed. Check the error messages above.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Interactive batch directory conversion - prompts for directory and settings
        /// </summary>
        private static void ConvertDirectory()
        {
            Console.WriteLine("\n--- Convert Directory ---");

            // Get directory path from user
            Console.Write("Enter the directory path: ");
            string directory = Console.ReadLine().Trim('"'); // Remove quotes if user copied path

            // Validate directory exists
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Error: Directory not found: {directory}");
                return;
            }

            // Get DES key (with default)
            Console.Write($"Enter DES key (press Enter for default '{Form1.key}'): ");
            string desKey = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(desKey))
            {
                desKey = Form1.key; // Use default DES key
            }

            // Get AES key (required)
            Console.Write("Enter AES key: ");
            string aesKey = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(aesKey))
            {
                Console.WriteLine("Error: AES key cannot be empty");
                return;
            }

            // Ask about subdirectories
            Console.Write("Include subdirectories? (y/n): ");
            bool includeSubdirs = Console.ReadLine().ToLower().StartsWith("y");

            // Ask about deleting originals (with warning)
            Console.Write("Delete original .ypg files after successful conversion? (y/n): ");
            bool deleteOriginal = Console.ReadLine().ToLower().StartsWith("y");

            if (deleteOriginal)
            {
                Console.WriteLine("\nWARNING: You chose to delete original files!");
                Console.WriteLine("Make sure you have backups. Press any key to continue or Ctrl+C to cancel...");
                Console.ReadKey();
            }

            Console.WriteLine("\nStarting conversion...");

            try
            {
                // Perform batch conversion
                int count = FileConverter.ConvertAllYPGInDirectory(
                    directory,
                    desKey,
                    aesKey,
                    includeSubdirs,
                    deleteOriginal
                );

                // Display results
                Console.WriteLine($"\n=== Conversion Complete ===");
                Console.WriteLine($"Total files converted: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
