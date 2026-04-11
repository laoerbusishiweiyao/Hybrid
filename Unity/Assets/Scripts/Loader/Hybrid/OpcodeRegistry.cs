// using System;
// using System.Collections.Generic;
//
// namespace Chaos
// {
//     public sealed class OpcodeRegistry : IDisposable
//     {
//         public static readonly OpcodeRegistry Default = new();
//
//         private readonly Dictionary<ushort, Type> opcodeTypeMap = new();
//         private readonly Dictionary<Type, ushort> typeOpcodeMap = new();
//         private readonly Dictionary<Type, Type> requestResponseMap = new();
//
//         public OpcodeRegistry()
//         {
//             opcodeTypeMap.Add(Opcode.WebTouchData, typeof(WebTouchData));
//             typeOpcodeMap.Add(typeof(WebTouchData), Opcode.WebTouchData);
//
//             opcodeTypeMap.Add(Opcode.WebLoaded, typeof(WebLoaded));
//             typeOpcodeMap.Add(typeof(WebLoaded), Opcode.WebLoaded);
//
//             opcodeTypeMap.Add(Opcode.UnityInformationRequest, typeof(UnityInformationRequest));
//             typeOpcodeMap.Add(typeof(UnityInformationRequest), Opcode.UnityInformationRequest);
//
//             opcodeTypeMap.Add(Opcode.UnityInformationResponse, typeof(UnityInformationResponse));
//             typeOpcodeMap.Add(typeof(UnityInformationResponse), Opcode.UnityInformationResponse);
//
//             requestResponseMap.Add(typeof(UnityInformationRequest), typeof(UnityInformationResponse));
//             
//             opcodeTypeMap.Add(Opcode.UnityInitialized, typeof(UnityInitialized));
//             typeOpcodeMap.Add(typeof(UnityInitialized), Opcode.UnityInitialized);
//             
//             opcodeTypeMap.Add(Opcode.BrowserInformationRequest, typeof(BrowserInformationRequest));
//             typeOpcodeMap.Add(typeof(BrowserInformationRequest), Opcode.BrowserInformationRequest);
//
//             opcodeTypeMap.Add(Opcode.BrowserInformationResponse, typeof(BrowserInformationResponse));
//             typeOpcodeMap.Add(typeof(BrowserInformationResponse), Opcode.BrowserInformationResponse);
//             
//             requestResponseMap.Add(typeof(BrowserInformationRequest), typeof(BrowserInformationResponse));
//         }
//
//         public ushort FindOpcode(Type type)
//         {
//             return typeOpcodeMap.GetValueOrDefault(type);
//         }
//
//         public Type FindType(ushort opcode)
//         {
//             return opcodeTypeMap.GetValueOrDefault(opcode);
//         }
//
//         public Type FindResponseType(Type type)
//         {
//             return requestResponseMap.GetValueOrDefault(type);
//         }
//
//         public void Dispose()
//         {
//         }
//     }
// }