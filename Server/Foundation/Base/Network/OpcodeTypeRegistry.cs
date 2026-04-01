using System.Reflection;
using Serilog;

namespace Chaos;

public sealed class OpcodeTypeRegistry : Singleton<OpcodeTypeRegistry>, ISingletonAwake
{
    private readonly BiDictionary<Type, ushort> typeOpcode = new();

    private readonly Dictionary<Type, Type> requestResponse = new();

    public void Awake()
    {
        var types = CodeTypeRegistry.Default.GetTypes(typeof(MessageAttribute));
        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<MessageAttribute>(false);
            if (attribute == null)
            {
                continue;
            }

            var opcode = attribute.Opcode;
            if (opcode != 0)
            {
                this.typeOpcode.Add(type, opcode);
            }

            // 检查request response
            if (typeof(IRequest).IsAssignableFrom(type))
            {
                if (typeof(ILocationMessage).IsAssignableFrom(type))
                {
                    this.requestResponse.Add(type, typeof(MessageResponse));
                    continue;
                }

                var responseTypeAttribute = type.GetCustomAttribute<ResponseTypeAttribute>(false);
                if (responseTypeAttribute == null)
                {
                    Log.Error($"not found responseType: {type}");
                    continue;
                }

                this.requestResponse.Add(type, CodeTypeRegistry.Default.GetType($"Chaos.{responseTypeAttribute.Type}"));
            }
        }
    }

    public ushort GetOpcode(Type type)
    {
        var opcode = this.typeOpcode.GetValueOrDefault(type);
        if (opcode == 0)
        {
            throw new Exception($"OpcodeType not found opcode: {type.FullName}");
        }

        return opcode;
    }

    public Type GetType(ushort opcode)
    {
        var type = this.typeOpcode.GetKeyOrDefault(opcode);
        if (type == null)
        {
            throw new Exception($"OpcodeType not found type: {opcode}");
        }

        return type;
    }

    public Type GetResponseType(Type request)
    {
        if (!this.requestResponse.TryGetValue(request, out var response))
        {
            throw new Exception($"not found response type, request type: {request.FullName}");
        }

        return response;
    }
}