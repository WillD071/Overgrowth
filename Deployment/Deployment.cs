using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;


public class Deployment
{
    public static void Main(string[] args)
    {
        switchToDeploymentFolder();

        try
        {
            string? parentPath = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName; //       \WindowsPersistence\ Folder
            string DeploymentPath = Path.Combine(parentPath, "Deployment", "DeployBins");


            Directory.Delete(DeploymentPath, true); // deletes the old deployment
            Directory.CreateDirectory(DeploymentPath); // Creates new deployment folder

            string relativePath = Path.Combine(Directory.GetCurrentDirectory(), "Configs");
            string fullPath = Path.GetFullPath(relativePath); // \Deployment\Configs\ Folder for the iterator
            if (Directory.Exists(fullPath))
            {  
                int i = 1;
                string primaryWatchDogProjectPath = Path.Combine(parentPath, "PrimaryWatchdog", "PrimaryWatchdog.csproj");
                string secondaryWatchDogProjectPath = Path.Combine(parentPath, "SecondaryWatchdog", "SecondaryWatchdog.csproj");
                string ConfigFilePath = Path.Combine(parentPath, "Config.cs");
                string parentOutputBinsPath = Path.Combine(parentPath, "Deployment", "DeployBins");
                foreach (string dir in Directory.GetDirectories(fullPath, "*", SearchOption.AllDirectories))
                {
                    string iteratedConfigPath = Path.Combine(dir, "Config.cs");
                    (string PrimaryWatchdogName, string SecondaryWatchdogName, string payloadName) binaryNames = GetWatchdogNames(iteratedConfigPath);


                    (string fullPath, string lastDirectory) watchdogInfo = GetPrimaryWatchdogPathInfo(iteratedConfigPath);
                    string dirName = "Deployment" + i + "To-" + watchdogInfo.lastDirectory;

                    string iteratedOutputBinsPath = Path.Combine(parentPath, "Deployment", "DeployBins", dirName);

                    ReplaceConfigFile(ConfigFilePath, iteratedConfigPath); // replaces the project's config file with the one for current deploy
                    Directory.CreateDirectory(iteratedOutputBinsPath);


                    File.WriteAllText(Path.Combine(iteratedOutputBinsPath, "DeployPath.txt"), watchdogInfo.fullPath); //write the primary watchdog path to a .txt


                    
                    Publish(primaryWatchDogProjectPath, iteratedOutputBinsPath, binaryNames.PrimaryWatchdogName);
                    Publish(secondaryWatchDogProjectPath, iteratedOutputBinsPath, binaryNames.SecondaryWatchdogName);

                    string[] exeFiles = Directory.GetFiles(dir, "*.exe");
                    string iteratedPayloadPath = exeFiles[0];

                    File.Copy(iteratedPayloadPath, Path.Combine(iteratedOutputBinsPath, binaryNames.payloadName), true);

                    i++;
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


    public static void switchToDeploymentFolder()
    {
        try
        {
            string targetFolderName = "Deployment";

            while (true)
            {
                string currentDirectory = Environment.CurrentDirectory; // Get the current directory
                string folderName = new DirectoryInfo(currentDirectory).Name;

                if (string.Equals(folderName, targetFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Found the target folder: {currentDirectory}");
                    break;
                }

                DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);

                if (parentDirectory == null)
                {
                    Console.WriteLine("Reached the root directory. Target folder not found.");
                    break;
                }

                Environment.CurrentDirectory = parentDirectory.FullName; // Change the current directory to the parent
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }


    public static void Publish(string projectPath, string outputPath, string assemblyName)
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

          
        }
    }

    public static (string PrimaryWatchdogName, string SecondaryWatchdogName, string payloadName) GetWatchdogNames(String filePath)
    {

        try
        {
            string fileContent = File.ReadAllText(filePath);
            // Regular expressions to match the SecondaryWatchdogName and PrimaryWatchdogName values
            string secondaryPattern = @"public static string SecondaryWatchdogName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)"";";
            string primaryPattern = @"public static string PrimaryWatchdogName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)"";";
            string payloadPattern = @"public static string PayloadName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)"";";


            // Extract the values using regex
            var secondaryMatch = Regex.Match(fileContent, secondaryPattern);
            var primaryMatch = Regex.Match(fileContent, primaryPattern);
            var payloadMatch = Regex.Match(fileContent, payloadPattern);

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

            if (payloadMatch.Success)
            {
                Console.WriteLine("payloadName: " + payloadMatch.Groups[1].Value);
            }
            else
            {
                Console.WriteLine("payloadName not found.");
            }

            return (primaryMatch.Groups[1].Value, secondaryMatch.Groups[1].Value, payloadMatch.Groups[1].Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading the file: " + ex.Message);
           return (null, null, null);
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

    public static void ReplaceConfigFile(string oldFilePath, string newFilePath)
    {
        try
        {
            // Check if the old Config.cs file exists
            if (File.Exists(oldFilePath))
            {
                // Delete the old Config.cs file
                File.Delete(oldFilePath);
                Console.WriteLine("Old Config.cs file deleted.");
            }

            // Check if the new Config.cs file exists
            if (File.Exists(newFilePath))
            {
                // Copy the new Config.cs file to the old location
                File.Copy(newFilePath, oldFilePath);
                Console.WriteLine("New Config.cs file copied.");
            }
            else
            {
                Console.WriteLine("New Config.cs file does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}


