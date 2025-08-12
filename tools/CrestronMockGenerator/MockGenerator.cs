using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CrestronMockGenerator
{
    public class MockGenerator
    {
        public string GenerateMockClass(TypeInfo typeInfo)
        {
            var compilationUnit = CompilationUnit();

            // Add usings
            compilationUnit = compilationUnit.AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Collections.Generic")),
                UsingDirective(ParseName("System.Linq"))
            );

            // Create namespace
            var namespaceDeclaration = NamespaceDeclaration(ParseName(typeInfo.Namespace));

            // Add XML documentation if available
            var trivia = new List<SyntaxTrivia>();
            if (!string.IsNullOrEmpty(typeInfo.Documentation))
            {
                trivia.Add(Comment($"/// <summary>"));
                trivia.Add(Comment($"/// {typeInfo.Documentation}"));
                trivia.Add(Comment($"/// </summary>"));
            }

            // Create type declaration
            MemberDeclarationSyntax typeDeclaration;
            
            if (typeInfo.IsEnum)
            {
                typeDeclaration = GenerateEnum(typeInfo);
            }
            else if (typeInfo.IsInterface)
            {
                typeDeclaration = GenerateInterface(typeInfo);
            }
            else
            {
                typeDeclaration = GenerateClass(typeInfo);
            }

            if (trivia.Any())
            {
                typeDeclaration = typeDeclaration.WithLeadingTrivia(trivia);
            }

            namespaceDeclaration = namespaceDeclaration.AddMembers(typeDeclaration);
            compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);

            // Format the code
            var workspace = new AdhocWorkspace();
            var formattedNode = Microsoft.CodeAnalysis.Formatting.Formatter.Format(
                compilationUnit, 
                workspace);

            return formattedNode.ToFullString();
        }

        private EnumDeclarationSyntax GenerateEnum(TypeInfo typeInfo)
        {
            var enumDeclaration = EnumDeclaration(typeInfo.Name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            foreach (var field in typeInfo.Fields)
            {
                var member = EnumMemberDeclaration(field.Name);
                
                if (!string.IsNullOrEmpty(field.Value))
                {
                    member = member.WithEqualsValue(
                        EqualsValueClause(ParseExpression(field.Value)));
                }

                if (!string.IsNullOrEmpty(field.Documentation))
                {
                    member = member.WithLeadingTrivia(
                        Comment($"/// <summary>"),
                        Comment($"/// {field.Documentation}"),
                        Comment($"/// </summary>"));
                }

                enumDeclaration = enumDeclaration.AddMembers(member);
            }

            return enumDeclaration;
        }

        private InterfaceDeclarationSyntax GenerateInterface(TypeInfo typeInfo)
        {
            var interfaceDeclaration = InterfaceDeclaration(typeInfo.Name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            // Add base interfaces
            if (typeInfo.Interfaces.Any())
            {
                var baseList = BaseList();
                foreach (var baseInterface in typeInfo.Interfaces)
                {
                    var typeName = GetSimpleTypeName(baseInterface);
                    baseList = baseList.AddTypes(SimpleBaseType(ParseTypeName(typeName)));
                }
                interfaceDeclaration = interfaceDeclaration.WithBaseList(baseList);
            }

            // Add properties
            foreach (var property in typeInfo.Properties)
            {
                var propertyDeclaration = GenerateInterfaceProperty(property);
                interfaceDeclaration = interfaceDeclaration.AddMembers(propertyDeclaration);
            }

            // Add methods
            foreach (var method in typeInfo.Methods)
            {
                var methodDeclaration = GenerateInterfaceMethod(method);
                interfaceDeclaration = interfaceDeclaration.AddMembers(methodDeclaration);
            }

            // Add events
            foreach (var evt in typeInfo.Events)
            {
                var eventDeclaration = GenerateInterfaceEvent(evt);
                interfaceDeclaration = interfaceDeclaration.AddMembers(eventDeclaration);
            }

            return interfaceDeclaration;
        }

        private ClassDeclarationSyntax GenerateClass(TypeInfo typeInfo)
        {
            var classDeclaration = ClassDeclaration(typeInfo.Name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            if (typeInfo.IsAbstract)
            {
                classDeclaration = classDeclaration.AddModifiers(Token(SyntaxKind.AbstractKeyword));
            }
            else if (typeInfo.IsSealed)
            {
                classDeclaration = classDeclaration.AddModifiers(Token(SyntaxKind.SealedKeyword));
            }

            // Add base class and interfaces
            var baseTypes = new List<string>();
            if (!string.IsNullOrEmpty(typeInfo.BaseType) && typeInfo.BaseType != "System.Object")
            {
                baseTypes.Add(typeInfo.BaseType);
            }
            baseTypes.AddRange(typeInfo.Interfaces);

            if (baseTypes.Any())
            {
                var baseList = BaseList();
                foreach (var baseType in baseTypes)
                {
                    var typeName = GetSimpleTypeName(baseType);
                    baseList = baseList.AddTypes(SimpleBaseType(ParseTypeName(typeName)));
                }
                classDeclaration = classDeclaration.WithBaseList(baseList);
            }

            // Add properties
            foreach (var property in typeInfo.Properties)
            {
                var propertyDeclaration = GenerateProperty(property);
                classDeclaration = classDeclaration.AddMembers(propertyDeclaration);
            }

            // Add methods
            foreach (var method in typeInfo.Methods)
            {
                var methodDeclaration = GenerateMethod(method, typeInfo.IsAbstract);
                classDeclaration = classDeclaration.AddMembers(methodDeclaration);
            }

            // Add events
            foreach (var evt in typeInfo.Events)
            {
                var eventDeclaration = GenerateEvent(evt);
                classDeclaration = classDeclaration.AddMembers(eventDeclaration);
            }

            return classDeclaration;
        }

        private PropertyDeclarationSyntax GenerateProperty(PropertyInfo property)
        {
            var typeName = GetSimpleTypeName(property.Type);
            var propertyDeclaration = PropertyDeclaration(ParseTypeName(typeName), property.Name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            if (property.IsStatic)
            {
                propertyDeclaration = propertyDeclaration.AddModifiers(Token(SyntaxKind.StaticKeyword));
            }

            var accessors = new List<AccessorDeclarationSyntax>();

            if (property.CanRead)
            {
                accessors.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }

            if (property.CanWrite)
            {
                accessors.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }

            propertyDeclaration = propertyDeclaration.WithAccessorList(
                AccessorList(List(accessors)));

            if (!string.IsNullOrEmpty(property.Documentation))
            {
                propertyDeclaration = propertyDeclaration.WithLeadingTrivia(
                    Comment($"/// <summary>"),
                    Comment($"/// {property.Documentation}"),
                    Comment($"/// </summary>"));
            }

            return propertyDeclaration;
        }

        private PropertyDeclarationSyntax GenerateInterfaceProperty(PropertyInfo property)
        {
            var typeName = GetSimpleTypeName(property.Type);
            var propertyDeclaration = PropertyDeclaration(ParseTypeName(typeName), property.Name);

            var accessors = new List<AccessorDeclarationSyntax>();

            if (property.CanRead)
            {
                accessors.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }

            if (property.CanWrite)
            {
                accessors.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }

            propertyDeclaration = propertyDeclaration.WithAccessorList(
                AccessorList(List(accessors)));

            if (!string.IsNullOrEmpty(property.Documentation))
            {
                propertyDeclaration = propertyDeclaration.WithLeadingTrivia(
                    Comment($"/// <summary>"),
                    Comment($"/// {property.Documentation}"),
                    Comment($"/// </summary>"));
            }

            return propertyDeclaration;
        }

        private MethodDeclarationSyntax GenerateMethod(MethodInfo method, bool isAbstractClass)
        {
            var returnTypeName = GetSimpleTypeName(method.ReturnType);
            var methodDeclaration = MethodDeclaration(ParseTypeName(returnTypeName), method.Name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            if (method.IsStatic)
            {
                methodDeclaration = methodDeclaration.AddModifiers(Token(SyntaxKind.StaticKeyword));
            }
            else if (method.IsAbstract && isAbstractClass)
            {
                methodDeclaration = methodDeclaration.AddModifiers(Token(SyntaxKind.AbstractKeyword));
            }
            else if (method.IsVirtual)
            {
                methodDeclaration = methodDeclaration.AddModifiers(Token(SyntaxKind.VirtualKeyword));
            }

            // Add parameters
            var parameters = new List<ParameterSyntax>();
            foreach (var param in method.Parameters)
            {
                var paramTypeName = GetSimpleTypeName(param.Type);
                var parameter = Parameter(Identifier(param.Name))
                    .WithType(ParseTypeName(paramTypeName));

                if (param.IsOut)
                {
                    parameter = parameter.AddModifiers(Token(SyntaxKind.OutKeyword));
                }
                else if (param.IsRef)
                {
                    parameter = parameter.AddModifiers(Token(SyntaxKind.RefKeyword));
                }

                if (param.HasDefaultValue && !string.IsNullOrEmpty(param.DefaultValue))
                {
                    parameter = parameter.WithDefault(
                        EqualsValueClause(ParseExpression(param.DefaultValue)));
                }

                parameters.Add(parameter);
            }

            methodDeclaration = methodDeclaration.WithParameterList(
                ParameterList(SeparatedList(parameters)));

            // Add body or semicolon
            if (method.IsAbstract && isAbstractClass)
            {
                methodDeclaration = methodDeclaration.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                var statements = new List<StatementSyntax>();
                
                // Add default implementation
                if (returnTypeName != "void")
                {
                    statements.Add(ParseStatement($"throw new NotImplementedException();"));
                }
                else
                {
                    statements.Add(ParseStatement($"// Mock implementation"));
                }

                methodDeclaration = methodDeclaration.WithBody(Block(statements));
            }

            if (!string.IsNullOrEmpty(method.Documentation))
            {
                methodDeclaration = methodDeclaration.WithLeadingTrivia(
                    Comment($"/// <summary>"),
                    Comment($"/// {method.Documentation}"),
                    Comment($"/// </summary>"));
            }

            return methodDeclaration;
        }

        private MethodDeclarationSyntax GenerateInterfaceMethod(MethodInfo method)
        {
            var returnTypeName = GetSimpleTypeName(method.ReturnType);
            var methodDeclaration = MethodDeclaration(ParseTypeName(returnTypeName), method.Name);

            // Add parameters
            var parameters = new List<ParameterSyntax>();
            foreach (var param in method.Parameters)
            {
                var paramTypeName = GetSimpleTypeName(param.Type);
                var parameter = Parameter(Identifier(param.Name))
                    .WithType(ParseTypeName(paramTypeName));

                if (param.IsOut)
                {
                    parameter = parameter.AddModifiers(Token(SyntaxKind.OutKeyword));
                }
                else if (param.IsRef)
                {
                    parameter = parameter.AddModifiers(Token(SyntaxKind.RefKeyword));
                }

                parameters.Add(parameter);
            }

            methodDeclaration = methodDeclaration
                .WithParameterList(ParameterList(SeparatedList(parameters)))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            if (!string.IsNullOrEmpty(method.Documentation))
            {
                methodDeclaration = methodDeclaration.WithLeadingTrivia(
                    Comment($"/// <summary>"),
                    Comment($"/// {method.Documentation}"),
                    Comment($"/// </summary>"));
            }

            return methodDeclaration;
        }

        private EventFieldDeclarationSyntax GenerateEvent(EventInfo evt)
        {
            var typeName = GetSimpleTypeName(evt.EventHandlerType);
            var eventDeclaration = EventFieldDeclaration(
                VariableDeclaration(ParseTypeName(typeName))
                    .AddVariables(VariableDeclarator(evt.Name)))
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            if (!string.IsNullOrEmpty(evt.Documentation))
            {
                eventDeclaration = eventDeclaration.WithLeadingTrivia(
                    Comment($"/// <summary>"),
                    Comment($"/// {evt.Documentation}"),
                    Comment($"/// </summary>"));
            }

            return eventDeclaration;
        }

        private EventFieldDeclarationSyntax GenerateInterfaceEvent(EventInfo evt)
        {
            var typeName = GetSimpleTypeName(evt.EventHandlerType);
            var eventDeclaration = EventFieldDeclaration(
                VariableDeclaration(ParseTypeName(typeName))
                    .AddVariables(VariableDeclarator(evt.Name)));

            if (!string.IsNullOrEmpty(evt.Documentation))
            {
                eventDeclaration = eventDeclaration.WithLeadingTrivia(
                    Comment($"/// <summary>"),
                    Comment($"/// {evt.Documentation}"),
                    Comment($"/// </summary>"));
            }

            return eventDeclaration;
        }

        private string GetSimpleTypeName(string fullTypeName)
        {
            // Convert full type names to simple names
            var typeMappings = new Dictionary<string, string>
            {
                ["System.Void"] = "void",
                ["System.String"] = "string",
                ["System.Int32"] = "int",
                ["System.Int64"] = "long",
                ["System.Int16"] = "short",
                ["System.UInt32"] = "uint",
                ["System.UInt64"] = "ulong",
                ["System.UInt16"] = "ushort",
                ["System.Byte"] = "byte",
                ["System.SByte"] = "sbyte",
                ["System.Boolean"] = "bool",
                ["System.Single"] = "float",
                ["System.Double"] = "double",
                ["System.Decimal"] = "decimal",
                ["System.Object"] = "object",
                ["System.Char"] = "char"
            };

            if (typeMappings.TryGetValue(fullTypeName, out var simpleName))
            {
                return simpleName;
            }

            // Handle generic types
            if (fullTypeName.Contains('`'))
            {
                // Simplified generic handling
                return "object";
            }

            // Return last part of the type name
            var lastDot = fullTypeName.LastIndexOf('.');
            return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
        }
    }
}