using System.Text;
using Serilog;

namespace Chaos;

public static class WpfMessageBuilder
{
    private const string Output = "../../Native/Windows/Hybrid.Windows/Hybrid.Native.Windows";

    public static void Build()
    {
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

    public static void Build(ProtobufParser.ProtoInfo info)
    {
        var builder = new StringBuilder();

        builder.AppendLine("// ReSharper disable PropertyCanBeMadeInitOnly.Global");
        builder.AppendLine("// ReSharper disable UnusedAutoPropertyAccessor.Global");
        builder.AppendLine("namespace Hybrid.Native.Windows;");
        builder.AppendLine();

        var prefix = string.Empty;

        // Messages
        foreach (var message in info.Messages)
        {
            Build(message, builder, prefix);
        }

        // Opcode
        builder.AppendLine($"{prefix}public static class Opcode");
        builder.AppendLine($"{prefix}{{");

        foreach (var message in info.Messages)
        {
            builder.AppendLine($"{prefix}\tpublic const ushort {message.Name} = {message.Opcode};");
        }

        builder.AppendLine($"{prefix}}}");

        // Write To File
        var file = Path.Combine(Output, $"{info.Name}_{info.OpcodeStart}.cs");
        File.WriteAllText(file, builder.ToString(), Encoding.UTF8);
    }

    private static void Build(ProtobufParser.MessageInfo info, StringBuilder builder, string prefix)
    {
        builder.AppendLine($"{prefix}[Message(Opcode.{info.Name})]");

        if (info.HasResponse)
        {
            builder.AppendLine($"{prefix}[ResponseType(nameof({info.ResponseType}))]");
        }

        if (info.HasInterface)
        {
            builder.AppendLine($"{prefix}public sealed class {info.Name} : MessageObject, {info.Interface}");
        }
        else
        {
            builder.AppendLine($"{prefix}public sealed class {info.Name} : MessageObject");
        }


        builder.AppendLine($"{prefix}{{");

        // Fields
        foreach (var field in info.Fields)
        {
            if (field.HasComment)
            {
                builder.AppendLine($"{prefix}\t/// <summary>");
                builder.AppendLine($"{prefix}\t/// {field.Comment}");
                builder.AppendLine($"{prefix}\t/// </summary>");
            }

            if (field.IsMap)
            {
                builder.AppendLine($"{prefix}\tpublic required Dictionary<{ConvertType(field.MapKey)}, {ConvertType(field.MapValue)}> {ConvertName(field.Name)} {{ get; set; }} = new();");
            }
            else if (field.IsRepeated)
            {
                builder.AppendLine($"{prefix}\tpublic required List<{ConvertType(field.Type)}> {ConvertName(field.Name)} {{ get; set; }} = new();");
            }
            else
            {
                if (field.IsOptional)
                {
                    builder.AppendLine($"{prefix}\tpublic required {ConvertType(field.Type)}? {ConvertName(field.Name)} {{ get; set; }}");
                }
                else
                {
                    builder.AppendLine($"{prefix}\tpublic required {ConvertType(field.Type)} {ConvertName(field.Name)} {{ get; set; }}");
                }
            }
        }

        builder.AppendLine($"{prefix}}}");
    }

    private static string ConvertName(string name)
    {
        return string.Join(string.Empty, name.Split('_').Select(segment => segment.ToUpper()[0] + segment[1..]));
    }


    private static string ConvertType(string type)
    {
        return type switch
        {
            "int16" => "short",
            "uint16" => "ushort",
            "int32" => "int",
            "uint32" => "uint",
            "int64" => "long",
            "uint64" => "ulong",
            "bytes" => "byte[]",
            _ => type
        };
    }
}