using System.Text.RegularExpressions;

namespace Chaos;

public static partial class ProtobufParser
{
    // 匹配 proto 文件名，提取方向、Opcode起始值、模块名
    private static readonly Regex ProtoFileNameRegex = ProtoFileNameRegexGenerator();

    private static readonly Regex MessageRegex = MessageRegexGenerator();

    private static readonly Regex FieldRegex = FieldRegexGenerator();

    private static readonly Regex MapTypeRegex = MapTypeRegexGenerator();

    public static ProtoInfo? Parse(string path)
    {
        var fileName = Path.GetFileName(path);

        if (!TryParseProtoInfo(fileName, out var protoInfo) || protoInfo is null)
        {
            return null;
        }

        var start = protoInfo.OpcodeStart;
        var content = File.ReadAllText(path);

        foreach (Match msgMatch in MessageRegex.Matches(content))
        {
            var messageInfo = new MessageInfo
            (
                msgMatch.Groups["Name"].Value,
                msgMatch.Groups["Interface"].Value.Trim(),
                [],
                msgMatch.Groups["ResponseType"].Value.Trim(),
                start++
            );

            var fieldsText = msgMatch.Groups["Fields"].Value;
            foreach (Match fieldMatch in FieldRegex.Matches(fieldsText))
            {
                if (string.IsNullOrWhiteSpace(fieldMatch.Groups["Name"].Value))
                {
                    continue;
                }

                var mapMatch = MapTypeRegex.Match(fieldMatch.Groups["Type"].Value.Trim());

                messageInfo.Fields.Add(new FieldInfo
                (
                    fieldMatch.Groups["Modifier"].Value.Trim(),
                    fieldMatch.Groups["Type"].Value.Trim(),
                    fieldMatch.Groups["Name"].Value,
                    int.Parse(fieldMatch.Groups["Order"].Value),
                    fieldMatch.Groups["Comment"].Value.Trim(),
                    mapMatch.Success ? mapMatch.Groups["Key"].Value.Trim() : string.Empty,
                    mapMatch.Success ? mapMatch.Groups["Value"].Value.Trim() : string.Empty
                ));
            }

            protoInfo.Messages.Add(messageInfo);
        }

        return protoInfo;
    }

    private static bool TryParseProtoInfo(string input, out ProtoInfo? info)
    {
        var match = ProtoFileNameRegex.Match(input);
        if (!match.Success || !Enum.TryParse<ProtoType>(match.Groups["Type"].Value, out var type) || !ushort.TryParse(match.Groups["OpcodeStart"].Value, out var start) || match.Groups["Name"].Value is not { Length: > 0 } name)
        {
            info = null;
            return false;
        }

        info = new ProtoInfo(type, start, name, []);
        return true;
    }

    public sealed record FieldInfo(string Modifier, string Type, string Name, int Order, string Comment, string MapKey, string MapValue)
    {
        public readonly string Modifier = Modifier;
        public readonly string Type = Type;
        public readonly string Name = Name;
        public readonly int Order = Order;
        public readonly string Comment = Comment;

        public readonly string MapKey = MapKey;
        public readonly string MapValue = MapValue;

        public bool IsRepeated => Modifier == "repeated";
        public bool IsOptional => Modifier == "optional";
        public bool IsRequired => Modifier == "required";
        public bool IsMap => string.IsNullOrEmpty(Modifier) && !string.IsNullOrEmpty(MapKey) && !string.IsNullOrEmpty(MapValue);

        public bool HasComment => !string.IsNullOrEmpty(Comment);
    }

    public sealed record MessageInfo(string Name, string Interface, List<FieldInfo> Fields, string ResponseType, ushort Opcode)
    {
        public readonly string Name = Name;
        public readonly string Interface = Interface;
        public readonly List<FieldInfo> Fields = Fields;
        public readonly string ResponseType = ResponseType;
        public readonly ushort Opcode = Opcode;

        public bool HasResponse => !string.IsNullOrEmpty(ResponseType);
        public bool HasInterface => !string.IsNullOrEmpty(Interface);
    }

    public enum ProtoType
    {
        None,
        Client,
        Server
    }

    public sealed record ProtoInfo(ProtoType ProtoType, ushort OpcodeStart, string Name, List<MessageInfo> Messages)
    {
        public readonly ProtoType ProtoType = ProtoType;
        public readonly ushort OpcodeStart = OpcodeStart;
        public readonly string Name = Name;
        public readonly List<MessageInfo> Messages = Messages;
    }

    [GeneratedRegex(@"^\s*(?:(?<Modifier>repeated|optional|required)\s+)?(?<Type>[\w.<>,\s]+?)\s+(?<Name>\w+)\s*=\s*(?<Order>\d+)\s*(?:\[[^\]]*\])?\s*;\s*(?://\s*(?<Comment>.*?))?\s*$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex FieldRegexGenerator();

    [GeneratedRegex(@"(?:(?://\s*ResponseType\s+(?<ResponseType>\w+)\s*\r?\n\s*))?message\s+(?<Name>\w+)\s*(?://\s*(?<Interface>[\w/]+))?\s*\{(?<Fields>[\s\S]*?)^\s*\}", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MessageRegexGenerator();

    [GeneratedRegex(@"^(?<Type>Client|Server)_(?<OpcodeStart>\d+)_(?<Name>[^.]+)\.proto$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
    private static partial Regex ProtoFileNameRegexGenerator();

    [GeneratedRegex(@"^map\s*<\s*(?<Key>[\w.]+)\s*,\s*(?<Value>[\w.<>,\s]+?)\s*>$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
    private static partial Regex MapTypeRegexGenerator();
}