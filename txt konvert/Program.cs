using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

class Figure
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Figure(string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return $"Name: {Name}, Width: {Width}, Height: {Height}";
    }
}

class FigureModel
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public FigureModel() { }  // Parameterless constructor for deserialization

    public FigureModel(Figure figure)
    {
        Name = figure.Name;
        Width = figure.Width;
        Height = figure.Height;
    }
}

class FileManager
{
    private readonly string filePath;

    public FileManager(string filePath)
    {
        this.filePath = filePath;
    }

    private List<string> LoadTxt()
    {
        return new List<string>(File.ReadAllLines(filePath));
    }

    private List<string> LoadJson()
    {
        string jsonContent = File.ReadAllText(filePath);
        List<FigureModel> figures = JsonSerializer.Deserialize<List<FigureModel>>(jsonContent);

        var lines = new List<string>();
        foreach (var figure in figures)
        {
            lines.Add($"{figure.Name}: {figure.Width}: {figure.Height}");
        }
        return lines;
    }

    private List<string> LoadXml()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);

        var lines = new List<string>();
        foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
        {
            lines.Add($"{node.Name}: {node.InnerText}");
        }
        return lines;
    }

    public List<string> LoadFile()
    {
        if (filePath.EndsWith(".txt"))
        {
            return LoadTxt();
        }
        else if (filePath.EndsWith(".json"))
        {
            return LoadJson();
        }
        else if (filePath.EndsWith(".xml"))
        {
            return LoadXml();
        }
        else
        {
            throw new NotSupportedException("Unsupported file format");
        }
    }

    private void SaveTxt(List<string> data)
    {
        File.WriteAllLines(filePath, data);
    }

    private void SaveJson(List<string> data)
    {
        List<FigureModel> figures = new List<FigureModel>();

        foreach (var line in data)
        {
            string[] parts = line.Split(":");
            FigureModel figure = new FigureModel
            {
                Name = parts[0].Trim(),
                Width = int.Parse(parts[1].Trim()),
                Height = int.Parse(parts[2].Trim())
            };
            figures.Add(figure);
        }

        string jsonContent = JsonSerializer.Serialize(figures, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, jsonContent);
    }

    private void SaveXml(List<string> data)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement root = xmlDoc.CreateElement("root");
        xmlDoc.AppendChild(root);

        foreach (var line in data)
        {
            string[] parts = line.Split(":");
            XmlElement element = xmlDoc.CreateElement(parts[0].Trim());
            element.InnerText = parts[1].Trim();
            root.AppendChild(element);
        }

        xmlDoc.Save(filePath);
    }

    public void SaveFile(List<string> data)
    {
        if (filePath.EndsWith(".txt"))
        {
            SaveTxt(data);
        }
        else if (filePath.EndsWith(".json"))
        {
            SaveJson(data);
        }
        else if (filePath.EndsWith(".xml"))
        {
            SaveXml(data);
        }
        else
        {
            throw new NotSupportedException("Unsupported file format");
        }
    }
}

class Program
{
    static void Main()
    {
        Console.Write("Enter the file path: ");
        string filePath = Console.ReadLine();

        if (File.Exists(filePath))
        {
            RunProgram(filePath);
        }
        else
        {
            Console.WriteLine("File not found. Exiting program.");
        }
    }

    static void RunProgram(string filePath)
    {
        FileManager fileManager = new FileManager(filePath);

        try
        {
            List<string> data = fileManager.LoadFile();
            DisplayData(data);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.F1)
                {
                    fileManager.SaveFile(data);
                    Console.WriteLine("\nFile saved.");
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nExiting program.");
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    ChangeValue(data, true);
                    DisplayData(data);
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    ChangeValue(data, false);
                    DisplayData(data);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    static void DisplayData(List<string> data)
    {
        Console.Clear();
        foreach (var item in data)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine("\nPress F1 to save, Up/Down arrows to change values, Esc to exit.");
    }

    static void ChangeValue(List<string> data, bool increase)
    {
        Console.Clear();
        for (int i = 0; i < data.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {data[i]}");
        }

        Console.Write("\nEnter the number of the item to change: ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= data.Count)
        {
            string[] parts = data[choice - 1].Split(":");
            if (int.TryParse(parts[1].Trim(), out int value))
            {
                value = increase ? value + 1 : value - 1;
                data[choice - 1] = $"{parts[0].Trim()}: {value}";
            }
        }
    }
}
