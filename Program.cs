

namespace GPOFolderRenamer
{
    using System;
    using System.IO;
    using System.Xml;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: YourProgram.exe <FolderPath> try in quotes");
                return;
            }

            string folderPath = args[0];

            if (Directory.Exists(folderPath))
            {
                ProcessFolders(folderPath);
            }
            else
            {
                Console.WriteLine("Specified folder does not exist.");
            }
            Console.ReadKey();
        }

        static string SanitizeFolderName(string name)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char invalidChar in invalidChars)
            {
                name = name.Replace(invalidChar, '_');
            }
            // You may want to add further sanitization logic here if needed.

            return name;
        }

        static void ProcessFolders(string folderPath)
        {
            foreach (var subfolder in Directory.GetDirectories(folderPath))
            {
                ProcessFolders(subfolder);

                // Check if there's a gpreport.xml file in the current subfolder
                string gpreportPath = Path.Combine(subfolder, "gpreport.xml");
                if (File.Exists(gpreportPath))
                {
                    try
                    {
                        // Load the XML document
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(gpreportPath);

                        // Use XPath to retrieve the "Name" element
                        XmlNode nameNode = xmlDoc.GetElementsByTagName("GPO")[0].ChildNodes[1];

                        if (nameNode != null)
                        {
                            string nameValue = nameNode.InnerText;

                            // Sanitize the folder name
                            nameValue = SanitizeFolderName(nameValue);

                            // Rename the containing folder to the sanitized "Name" element value
                            string newFolderPath = Path.Combine(Path.GetDirectoryName(subfolder), nameValue);

                            if (!Directory.Exists(newFolderPath))
                            {
                                Directory.Move(subfolder, newFolderPath);
                                Console.WriteLine($"Renamed folder '{subfolder}' to '{nameValue}'");
                            }
                            else
                            {
                                Console.WriteLine($"Folder '{newFolderPath}' already exists. Rename skipped.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("The 'Name' element was not found in the XML. Rename skipped.");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error processing '{gpreportPath}': {e.Message}");
                    }
                }
            }
        }
    }


}
