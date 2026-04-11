using System.Text;
using Serilog;

namespace Chaos;

public static class CSharpBuilder
{
    private const string ClientOutput = "../../Unity/Assets/Scripts/Model/Generated/Message";
    private const string ServerOutput = "../../Server/Model/Generated/Message";

    public static void Build()
    {
        #region 清空输出

        if (Directory.Exists(ClientOutput))
        {
            Directory.Delete(ClientOutput, true);
        }

        Directory.CreateDirectory(ClientOutput);

        if (Directory.Exists(ServerOutput))
        {
            Directory.Delete(ServerOutput, true);
        }

        Directory.CreateDirectory(ServerOutput);

        #endregion

        foreach (var protoFile in Directory.EnumerateFiles("../../Protobuf", "*.proto", SearchOption.AllDirectories))
        {
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

        builder.AppendLine("using MemoryPack;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();

        if (info.ProtoType is ProtobufParser.ProtoType.Server)
        {
            builder.AppendLine("namespace Chaos;");
        }
        else
        {
            builder.AppendLine("namespace Chaos");
            builder.AppendLine("{");
        }

        var prefix = info.ProtoType is ProtobufParser.ProtoType.Server ? string.Empty : "\t";

        // Messages
        foreach (var message in info.Messages)
        {
            Build(message, builder, prefix);
        }

        // Opcode
        builder.AppendLine($"{prefix}public static partial class Opcode");
        builder.AppendLine($"{prefix}{{");

        foreach (var message in info.Messages)
        {
            builder.AppendLine($"{prefix}\tpublic const ushort {message.Name} = {message.Opcode};");
        }

        builder.AppendLine($"{prefix}}}");

        if (info.ProtoType is not ProtobufParser.ProtoType.Server)
        {
            builder.AppendLine("}");
        }

        // Write To File
        var file = Path.Combine(info.ProtoType is ProtobufParser.ProtoType.Server ? ServerOutput : ClientOutput, $"{info.Name}_{info.OpcodeStart}.cs");
        File.WriteAllText(file, builder.ToString(), Encoding.UTF8);
    }

    private static void Build(ProtobufParser.MessageInfo info, StringBuilder builder, string prefix)
    {
        builder.AppendLine($"{prefix}[MemoryPackable]");
        builder.AppendLine($"{prefix}[Message(Opcode.{info.Name})]");

        if (info.HasResponse)
        {
            builder.AppendLine($"{prefix}[ResponseType(nameof({info.ResponseType}))]");
        }

        if (info.HasInterface)
        {
            builder.AppendLine($"{prefix}public sealed partial class {info.Name} : MessageObject, {info.Interface}");
        }
        else
        {
            builder.AppendLine($"{prefix}public sealed partial class {info.Name} : MessageObject");
        }


        builder.AppendLine($"{prefix}{{");

        // Create
        builder.AppendLine($"{prefix}\tpublic static {info.Name} Create(bool isFromPool = false)");
        builder.AppendLine($"{prefix}\t{{");
        builder.AppendLine($"{prefix}\t\treturn ObjectPool.Rent<{info.Name}>(isFromPool);");
        builder.AppendLine($"{prefix}\t}}");

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
                builder.AppendLine($"{prefix}\t[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]");
                builder.AppendLine($"{prefix}\t[MemoryPackOrder({field.Order})]");
                builder.AppendLine($"{prefix}\tpublic Dictionary<{ConvertType(field.MapKey)}, {ConvertType(field.MapValue)}> {ConvertName(field.Name)} {{ get; set; }} = new();");
            }
            else if (field.IsRepeated)
            {
                builder.AppendLine($"{prefix}\t[MemoryPackOrder({field.Order})]");
                builder.AppendLine($"{prefix}\tpublic List<{ConvertType(field.Type)}> {ConvertName(field.Name)} {{ get; set; }} = new();");
            }
            else
            {
                builder.AppendLine($"{prefix}\t[MemoryPackOrder({field.Order})]");
                if (field.IsOptional)
                {
                    builder.AppendLine($"{prefix}\tpublic {ConvertType(field.Type)}? {ConvertName(field.Name)} {{ get; set; }}");
                }
                else
                {
                    builder.AppendLine($"{prefix}\tpublic {ConvertType(field.Type)} {ConvertName(field.Name)} {{ get; set; }}");
                }
            }
        }

        // Dispose
        builder.AppendLine($"{prefix}\tpublic override void Dispose()");
        builder.AppendLine($"{prefix}\t{{");

        builder.AppendLine($"{prefix}\t\tif (!IsFromPool)");
        builder.AppendLine($"{prefix}\t\t{{");
        builder.AppendLine($"{prefix}\t\t\treturn;");
        builder.AppendLine($"{prefix}\t\t}}");

        // Fields Dispose
        foreach (var field in info.Fields)
        {
            if (field.IsRepeated || field.IsMap)
            {
                builder.AppendLine($"{prefix}\t\t{ConvertName(field.Name)}.Clear();");
            }
            else
            {
                builder.AppendLine($"{prefix}\t\t{ConvertName(field.Name)} = default;");
            }
        }

        builder.AppendLine($"{prefix}\t\tObjectPool.Recycle(this);");

        builder.AppendLine($"{prefix}\t}}");

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