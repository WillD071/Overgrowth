using Newtonsoft.Json.Linq;
using ServiceStack.Text;
using System.Diagnostics;
using System.Reflection;

namespace watchdogHelper{
    public class watchdogHelper
    {

    public static bool IsMutexRunning(string mutexName)
    {
        bool isNewInstance;
        
        // Attempt to create a mutex with the specified name
        using (Mutex mutex = new Mutex(false, mutexName, out isNewInstance))
        {
            // If a new instance was created, it means no other instance was running
            if (isNewInstance)
            {
                // Release the mutex so it's not held by this check
                mutex.ReleaseMutex();
                return false;
            }
            else
            {
                // If we couldn't create a new instance, another instance is already running
                return true;
            }
        }
    }

    public static bool IsProcessRunning(string processName)
    {
        // Get a list of processes by name
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0;
    }

    public static void CheckBinary(string destinationPath, string name)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string sourcePath = Path.Combine(currentDirectory, name);
        string destPathBinary = Path.Combine(destinationPath, name);


        if(File.Exists(destPathBinary) && File.Exists(sourcePath)){
            return;
        } 
        else{
            CopyBinary(destPathBinary, sourcePath);
        }
    }

    public static void CopyBinary(string destPathBinary, string sourcePath){
        // Check if the .exe exists in the current directory
        if (File.Exists(sourcePath))
        {
            // Copy the .exe to the destination
            try
            {
                File.Copy(sourcePath, destPathBinary, overwrite: true);
                Console.WriteLine($"'{binaryName}' found and copied to {destPathBinary}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }
        else if(File.Exists(destPathBinary)){
            try
            {
                File.Copy(destPathBinary, sourcePath, overwrite: false);
                Console.WriteLine($"'{binaryName}' found and copied to {destPathBinary}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"'{binaryName}' not found in the current directory.");
        }
    }

    public static void runBinary(string binaryPath, string Mutex, string arguments = "")
    {
        try
        {
            // Initialize the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = binaryPath,
                Arguments = arguments,
                UseShellExecute = false,   // Set to true if you want to use the system shell
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true      // Set to false if you want a window to appear
            };

            // Start the process
            using (Process process = Process.Start(startInfo))
            {
                // Capture output if needed
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();

                // Output the results
                Console.WriteLine("Output:");
                Console.WriteLine(output);

                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("Errors:");
                    Console.WriteLine(errors);
                }

                Console.WriteLine("Process exited with code " + process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run binary: {ex.Message}");
        }
    }

    public static void runWatchdog(string binaryPath, string arguments = "")
    {
            RegistryHelper.
            if(!IsMutexRunning(secondaryWatchdogMutexName)){
                CheckBinary(secondaryWatchdogPath, secondaryWatchdogName);
            }
    }

    public static void LoadFromJson(string jsonFilePath, params string[] keysToLoad)
    {
        try
        {
            // Read JSON file content
            var jsonContent = File.ReadAllText(jsonFilePath);
            var config = JObject.Parse(jsonContent);

            // Use reflection to get all static properties
            var properties = typeof(Config)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(p => keysToLoad.Contains(p.Name, StringComparer.OrdinalIgnoreCase));

            // Set values for each specified key
            foreach (var property in properties)
            {
                var jsonValue = config[property.Name];
                if (jsonValue != null && property.CanWrite)
                {
                    property.SetValue(null, jsonValue.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
        }
    }
    }
}