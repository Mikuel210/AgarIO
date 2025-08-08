using System.Text.Json;

namespace PaperIO;

public static class PlayerPrefs
{
    public static string filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AgarIO", "data.txt"
    );

    static PlayerPrefs()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        } 
    }

    public static void Set(int value)
    {
        File.WriteAllText(filePath, value.ToString());
    }

    public static int Get()
    {
        try
        {
            return int.Parse(File.ReadAllText(filePath));
        }
        catch
        {
            return 0;
        }
    }
}