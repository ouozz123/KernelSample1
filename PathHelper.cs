namespace KernelSample;
public class PathHelper
{
    public static string GetFullFilePath(string filePath)
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        return Path.Combine(projectDirectory, filePath);
    }
}
