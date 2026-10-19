using System.Diagnostics.CodeAnalysis;
using IBussinesLogic;

namespace BusinessLogic;

[ExcludeFromCodeCoverage]
public class FileService : IFileService
{
    public void CopyFile(string sourcePath, string destinationPath)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.Copy(sourcePath, destinationPath, overwrite: true);
    }

    public int GetFileSizeInKb(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        return (int)(fileInfo.Length / 1024);
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }
}
