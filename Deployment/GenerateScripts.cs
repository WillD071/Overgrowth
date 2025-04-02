using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

public class WatchdogInfo
{
    public string DirectoryPath { get; set; }
    public string PayloadName { get; set; }
    public string PrimaryWatchdogName { get; set; }
    public string SecondaryWatchdogName { get; set; }
}
public class GenerateScripts
{
    public static List<WatchdogInfo> WatchdogDatabase { get; private set; } = new List<WatchdogInfo>();
    public static string processList = "";
    public static string dirList = "";
    public static List<(string Directory, string PrimaryWatchdogName, string dirName)> WatchdogTuples = new List<(string, string, string)>();

    public static void makeDeployFiles(string newConfigPath, string OutputBins, string oldConfigPath)
    {
        Directory.Delete(newConfigPath, true);
        Directory.CreateDirectory(newConfigPath);

        string filesPath = Path.Combine(newConfigPath, "files");
        string tasksPath = Path.Combine(newConfigPath, "tasks");


        Directory.CreateDirectory(filesPath);
        Directory.CreateDirectory(tasksPath);

        CopyFilesRecursively(OutputBins, filesPath);

        extractBinaryInfo(filesPath);


            foreach (WatchdogInfo watchdogInfo in WatchdogDatabase)
        {
            processList += "\"" + watchdogInfo.PayloadName + "\",";
            processList += "\"" + watchdogInfo.PrimaryWatchdogName + "\",";
            processList += "\"" + watchdogInfo.SecondaryWatchdogName + "\",";
        }
        processList = processList.TrimEnd(',');
        processList = processList.Replace(".exe", "");

        string deployDirsPath = Directory.GetParent(oldConfigPath).FullName;
        deployDirsPath = Path.Combine(deployDirsPath, "WinPersistAnsible", "files");

        generateAnsiblePlaybook(Path.Combine(tasksPath, "playbook.yml"), deployDirsPath);

        generateDebugScript(Path.Combine(filesPath, "Debug.ps1"));

        string filePaths = GetFilePathsFromConfig(oldConfigPath);
        filePaths = filePaths.Replace("\"C:\\Windows\\\",","");
        filePaths = filePaths.Replace("\"C:\\\",", "");
        generatePowerShellScript(Path.Combine(filesPath, "KillPersistence.ps1"), filePaths);



    }
    public static void extractBinaryInfo(string outputBins)
    {
        string[] directories = Directory.GetDirectories(outputBins);

        foreach (string directory in directories)
        {
            string txtFilePath = Path.Combine(directory, "DeployPath.txt"); // Assuming the text file is named 'file.txt'
            if (File.Exists(txtFilePath))
            {
                // Read the content of the .txt file
                string[] lines = File.ReadAllLines(txtFilePath);

                // Create a WatchdogInfo object to store the extracted data
                WatchdogInfo watchdogInfo = new WatchdogInfo
                {
                    DirectoryPath = directory,
                    PayloadName = ExtractInfo(lines, "Payload Name:"),
                    PrimaryWatchdogName = ExtractInfo(lines, "Primary Watchdog Name:"),
                    SecondaryWatchdogName = ExtractInfo(lines, "Secondary Watchdog Name:")
                };

                // Add the WatchdogInfo object to the static list
                WatchdogDatabase.Add(watchdogInfo);
            }
            else
            {
                Console.WriteLine($"File not found in directory: {directory}");
            }
        }

    }

    static string ExtractInfo(string[] lines, string prefix)
    {
        foreach (string line in lines)
        {
            if (line.StartsWith(prefix))
            {
                return line.Substring(prefix.Length).Trim();
            }
        }
        return string.Empty;
    }
    private static void CopyFilesRecursively(string sourceDir, string targetDir)
    {
        // Copy all files from the source to the target
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, targetFile, overwrite: true);
            Console.WriteLine("Copied file: " + file);
        }

        // Recursively copy subdirectories
        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
            if (!Directory.Exists(targetSubDir))
            {
                Directory.CreateDirectory(targetSubDir);
                Console.WriteLine("Created subdirectory: " + targetSubDir);
            }

            // Recurse into subdirectories
            CopyFilesRecursively(directory, targetSubDir);
        }
    }

    public static void generateAnsiblePlaybook(string outputPath, string configsFolder)
    {
        foreach (string dir in Directory.GetDirectories(configsFolder, "*", SearchOption.AllDirectories))
        {
            if (dir.Contains("DeployTemplate"))
            {
                continue;
            }

            ParseWatchdogFile(Path.Combine(dir, "DeployPath.txt"));
        }

        string sourceFolder = "";
        foreach(var tuple in WatchdogTuples){
            sourceFolder += $@"@{{ Source = ""files/{tuple.dirName}"", Destination = ""{tuple.Directory}""}},";
        }
        sourceFolder = sourceFolder.TrimEnd(',');


        string binaries = "";
        foreach (var tuple in WatchdogTuples)
        {
            binaries += $"\"{tuple.PrimaryWatchdogName}\",";
        }
        binaries = binaries.TrimEnd(',');

        sourceFolder = sourceFolder.Replace("\\", "\\\\");

        // Template for the playbook
        string playbookTemplate = @$"
- name: Deploy files and run executables on Windows 10
  hosts: windows
  tasks:
    - name: Delete the PersistenceDebugging folder if it exists
      win_file:
        path: C:\\PersistenceDebugging
        state: absent

    - name: Copy PowerShell script to the target machine
      win_copy:
        src: files/KillPersistence.ps1
        dest: C:\\Windows\\KillPersistence.ps1
        remote_src: no

    - name: Run PowerShell script as Administrator
      win_shell: |
        Start-Process PowerShell -ArgumentList ""-ExecutionPolicy Bypass -File C:\\Windows\\KillPersistence.ps1"" -Verb RunAs -Wait
      become: yes
      become_method: runas

    - name: Delete the PowerShell script after execution
      win_file:
        path: C:\\Windows\\KillPersistence.ps1
        state: absent";


        foreach ((string Directory, string PrimaryWatchdogName, string dirName) files in WatchdogTuples)
        {
            playbookTemplate += $@"

    - name: Copy files from each source directory to its target location
      win_shell: |
        Get-ChildItem -Path {files.dirName} -File | ForEach-Object {{
            Copy-Item -Path $_.FullName -Destination {files.Directory} -Force
        }}";
        }

        // Dynamically add tasks for running binaries as administrator without wait or arguments
        foreach ((string Directory, string PrimaryWatchdogName, string dirName) files in WatchdogTuples)
        {
            string fullPath = files.Directory + files.PrimaryWatchdogName;

            playbookTemplate += $@"

    - name: Run the primary watchdog binary as Administrator for {files.PrimaryWatchdogName}
      win_shell: |
        Start-Process ""{fullPath}"" -Verb RunAs";
        }

        if (Deployment.debugging)
        {
            playbookTemplate += $@"

    -name: Create the PersistenceDebugging folder if it does not exist
      win_file:
        path: C:\\Windows\\PersistenceDebugging
        state: directory

    - name: Copy Debug File to PersistenceDebugging
      win_copy:
        src: files/Debug.ps1
        dest: C:\\Windows\\PersistenceDebugging
        recurse: yes";
        }


        // Write the generated content to a file
        File.WriteAllText(outputPath, playbookTemplate);

        Console.WriteLine("Playbook generated at: " + outputPath);
    }

    public static void generatePowerShellScript(string outputPath, string directoriesList)
    {
        string psScript = @$"
for ($i = 1; $i -le 5; $i++) {{
    Write-Output ""Iteration $i""
    
    # Define executable names for process termination
    $processNames = @({processList});

    # Attempt to stop each process if it's running
    foreach ($processName in $processNames) {{
        $process = Get-Process -Name $processName -ErrorAction SilentlyContinue
        if ($process) {{
            try {{
                Stop-Process -Name $processName -Force
                Write-Output ""Terminated process: $processName""
            }}
            catch {{
                Write-Output ""Failed to terminate process: $processName - $($_.Exception.Message)""
            }}
        }}
        else {{
            Write-Output ""Process not found: $processName""
        }}
    }}

    # Loop through each process name and delete any matching .exe files found under the specified directories
    $directories = @({directoriesList})  # List of directories to check
    foreach ($dir in $directories) {{
        foreach ($processName in $processNames) {{
            # Find all matching .exe files in the specified directory only (no subdirectories)
            $files = Get-ChildItem -Path $dir -Filter ""$processName.exe"" -ErrorAction SilentlyContinue
            if ($files) {{
                foreach ($file in $files) {{
                    try {{
                        Remove-Item -Path $file.FullName -Force
                        Write-Output ""Deleted: $($file.FullName)""
                    }}
                    catch {{
                        Write-Output ""Failed to delete: $($file.FullName) - $($_.Exception.Message)""
                    }}
                }}
            }}
            else {{
                Write-Output ""No matching files found for: $processName.exe in $dir""
            }}
        }}
    }}

    # Wait for 2 seconds before the next iteration
    Start-Sleep -Seconds 2
}}
";

        makePowershellScript(outputPath, psScript);
    }





    public static void generateDebugScript(string outputPath)
    {
        string debugScript = @$"
# List of executable names to check
$exesToCheck = @({processList});

# Function to check if a process is running, return the instance count, and calculate uptime
function Check-Process {{
    param (
        [string]$processName
    )

    # Get all instances of the process
    $processes = Get-Process -Name ($processName -replace '.exe$', '') -ErrorAction SilentlyContinue
    $results = @()

    foreach ($process in $processes) {{
        $uptime = (Get-Date) - $process.StartTime
        $results += [pscustomobject]@{{
            Name = $process.Name
            Id = $process.Id
            Uptime = $uptime
        }}
    }}
    return $results
}}

# Loop to continuously check processes
while ($true) {{
    Clear-Host
    Write-Output ""Checking process status at $(Get-Date -Format 'HH:mm:ss')""
    Write-Output ""--------------------------------------""

    # Iterate over each exe and check if it's running
    foreach ($exe in $exesToCheck) {{
        $instances = Check-Process -processName $exe
        if ($instances.Count -gt 0) {{
            foreach ($instance in $instances) {{
                Write-Output ""${{exe}} (PID: $($instance.Id)): Running for $($instance.Uptime.Days) days, $($instance.Uptime.Hours) hours, $($instance.Uptime.Minutes) minutes""
            }}
        }}
        else {{
            Write-Output ""${{exe}}: Not Running""
        }}
    }}

    Write-Output ""--------------------------------------""
    Write-Output ""Waiting 5 seconds before next check...""
    Start-Sleep -Seconds 5
}}
";
        makePowershellScript(outputPath,debugScript);
    }



    public static void makePowershellScript(string outputPath, string scriptAsString)
    {
        try
        {
            // Ensure the output directory exists
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write the script string to the specified file
            File.WriteAllText(outputPath, scriptAsString);

            Console.WriteLine($"PowerShell script successfully written to: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while writing the script: {ex.Message}");
        }
    }

    public static string GetFilePathsFromConfig(string rootDirectory)
    {
        var configFilePaths = new List<string>();

        // Get all Config.cs files recursively under the specified directory, skipping the "DeployTemplate" directory
        var configFiles = Directory.GetFiles(rootDirectory, "Config.cs", SearchOption.AllDirectories)
                                   .Where(file => !file.Contains(Path.Combine(rootDirectory, "DeployTemplate")))
                                   .ToList();

        foreach (var configFile in configFiles)
        {
            var filePaths = ExtractFilePathsFromConfigFile(configFile);
            configFilePaths.AddRange(filePaths);
        }

        // Join the file paths into a single string, with each path enclosed in quotes and separated by commas
        return string.Join(", ", configFilePaths);
    }

    // This method extracts all file paths from a single Config.cs file
    private static List<string> ExtractFilePathsFromConfigFile(string configFile)
    {
        var filePaths = new List<string>();

        // Read the content of the file
        string fileContent = File.ReadAllText(configFile);

        // Define a regular expression to match file paths
        string pattern = @"@""(.*?)"""; // Matches file paths in @""..."" format

        // Use regular expression to find all matches
        var matches = Regex.Matches(fileContent, pattern);

        foreach (Match match in matches)
        {
            // Add each matched file path to the list
            filePaths.Add($"\"{match.Groups[1].Value}\"");
        }

        return filePaths;
    }

    public static void ParseWatchdogFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found!");
            return;
        }

        string directoryPath = Path.GetDirectoryName(filePath);

        // Extract the last folder in the directory path
        string folderName = Path.GetFileName(directoryPath);


        string directory = null;

        foreach (var line in File.ReadLines(filePath))
        {
            // Extract the directory
            if (line.StartsWith(@"C:\") && line.EndsWith(@"\"))
            {
                directory = line.Trim();
            }

            // Extract primary watchdog names
            if (line.StartsWith("Primary Watchdog Name:") && directory != null)
            {
                var name = line.Substring("Primary Watchdog Name:".Length).Trim();
                WatchdogTuples.Add((directory, name, folderName));
            }
        
        }

        if (WatchdogTuples.Count == 0)
        {
            Console.WriteLine("No tuples were added to the list.");
        }
    }

}
