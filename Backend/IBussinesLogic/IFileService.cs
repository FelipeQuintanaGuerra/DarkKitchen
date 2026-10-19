namespace IBussinesLogic;

public interface IFileService
{
    void CopyFile(string sourcePath, string destinationPath);
    int GetFileSizeInKb(string filePath);
    bool FileExists(string filePath);
}
