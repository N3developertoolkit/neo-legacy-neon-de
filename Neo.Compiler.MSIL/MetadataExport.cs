using Mono.Cecil;
using Neo.Compiler.MSIL;
using System;
using static Neo.Compiler.MyJson;

namespace Neo.Compiler
{
    class MetadataExport
    {
        struct Metadata
        {
            public string Title;
            public string Description;
            public string Version;
            public string Author;
            public string Email;
            public bool HasStorage;
            public bool HasDynamicInvoke;
            public bool IsPayable;

            public Metadata(AssemblyDefinition asm)
            {
                Title = asm.Name.Name;
                Description = Title;
                Version = asm.Name.Version.ToString();
                Author = string.Empty;
                Email = string.Empty;
                HasDynamicInvoke = false;
                HasStorage = false;
                IsPayable = false;
            }

            public JsonNode_Object ToJson()
            {
                var json = new JsonNode_Object();
                json.SetDictValue("title", Title);
                json.SetDictValue("description", Description);
                json.SetDictValue("version", Version);
                json.SetDictValue("author", Author);
                json.SetDictValue("email", Email);
                json.SetDictValue("has-storage", HasStorage);
                json.SetDictValue("has-dynamic-invoke", HasDynamicInvoke);
                json.SetDictValue("is-payable", IsPayable);
                return json;

            }
        }

        static void ProcessAssemblyMetadataAttribute(CustomAttribute attrib, ref Metadata md)
        {
            var value = attrib.ConstructorArguments[1].Value.ToString();

            bool ParseBoolValue()
            {
                return bool.TryParse(value, out var result) ? result : false;
            }

            switch (attrib.ConstructorArguments[0].Value.ToString())
            {
                case "ContractVersion":
                    md.Version = value;
                    break;
                case "ContractAuthor":
                    md.Author = value;
                    break;
                case "ContractEmail":
                    md.Email = value;
                    break;
                case "ContractHasStorage":
                    md.HasStorage = ParseBoolValue();
                    break;
                case "ContractHasDynamicInvoke":
                    md.HasDynamicInvoke = ParseBoolValue();
                    break;
                case "ContractIsPayable":
                    md.IsPayable = ParseBoolValue();
                    break;
            }
        }

        public static JsonNode_Object Export(ILModule _in)
        {
            var md = new Metadata(_in.module.Assembly);
            foreach (var attrib in _in.module.Assembly.CustomAttributes)
            {
                switch (attrib.AttributeType.FullName)
                {
                    case "System.Reflection.AssemblyTitleAttribute":
                        md.Title = attrib.ConstructorArguments[0].Value.ToString();
                        break;
                    case "System.Reflection.AssemblyDescriptionAttribute":
                        md.Description = attrib.ConstructorArguments[0].Value.ToString();
                        break;
                    case "System.Reflection.AssemblyCompanyAttribute":
                        md.Author = string.IsNullOrEmpty(md.Author)
                            ? attrib.ConstructorArguments[0].Value.ToString()
                            : md.Author;
                        break;
                    case "System.Reflection.AssemblyMetadataAttribute":
                        ProcessAssemblyMetadataAttribute(attrib, ref md);
                        break;
                }
            }
            return md.ToJson();
        }
    }
}
