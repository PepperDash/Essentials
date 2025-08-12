using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace CrestronMockGenerator
{
    public class AssemblyAnalyzer
    {
        private readonly string _assemblyPath;
        private readonly string _xmlDocPath;
        private Dictionary<string, string> _xmlDocumentation = new();

        public AssemblyAnalyzer(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
            _xmlDocPath = Path.ChangeExtension(assemblyPath, ".xml");
            
            if (File.Exists(_xmlDocPath))
            {
                LoadXmlDocumentation();
            }
        }

        private void LoadXmlDocumentation()
        {
            try
            {
                var doc = XDocument.Load(_xmlDocPath);
                var members = doc.Descendants("member");
                
                foreach (var member in members)
                {
                    var name = member.Attribute("name")?.Value;
                    var summary = member.Element("summary")?.Value?.Trim();
                    
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(summary))
                    {
                        _xmlDocumentation[name] = summary;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading XML documentation: {ex.Message}");
            }
        }

        public AssemblyInfo AnalyzeAssembly()
        {
            var assemblyInfo = new AssemblyInfo
            {
                Name = Path.GetFileNameWithoutExtension(_assemblyPath),
                Types = new List<TypeInfo>()
            };

            try
            {
                // Use MetadataLoadContext to load assembly without dependencies
                var resolver = new PathAssemblyResolver(new[] { _assemblyPath });
                var mlc = new MetadataLoadContext(resolver);
                
                var assembly = mlc.LoadFromAssemblyPath(_assemblyPath);
                
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (type.IsSpecialName || type.Name.Contains("<>"))
                        continue;

                    var typeInfo = AnalyzeType(type);
                    if (typeInfo != null)
                    {
                        assemblyInfo.Types.Add(typeInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing assembly: {ex.Message}");
            }

            return assemblyInfo;
        }

        private TypeInfo? AnalyzeType(Type type)
        {
            try
            {
                var typeInfo = new TypeInfo
                {
                    Name = type.Name,
                    Namespace = type.Namespace ?? "",
                    FullName = type.FullName ?? "",
                    IsInterface = type.IsInterface,
                    IsAbstract = type.IsAbstract,
                    IsSealed = type.IsSealed,
                    IsEnum = type.IsEnum,
                    IsClass = type.IsClass,
                    BaseType = type.BaseType?.FullName,
                    Documentation = GetDocumentation($"T:{type.FullName}"),
                    Properties = new List<PropertyInfo>(),
                    Methods = new List<MethodInfo>(),
                    Events = new List<EventInfo>(),
                    Fields = new List<FieldInfo>(),
                    Interfaces = type.GetInterfaces().Select(i => i.FullName).Where(n => n != null).ToList()
                };

                // Analyze properties
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    typeInfo.Properties.Add(new PropertyInfo
                    {
                        Name = prop.Name,
                        Type = prop.PropertyType.FullName ?? "",
                        CanRead = prop.CanRead,
                        CanWrite = prop.CanWrite,
                        IsStatic = prop.GetMethod?.IsStatic ?? prop.SetMethod?.IsStatic ?? false,
                        Documentation = GetDocumentation($"P:{type.FullName}.{prop.Name}")
                    });
                }

                // Analyze methods
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => !m.IsSpecialName))
                {
                    var parameters = method.GetParameters().Select(p => new ParameterInfo
                    {
                        Name = p.Name ?? "",
                        Type = p.ParameterType.FullName ?? "",
                        IsOut = p.IsOut,
                        IsRef = p.ParameterType.IsByRef && !p.IsOut,
                        HasDefaultValue = p.HasDefaultValue,
                        DefaultValue = p.HasDefaultValue ? p.DefaultValue?.ToString() : null
                    }).ToList();

                    typeInfo.Methods.Add(new MethodInfo
                    {
                        Name = method.Name,
                        ReturnType = method.ReturnType.FullName ?? "",
                        IsStatic = method.IsStatic,
                        IsVirtual = method.IsVirtual,
                        IsAbstract = method.IsAbstract,
                        Parameters = parameters,
                        Documentation = GetDocumentation($"M:{type.FullName}.{method.Name}")
                    });
                }

                // Analyze events
                foreach (var evt in type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    typeInfo.Events.Add(new EventInfo
                    {
                        Name = evt.Name,
                        EventHandlerType = evt.EventHandlerType?.FullName ?? "",
                        Documentation = GetDocumentation($"E:{type.FullName}.{evt.Name}")
                    });
                }

                // Analyze fields (for enums)
                if (type.IsEnum)
                {
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (field.Name == "value__") continue;
                        
                        typeInfo.Fields.Add(new FieldInfo
                        {
                            Name = field.Name,
                            Type = field.FieldType.FullName ?? "",
                            Value = field.GetRawConstantValue()?.ToString(),
                            Documentation = GetDocumentation($"F:{type.FullName}.{field.Name}")
                        });
                    }
                }

                return typeInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing type {type.Name}: {ex.Message}");
                return null;
            }
        }

        private string GetDocumentation(string memberName)
        {
            return _xmlDocumentation.TryGetValue(memberName, out var doc) ? doc : "";
        }
    }

    public class AssemblyInfo
    {
        public string Name { get; set; } = "";
        public List<TypeInfo> Types { get; set; } = new();
    }

    public class TypeInfo
    {
        public string Name { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsInterface { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
        public bool IsEnum { get; set; }
        public bool IsClass { get; set; }
        public string? BaseType { get; set; }
        public List<string> Interfaces { get; set; } = new();
        public string Documentation { get; set; } = "";
        public List<PropertyInfo> Properties { get; set; } = new();
        public List<MethodInfo> Methods { get; set; } = new();
        public List<EventInfo> Events { get; set; } = new();
        public List<FieldInfo> Fields { get; set; } = new();
    }

    public class PropertyInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool IsStatic { get; set; }
        public string Documentation { get; set; } = "";
    }

    public class MethodInfo
    {
        public string Name { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsAbstract { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = new();
        public string Documentation { get; set; } = "";
    }

    public class ParameterInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsOut { get; set; }
        public bool IsRef { get; set; }
        public bool HasDefaultValue { get; set; }
        public string? DefaultValue { get; set; }
    }

    public class EventInfo
    {
        public string Name { get; set; } = "";
        public string EventHandlerType { get; set; } = "";
        public string Documentation { get; set; } = "";
    }

    public class FieldInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Value { get; set; }
        public string Documentation { get; set; } = "";
    }
}