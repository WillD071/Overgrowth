using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;


public class Deployment
{
    public static void Main(string[] args)
    {
        string primaryWatchdogProject = "Watchdog/Watchdog.csproj";
        string secondaryWatchdogProject = "Watchdog/Watchdog2.csproj";

        try
        {
            string relativePath = Path.Combine(Directory.GetCurrentDirectory(), "Confits");
            string fullPath = Path.GetFullPath(relativePath);
            if (Directory.Exists(fullPath))
            {
                foreach (string dir in Directory.GetDirectories(fullPath, "*", SearchOption.AllDirectories))
                {
                    




                }
            }
            else
            {
                Console.WriteLine("Directory does not exist: " + fullPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }

    }


    public static void Publish(string projectPath, string outputPath, string assemblyName, string configPath)
    {
        // Build the dotnet publish arguments
        var publishArgs = $"publish \"{projectPath}\" -o \"{outputPath}\" -p:AssemblyName={assemblyName}";

        // Configure the process start info
        var processInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = publishArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process
        using (var process = new Process { StartInfo = processInfo })
        {
            process.Start();

            // Read and display output
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Console.WriteLine("Output:");
            Console.WriteLine(output);

            if (!string.IsNullOrWhiteSpace(errors))
            {
                Console.WriteLine("Errors:");
                Console.WriteLine(errors);
            }

            Console.WriteLine($"Publish process exited with code {process.ExitCode}");

            // Copy Config.cs to the output directory
            if (File.Exists(configPath))
            {
                try
                {
                    string destinationConfigPath = Path.Combine(outputPath, "Config.cs");
                    File.Copy(configPath, destinationConfigPath, overwrite: true);
                    Console.WriteLine($"Config.cs copied to {destinationConfigPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying Config.cs: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Config.cs not found at the specified path.");
            }
        }
    }

    public static string[]? GetWatchdogNames(String filePath)
    {

        try
        {
            string fileContent = File.ReadAllText(filePath);
            // Regular expressions to match the SecondaryWatchdogName and PrimaryWatchdogName values
            string secondaryPattern = @"public static string SecondaryWatchdogName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)"";";
            string primaryPattern = @"public static string PrimaryWatchdogName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)"";";

            // Extract the values using regex
            var secondaryMatch = Regex.Match(fileContent, secondaryPattern);
            var primaryMatch = Regex.Match(fileContent, primaryPattern);

            if (secondaryMatch.Success)
            {
                Console.WriteLine("SecondaryWatchdogName: " + secondaryMatch.Groups[1].Value);
            }
            else
            {
                Console.WriteLine("SecondaryWatchdogName not found.");
            }

            if (primaryMatch.Success)
            {
                Console.WriteLine("PrimaryWatchdogName: " + primaryMatch.Groups[1].Value);
            }
            else
            {
                Console.WriteLine("PrimaryWatchdogName not found.");
            }

            return [secondaryMatch.Groups[1].Value, primaryMatch.Groups[1].Value];
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading the file: " + ex.Message);
           return null;
        }
    }

    public static (string fullPath, string lastDirectory) GetPrimaryWatchdogPathInfo(string filePath)
    {
        try
        {
            // Read all text from the file
            string fileContent = File.ReadAllText(filePath);

            // Regex pattern to capture the folder path in PrimaryWatchdogPath
            string primaryPathPattern = @"public static string PrimaryWatchdogPath\s*{\s*get;\s*private\s*set;\s*}\s*=\s*@""([^""]+)"";";

            // Match the PrimaryWatchdogPath and extract the folder name
            var match = Regex.Match(fileContent, primaryPathPattern);

            if (match.Success)
            {
                // Extract the path value
                string primaryPath = match.Groups[1].Value;

                // Get the last folder name
                string lastDirectory = new DirectoryInfo(primaryPath.TrimEnd(Path.DirectorySeparatorChar)).Name;

                return (primaryPath, lastDirectory);
            }
            else
            {
                Console.WriteLine("PrimaryWatchdogPath not found.");
                return (null, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading the file: " + ex.Message);
            return (null, null);
        }
    }
}


