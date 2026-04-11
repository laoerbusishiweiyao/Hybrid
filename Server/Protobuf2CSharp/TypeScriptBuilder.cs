using System.Text;
using Serilog;

namespace Chaos;

public static class TypeScriptBuilder
{
    private const string Output = "../../Web/unity-web-ui/src/runtime/generated/message";

    public static void Build()
    {
        #region 清空输出

        if (Directory.Exists(Output))
        {
            Directory.Delete(Output, true);
        }

        Directory.CreateDirectory(Output);

        #endregion

        foreach (var protoFile in Directory.EnumerateFiles("../../Protobuf", "*.proto", SearchOption.AllDirectories))
        {
            var name = Path.GetFileNameWithoutExtension(protoFile);
            if (!name.StartsWith("Client_") || !name.EndsWith("_Web"))
            {
                continue;
            }

            if (ProtobufParser.Parse(protoFile) is not { } info)
            {
                Log.Warning("Proto file {file} parse failed", protoFile);
                continue;
            }

            Build(info);
        }
    }

    private static void Build(ProtobufParser.ProtoInfo info)
    {
        var builder = new StringBuilder();

        // Messages
        builder.Clear();
        builder.AppendLine($"import {{ MessageObject, RequestObject, ResponseObject, {string.Join(", ", info.Messages.Select(message => message.Interface).Where(name => !string.IsNullOrEmpty(name)).Distinct().Select(name => $"type {name}"))} }} from '../../Message/IMessage';");
        builder.AppendLine();

        foreach (var message in info.Messages)
        {
            Build(message, builder);
        }

        File.WriteAllText(Path.Combine(Output, "Message.ts"), builder.ToString(), Encoding.UTF8);

        // Opcode
        builder.Clear();
        builder.AppendLine("export const Opcode = {");
        foreach (var message in info.Messages)
        {
            builder.AppendLine($"\t{message.Name}: {message.Opcode},");
        }

        builder.AppendLine("} as const;");
        File.WriteAllText(Path.Combine(Output, "Opcode.ts"), builder.ToString(), Encoding.UTF8);

        // OpcodeTypeRegistry
        builder.Clear();
        builder.AppendLine("import { type MessageType, type RequestType, type ResponseType } from '../../Message/IMessage';");
        builder.AppendLine($"import {{ {string.Join(", ", info.Messages.Select(message => message.Name))} }} from './Message';");
        builder.AppendLine();

        builder.AppendLine("""
                           class OpcodeTypeRegistryClass {
                               private readonly opcodeTypeMap = new Map<number, MessageType>();
                               private readonly typeOpcodeMap = new Map<MessageType, number>();
                               private readonly requestResponseMap = new Map<RequestType, ResponseType>();

                               constructor() {
                           """);
        foreach (var message in info.Messages)
        {
            builder.AppendLine($"\t\tthis.opcodeTypeMap.set({message.Opcode}, {message.Name});");
            builder.AppendLine($"\t\tthis.typeOpcodeMap.set({message.Name}, {message.Opcode});");
        }

        foreach (var message in info.Messages)
        {
            if (!message.HasResponse)
            {
                continue;
            }

            builder.AppendLine($"\t\tthis.requestResponseMap.set({message.Name}, {message.ResponseType});");
        }

        builder.AppendLine("""
                               }

                               findOpcode(type: MessageType) {
                                   return this.typeOpcodeMap.get(type);
                               }

                               findType(opcode: number) {
                                   return this.opcodeTypeMap.get(opcode);
                               }

                               findResponseType(requestType: RequestType) {
                                   return this.requestResponseMap.get(requestType);
                               }
                           }
                           """);

        builder.AppendLine();
        builder.AppendLine("export const OpcodeTypeRegistry = new OpcodeTypeRegistryClass();");

        File.WriteAllText(Path.Combine(Output, "OpcodeTypeRegistry.ts"), builder.ToString(), Encoding.UTF8);
    }

    private static void Build(ProtobufParser.MessageInfo info, StringBuilder builder)
    {
        List<string> ignoreFields = ["request_id", "status_code", "message"];

        if (info.HasInterface)
        {
            var parent = info.Interface switch
            {
                "IRequest" or "ISessionRequest" or "IWebRequest" => "RequestObject",
                "IResponse" or "ISessionResponse" or "IWebResponse" => "ResponseObject",
                _ => "MessageObject",
            };
            builder.AppendLine($"export class {info.Name} extends {parent} implements {info.Interface} {{");
        }
        else
        {
            builder.AppendLine($"export class {info.Name} extends MessageObject {{");
        }

        var fields = info.Fields.FindAll(field => !ignoreFields.Contains(field.Name));

        // Fields
        foreach (var field in fields)
        {
            if (field.HasComment)
            {
                builder.AppendLine("\t/**");
                builder.AppendLine($"\t * {field.Comment}");
                builder.AppendLine("\t */");
            }

            if (field.IsMap)
            {
                builder.AppendLine($"\t{ConvertName(field.Name)}!: Map<{ConvertType(field.MapKey)}, {ConvertType(field.MapValue)}>;");
            }
            else if (field.IsRepeated)
            {
                builder.AppendLine($"\t{ConvertName(field.Name)}!: Array<{ConvertType(field.Type)}>;");
            }
            else
            {
                if (field.IsOptional)
                {
                    builder.AppendLine($"\t{ConvertName(field.Name)}?: {ConvertType(field.Type)};");
                }
                else
                {
                    builder.AppendLine($"\t{ConvertName(field.Name)}!: {ConvertType(field.Type)};");
                }
            }
        }

        // Constructor
        if (fields.Count > 0)
        {
            builder.Append("\tconstructor(");
            builder.Append(string.Join(", ", fields.Select(field => $"{ConvertName(field.Name)}{(field.IsOptional ? "?" : "")}: {ConvertType(field.Type)}")));
            builder.AppendLine(") {");

            builder.AppendLine("\t\tsuper();");
            foreach (var field in fields)
            {
                builder.AppendLine($"\t\tthis.{ConvertName(field.Name)} = {ConvertName(field.Name)};");
            }

            builder.AppendLine("\t}");
        }

        builder.AppendLine("}");
    }

    private static string ConvertName(string name)
    {
        var segments = name.Split('_');
        return segments[0] + string.Join(string.Empty, segments.Skip(1).Select(segment => segment.ToUpper()[0] + segment[1..]));
    }

    private static string ConvertType(string type)
    {
        return type switch
        {
            "bool" => "boolean",

            "byte" or "sbyte" or "short" or "uint16" or "ushort" or "int" or "int32" or "uint" or "uint32" or "float" or "double" => "number",

            "long" or "int64" or "ulong" or "uint64" => "bigint",

            "string" => "string",

            "Guid" => "string",

            "DateTime" => "Date",

            "bytes" => "Uint8Array",

            _ => type
        };
    }
}