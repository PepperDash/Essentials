using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;

namespace CrestronMockGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Crestron Mock Library Generator");
            Console.WriteLine("================================");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: CrestronMockGenerator <input-assembly-path> <output-directory>");
                Console.WriteLine("Example: CrestronMockGenerator /path/to/SimplSharp.dll /output/mocks");
                return;
            }

            var inputPath = args[0];
            var outputDir = args[1];

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Error: Assembly file not found: {inputPath}");
                return;
            }

            Directory.CreateDirectory(outputDir);

            try
            {
                // Analyze the assembly
                Console.WriteLine($"Analyzing assembly: {inputPath}");
                var analyzer = new AssemblyAnalyzer(inputPath);
                var assemblyInfo = analyzer.AnalyzeAssembly();

                Console.WriteLine($"Found {assemblyInfo.Types.Count} types");

                // Save assembly metadata as JSON
                var metadataPath = Path.Combine(outputDir, $"{assemblyInfo.Name}.metadata.json");
                var json = JsonConvert.SerializeObject(assemblyInfo, Formatting.Indented);
                File.WriteAllText(metadataPath, json);
                Console.WriteLine($"Saved metadata to: {metadataPath}");

                // Generate mock classes
                var generator = new MockGenerator();
                var generatedFiles = new List<string>();

                // Group types by namespace
                var namespaceGroups = assemblyInfo.Types.GroupBy(t => t.Namespace);

                foreach (var namespaceGroup in namespaceGroups)
                {
                    var namespacePath = Path.Combine(outputDir, namespaceGroup.Key.Replace('.', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(namespacePath);

                    foreach (var typeInfo in namespaceGroup)
                    {
                        try
                        {
                            Console.WriteLine($"Generating mock for: {typeInfo.FullName}");
                            var mockCode = generator.GenerateMockClass(typeInfo);
                            
                            var fileName = $"{typeInfo.Name}.cs";
                            var filePath = Path.Combine(namespacePath, fileName);
                            
                            File.WriteAllText(filePath, mockCode);
                            generatedFiles.Add(filePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error generating mock for {typeInfo.Name}: {ex.Message}");
                        }
                    }
                }

                // Generate project file
                GenerateProjectFile(outputDir, assemblyInfo.Name);

                Console.WriteLine($"\nGeneration complete!");
                Console.WriteLine($"Generated {generatedFiles.Count} mock files");
                Console.WriteLine($"Output directory: {outputDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        static void GenerateProjectFile(string outputDir, string assemblyName)
        {
            var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>" + assemblyName + @".Mock</AssemblyName>
    <RootNamespace>" + assemblyName + @"</RootNamespace>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>" + assemblyName + @".Mock</PackageId>
    <Version>1.0.0</Version>
    <Description>Mock implementation of " + assemblyName + @" for testing</Description>
  </PropertyGroup>

</Project>";

            var projectPath = Path.Combine(outputDir, $"{assemblyName}.Mock.csproj");
            File.WriteAllText(projectPath, projectContent);
            Console.WriteLine($"Generated project file: {projectPath}");
        }
    }
}