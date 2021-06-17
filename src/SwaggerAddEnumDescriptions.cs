using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NetCoreEnumDocumentation
{ 
    // This feels a bit like black magic, but it works and is battle tested in a production system
    internal class SwaggerAddEnumDescriptions : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // add enum descriptions to result models
            try
            {
                KeyValuePair<string, OpenApiSchema>[] enumTypes = swaggerDoc.Components.Schemas.Where(x => x.Value?.Enum?.Count > 0).ToArray();
                foreach (KeyValuePair<string, OpenApiSchema> property in swaggerDoc.Components.Schemas)
                {
                    string key = property.Key;
                    IList<IOpenApiAny> propertyEnums = property.Value.Enum;
                    if (propertyEnums?.Count > 0)
                    {
                        property.Value.Description += "<br />" + DescribeEnum(propertyEnums, key);
                    }

                    foreach (KeyValuePair<string, OpenApiSchema> innerProperty in property.Value.Properties)
                    {
                        IList<IOpenApiAny> enums = GetUnderlyingEnumType(enumTypes, innerProperty, out key).Value?.Enum;
                        if (enums?.Count > 0)
                        {
                            innerProperty.Value.Description += "<br />" + DescribeEnum(enums, key);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // add enum descriptions to input parameters
            foreach (var pathItem in swaggerDoc.Paths.Values)
            {
                DescribeEnumParameters(pathItem.Operations, swaggerDoc);
            }
        }

        private static KeyValuePair<string, OpenApiSchema> GetUnderlyingEnumType(
            KeyValuePair<string, OpenApiSchema>[] enumTypes, KeyValuePair<string, OpenApiSchema> property,
            out string key)
        {
            KeyValuePair<string, OpenApiSchema> underlyingEnumType =
                enumTypes.FirstOrDefault(x => property.Value.AllOf.Any(z => z.Reference.Id.Equals(x.Key)));
            key = underlyingEnumType.Key;
            return underlyingEnumType;
        }

        private void DescribeEnumParameters(IDictionary<OperationType, OpenApiOperation> operations,
            OpenApiDocument swaggerDoc)
        {
            if (operations != null)
            {
                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in operations)
                {
                    foreach (OpenApiParameter param in operation.Value.Parameters)
                    {
                        KeyValuePair<string, OpenApiSchema> paramEnum = swaggerDoc.Components.Schemas.FirstOrDefault(x => x.Key == param.Name);
                        if (paramEnum.Value != null)
                        {
                            param.Description += DescribeEnum(paramEnum.Value.Enum, paramEnum.Key);
                        }
                    }
                }
            }
        }

        private Type GetEnumTypeByName(string enumTypeName)
        {
            // This is required if we have any enum type that clashes in naming with builtin .NET types
            // I have experienced having an enum called Label which also exists in .NET. This little thing works around that issue
            return AppDomain.CurrentDomain
                .GetAssemblies().Where(x => x.FullName.Contains("NetCoreEnumDocumentation"))
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.Name == enumTypeName && x.FullName?.Contains("NetCoreEnumDocumentation") == true);
        }

        private string DescribeEnum(IList<IOpenApiAny> enums, string propertyTypeName)
        {
            List<string> enumDescriptions = new List<string>();
            Type enumType = GetEnumTypeByName(propertyTypeName);
            if (enumType == null)
                return null;

            foreach (IOpenApiAny openApiAny in enums)
            {
                // TODO This is a new modification to support strings. Should probably be refactored to make it more readable
                if (openApiAny is OpenApiString enumString)
                {
                    string name = enumString.Value;
                    string desc = GetDescription(enumType, name);
                    enumDescriptions.Add($"{name} = {desc}");
                    continue;
                }
                
                OpenApiInteger enumOption = (OpenApiInteger) openApiAny;
                int enumInt = enumOption.Value;
                string enumName;

                try
                {
                    enumName = Enum.GetName(enumType, enumInt);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Failed to get name of enum: {enumType}", e);
                }

                string description = GetDescription(enumType, enumName);
                enumDescriptions.Add($"{enumInt} = {enumName} ({description})");
            }

            return string.Join("<br />", enumDescriptions.ToArray());
        }

        private static string GetDescription(Type enumType, string name)
        {
            var attribute = enumType.GetMember(name)
                .FirstOrDefault(x => x.DeclaringType == enumType)?.GetCustomAttribute(typeof(DescriptionAttribute));

            return attribute is DescriptionAttribute description ? description.Description : null;
        }
    }
}