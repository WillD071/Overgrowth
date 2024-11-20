using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class GenerateScripts
    {

        public static void makeDeployFiles(string configPath, string OutputBins)
        {
            Directory.Delete(configPath);
            Directory.CreateDirectory(configPath);

            string filesPath = Path.Combine(configPath, "files");
            string tasksPath = Path.Combine(configPath, "tasks");
        string debugScriptsPath = Path.Combine(filesPath, "DebugScripts");


            Directory.CreateDirectory(filesPath);
            Directory.CreateDirectory(tasksPath);

            CopyFilesRecursively(OutputBins, filesPath);

            extractBinaryInfo(filesPath);

            Directory.CreateDirectory(debugScriptsPath); 

            generateAnsiblePlaybook(tasksPath);

            //generateDebugScripts(debugScriptsPath);



    }
    public static void extractBinaryInfo(string outputBins)
    {

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

    public static void generateAnsiblePlaybook(string outputPath)
        {

            string sourceFilesPath = @"C:\path\to\your\files";  // Adjust this path
            string ps1ScriptPath = @"C:\path\to\your\script.ps1";  // Adjust this path
            string targetFolderPath = @"C:\Windows\PersistenceDebugging";

            // Template for the playbook
            string playbookTemplate = @"
- name: Deploy files and run executables on Windows 10
  hosts: windows
  tasks:
    - name: Delete the PersistenceDebugging folder if it exists
      win_file:
        path: {{ target_folder }}
        state: absent

    - name: Create the PersistenceDebugging folder if it does not exist
      win_file:
        path: {{ target_folder }}
        state: directory

    - name: Copy directory of files to PersistenceDebugging
      win_copy:
        src: {{ source_files }}
        dest: {{ target_folder }}
        recurse: yes

    - name: Copy PowerShell script to the target machine
      win_copy:
        src: {{ ps1_script }}
        dest: {{ target_folder }}\script.ps1
        remote_src: no

    - name: Run PowerShell script as Administrator
      win_shell: |
        Start-Process PowerShell -ArgumentList ""-ExecutionPolicy Bypass -File {{ target_folder }}\script.ps1"" -Verb RunAs -Wait
      become: yes
      become_method: runas

    - name: Delete the PowerShell script after execution
      win_file:
        path: {{ target_folder }}\script.ps1
        state: absent

    - name: Copy files from each source directory to its target location
      win_shell: |
        $SourceFolders = @(
          @{ Source = ""C:\Source1""; Destination = ""C:\Target1"" },
          @{ Source = ""C:\Source2""; Destination = ""D:\Target2"" }
        )

        foreach ($Folder in $SourceFolders) {
          Get-ChildItem -Path $Folder.Source -File | ForEach-Object {
            Copy-Item -Path $_.FullName -Destination $Folder.Destination -Force
          }
        }

    - name: Run binaries as Administrator
      win_shell: |
        $Binaries = @(
          ""C:\Target1\binary1.exe"",
          ""D:\Target2\binary2.exe""
        )

        foreach ($Binary in $Binaries) {
          Start-Process -FilePath $Binary -ArgumentList ""/some-args"" -Verb RunAs -Wait
        }
      become: yes
      become_method: runas
";

            // Replace the placeholders with actual values
            string playbookContent = playbookTemplate
                .Replace("{{ source_files }}", sourceFilesPath)
                .Replace("{{ ps1_script }}", ps1ScriptPath)
                .Replace("{{ target_folder }}", targetFolderPath);

            // Specify the output file path

            // Write the generated content to a file
            File.WriteAllText(outputPath, playbookContent);

            Console.WriteLine("Playbook generated at: " + outputPath);
        }
    
}
