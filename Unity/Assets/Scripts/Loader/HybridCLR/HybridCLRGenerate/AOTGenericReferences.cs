using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"MemoryPack.Core.dll",
		"MongoDB.Bson.dll",
		"Serilog.dll",
		"System.Core.dll",
		"System.Runtime.CompilerServices.Unsafe.dll",
		"System.Text.Json.dll",
		"System.dll",
		"Unity.Foundation.dll",
		"UnityEngine.AndroidJNIModule.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Chaos.AwakeSystem<object,int>
	// Chaos.AwakeSystem<object,object,byte>
	// Chaos.AwakeSystem<object,object>
	// Chaos.AwakeSystem<object>
	// Chaos.BiDictionary<int,object>
	// Chaos.BsonStructSerializer<Unity.Mathematics.float2>
	// Chaos.BsonStructSerializer<Unity.Mathematics.float3>
	// Chaos.BsonStructSerializer<Unity.Mathematics.float4>
	// Chaos.BsonStructSerializer<Unity.Mathematics.quaternion>
	// Chaos.CodeMapper<object>
	// Chaos.DestroySystem<object>
	// Chaos.EntityReference<object>
	// Chaos.EventHandler<object,Chaos.AppStartInitializeFinishEventArgs>
	// Chaos.EventHandler<object,Chaos.EntryClientPhaseEventArgs>
	// Chaos.EventHandler<object,Chaos.EntrySharedPhaseEventArgs>
	// Chaos.EventHandler<object,Chaos.UnitChangePositionEventArgs>
	// Chaos.EventHandler<object,Chaos.UnitChangeRotationEventArgs>
	// Chaos.EventHandler<object,Chaos.WebUiInitializeFinishEventArgs>
	// Chaos.EventHandler<object,Chaos.WebUiMessageSentEventArgs>
	// Chaos.EventSystem<object,Chaos.UpdateEventArgs>
	// Chaos.IAwake<int>
	// Chaos.IAwake<object,byte>
	// Chaos.IAwake<object>
	// Chaos.IAwakeSystem<int>
	// Chaos.IAwakeSystem<object,byte>
	// Chaos.IAwakeSystem<object>
	// Chaos.IEvent<Chaos.UpdateEventArgs>
	// Chaos.ISingletonAwake<object>
	// Chaos.InvokeHandler<Chaos.FiberInitializeEventArgs,object>
	// Chaos.InvokeHandler<Chaos.MailboxInvokeEventArgs>
	// Chaos.InvokeHandler<Chaos.NetComponentOnReadEventArgs>
	// Chaos.InvokeHandler<Chaos.TimerCallback>
	// Chaos.Singleton<object>
	// Chaos.StateMachineWrapper<Chaos.AppStartInitializeFinishEventHandler.<RunAsync>d__0>
	// Chaos.StateMachineWrapper<Chaos.Entry.<StartAsync>d__1>
	// Chaos.StateMachineWrapper<Chaos.EntryClientPhaseEventHandler.<RunAsync>d__0>
	// Chaos.StateMachineWrapper<Chaos.EntrySharedPhaseEventHandler.<RunAsync>d__0>
	// Chaos.StateMachineWrapper<Chaos.FiberInitialize_Client.<Handle>d__0>
	// Chaos.StateMachineWrapper<Chaos.FiberInitialize_Sample.<Handle>d__0>
	// Chaos.StateMachineWrapper<Chaos.FiberInitialize_WebUI.<Handle>d__0>
	// Chaos.StateMachineWrapper<Chaos.MemoryMappedFileComponentSystem.<SendAsync>d__7>
	// Chaos.StateMachineWrapper<Chaos.MessageDispatcher.<HandleAsync>d__3>
	// Chaos.StateMachineWrapper<Chaos.MessageHandler.<Handle>d__1<object,object,object>>
	// Chaos.StateMachineWrapper<Chaos.MessageHandler.<Handle>d__1<object,object>>
	// Chaos.StateMachineWrapper<Chaos.ObjectWaitSystem.<WaitAsync>d__2<object>>
	// Chaos.StateMachineWrapper<Chaos.ProcessInnerMessageSenderInfo.<WaitAsync>d__13>
	// Chaos.StateMachineWrapper<Chaos.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<CallAsync>g__TimeoutAsync|0>d>
	// Chaos.StateMachineWrapper<Chaos.ProcessInnerSenderSystem.<CallAsync>d__10>
	// Chaos.StateMachineWrapper<Chaos.RequestInfo.<WaitAsync>d__5>
	// Chaos.StateMachineWrapper<Chaos.SessionMessageHandler.<HandleAsync>d__2<object,object>>
	// Chaos.StateMachineWrapper<Chaos.SessionMessageHandler.<HandleAsync>d__2<object>>
	// Chaos.StateMachineWrapper<Chaos.SessionSystem.<CallAsync>d__3>
	// Chaos.StateMachineWrapper<Chaos.SessionSystem.<CallAsync>d__4>
	// Chaos.StateMachineWrapper<Chaos.Web2UnityLoadedMessageHandler.<RunAsync>d__0>
	// Chaos.StateMachineWrapper<Chaos.Web2UnityVersionRequestHandler.<RunAsync>d__0>
	// Chaos.StateMachineWrapper<Chaos.WebMessageHandler.<HandleAsync>d__2<object,object>>
	// Chaos.StateMachineWrapper<Chaos.WebMessageHandler.<HandleAsync>d__2<object>>
	// Chaos.StateMachineWrapper<Chaos.WebUiComponentSystem.<SendAsync>d__4>
	// Chaos.StateMachineWrapper<Chaos.WebUiInitializeFinishEventHandler.<RunAsync>d__0>
	// Chaos.StateMachineWrapper<Chaos.WebUiMessageSentEventHandler.<RunAsync>d__0>
	// Chaos.StateMachineWrapper<Chaos.WebViewComponentSystem.<SendAsync>d__8>
	// Chaos.StateMachineWrapper<Chaos.Wpf2UnityLoadedMessageHandler.<RunAsync>d__0>
	// Chaos.ThreadTask<int>
	// Chaos.ThreadTask<object>
	// Chaos.ThreadTaskAsyncMethodBuilder<int>
	// Chaos.ThreadTaskAsyncMethodBuilder<object>
	// Chaos.Timer<object>
	// Chaos.UpdateSystem<object>
	// MemoryPack.Formatters.ArrayFormatter<object>
	// MemoryPack.Formatters.NullableFormatter<int>
	// MemoryPack.IMemoryPackable<object>
	// MemoryPack.MemoryPackFormatter<System.Nullable<int>>
	// MemoryPack.MemoryPackFormatter<object>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.float2>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.float3>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.float4>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.quaternion>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.float2>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.float3>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.float4>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.quaternion>
	// System.Action<Chaos.MessageDispatcherInfo>
	// System.Action<Chaos.MessageInfo>
	// System.Action<Chaos.SessionMessageDispatcherInfo>
	// System.Action<Chaos.WebMessageDispatcherInfo>
	// System.Action<byte>
	// System.Action<int,object>
	// System.Action<int>
	// System.Action<long,int>
	// System.Action<long,object>
	// System.Action<object,object>
	// System.Action<object>
	// System.ArraySegment.Enumerator<byte>
	// System.ArraySegment.Enumerator<ushort>
	// System.ArraySegment<byte>
	// System.ArraySegment<ushort>
	// System.Buffers.ArrayPool<byte>
	// System.Buffers.IBufferWriter<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.LockedStack<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.PerCoreLockedStacks<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool<byte>
	// System.ByReference<byte>
	// System.ByReference<ushort>
	// System.Collections.Concurrent.ConcurrentDictionary.<GetEnumerator>d__35<int,object>
	// System.Collections.Concurrent.ConcurrentDictionary.<GetEnumerator>d__35<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary.DictionaryEnumerator<int,object>
	// System.Collections.Concurrent.ConcurrentDictionary.DictionaryEnumerator<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Node<int,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Node<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Tables<int,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Tables<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary<int,object>
	// System.Collections.Concurrent.ConcurrentDictionary<object,object>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<Chaos.MessageInfo>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<System.ValueTuple<ushort,object>>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<Chaos.MessageInfo>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<System.ValueTuple<ushort,object>>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<object>
	// System.Collections.Concurrent.ConcurrentQueue<Chaos.MessageInfo>
	// System.Collections.Concurrent.ConcurrentQueue<System.ValueTuple<ushort,object>>
	// System.Collections.Concurrent.ConcurrentQueue<object>
	// System.Collections.Generic.ArraySortHelper<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.ArraySortHelper<Chaos.MessageInfo>
	// System.Collections.Generic.ArraySortHelper<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.ArraySortHelper<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<Chaos.FiberInstanceId>
	// System.Collections.Generic.Comparer<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.Comparer<Chaos.MessageInfo>
	// System.Collections.Generic.Comparer<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.Comparer<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<long>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Comparer<ushort>
	// System.Collections.Generic.Dictionary.Enumerator<int,Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.Dictionary.Enumerator<int,Chaos.RequestInfo>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,Chaos.RequestInfo>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.Dictionary.KeyCollection<int,Chaos.RequestInfo>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<ushort,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,Chaos.RequestInfo>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.Dictionary.ValueCollection<int,Chaos.RequestInfo>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<ushort,object>
	// System.Collections.Generic.Dictionary<int,Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.Dictionary<int,Chaos.RequestInfo>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<ushort,object>
	// System.Collections.Generic.EqualityComparer<Chaos.FiberInstanceId>
	// System.Collections.Generic.EqualityComparer<Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.EqualityComparer<Chaos.RequestInfo>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<double>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<long>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.ICollection<Chaos.MessageInfo>
	// System.Collections.Generic.ICollection<Chaos.RequestInfo>
	// System.Collections.Generic.ICollection<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.ICollection<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,Chaos.ProcessInnerMessageSenderInfo>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,Chaos.RequestInfo>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.ICollection<System.ValueTuple<ushort,object>>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.IComparer<Chaos.MessageInfo>
	// System.Collections.Generic.IComparer<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.IComparer<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<long>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IDictionary<int,object>
	// System.Collections.Generic.IDictionary<object,object>
	// System.Collections.Generic.IEnumerable<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.IEnumerable<Chaos.MessageInfo>
	// System.Collections.Generic.IEnumerable<Chaos.RequestInfo>
	// System.Collections.Generic.IEnumerable<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.IEnumerable<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,Chaos.ProcessInnerMessageSenderInfo>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,Chaos.RequestInfo>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<ushort,object>>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.IEnumerator<Chaos.MessageInfo>
	// System.Collections.Generic.IEnumerator<Chaos.RequestInfo>
	// System.Collections.Generic.IEnumerator<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.IEnumerator<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,Chaos.ProcessInnerMessageSenderInfo>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,Chaos.RequestInfo>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<ushort,object>>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<long>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<ushort>
	// System.Collections.Generic.IList<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.IList<Chaos.MessageInfo>
	// System.Collections.Generic.IList<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.IList<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IReadOnlyDictionary<object,int>
	// System.Collections.Generic.KeyValuePair<int,Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.KeyValuePair<int,Chaos.RequestInfo>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<ushort,object>
	// System.Collections.Generic.List.Enumerator<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.List.Enumerator<Chaos.MessageInfo>
	// System.Collections.Generic.List.Enumerator<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.List.Enumerator<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.List<Chaos.MessageInfo>
	// System.Collections.Generic.List<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.List<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<Chaos.FiberInstanceId>
	// System.Collections.Generic.ObjectComparer<Chaos.MessageDispatcherInfo>
	// System.Collections.Generic.ObjectComparer<Chaos.MessageInfo>
	// System.Collections.Generic.ObjectComparer<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.Generic.ObjectComparer<Chaos.WebMessageDispatcherInfo>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<long>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectComparer<ushort>
	// System.Collections.Generic.ObjectEqualityComparer<Chaos.FiberInstanceId>
	// System.Collections.Generic.ObjectEqualityComparer<Chaos.ProcessInnerMessageSenderInfo>
	// System.Collections.Generic.ObjectEqualityComparer<Chaos.RequestInfo>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<double>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<long>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<long,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<long,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<long,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<long,object>
	// System.Collections.Generic.SortedDictionary<long,object>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<Chaos.MessageDispatcherInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<Chaos.MessageInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<Chaos.SessionMessageDispatcherInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<Chaos.WebMessageDispatcherInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<Chaos.MessageDispatcherInfo>
	// System.Comparison<Chaos.MessageInfo>
	// System.Comparison<Chaos.SessionMessageDispatcherInfo>
	// System.Comparison<Chaos.WebMessageDispatcherInfo>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Func<int,object,object>
	// System.Func<int,object>
	// System.Func<object,byte>
	// System.Func<object,object,byte>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.IEquatable<object>
	// System.Linq.Buffer<Chaos.RequestInfo>
	// System.Linq.Buffer<object>
	// System.Nullable<byte>
	// System.Nullable<int>
	// System.Predicate<Chaos.MessageDispatcherInfo>
	// System.Predicate<Chaos.MessageInfo>
	// System.Predicate<Chaos.SessionMessageDispatcherInfo>
	// System.Predicate<Chaos.WebMessageDispatcherInfo>
	// System.Predicate<int>
	// System.Predicate<object>
	// System.ReadOnlySpan<byte>
	// System.ReadOnlySpan<ushort>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Span.Enumerator<byte>
	// System.Span.Enumerator<ushort>
	// System.Span<byte>
	// System.Span<ushort>
	// System.Text.Json.Serialization.ConfigurationList<object>
	// System.Text.Json.Serialization.Converters.JsonMetadataServicesConverter<object>
	// System.Text.Json.Serialization.JsonConverter<object>
	// System.Text.Json.Serialization.JsonDictionaryConverter<object>
	// System.Text.Json.Serialization.JsonResumableConverter<object>
	// System.Text.Json.Serialization.Metadata.JsonParameterInfo<object>
	// System.Text.Json.Serialization.Metadata.JsonPropertyInfo.<>c__DisplayClass10_0<object>
	// System.Text.Json.Serialization.Metadata.JsonPropertyInfo.<>c__DisplayClass10_1<object>
	// System.Text.Json.Serialization.Metadata.JsonPropertyInfo.<>c__DisplayClass15_0<object>
	// System.Text.Json.Serialization.Metadata.JsonPropertyInfo.<>c__DisplayClass15_1<object>
	// System.Text.Json.Serialization.Metadata.JsonPropertyInfo.<>c__DisplayClass9_0<object>
	// System.Text.Json.Serialization.Metadata.JsonPropertyInfo.<>c__DisplayClass9_1<object>
	// System.Text.Json.Serialization.Metadata.JsonPropertyInfo<object>
	// System.Text.Json.Serialization.Metadata.JsonTypeInfo.<>c__DisplayClass37_0<object>
	// System.Text.Json.Serialization.Metadata.JsonTypeInfo.<>c__DisplayClass37_1<object>
	// System.Text.Json.Serialization.Metadata.JsonTypeInfo<object>
	// System.Threading.Tasks.Sources.IValueTaskSource<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory<object>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<object>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<object>
	// System.Threading.Tasks.ValueTask<object>
	// System.ValueTuple<Chaos.FiberInstanceId,object>
	// System.ValueTuple<ushort,object>
	// }}

	public void RefMethods()
	{
		// Chaos.ThreadTask<object> Chaos.CancelSignalExtensions.TimeoutAsync<object>(Chaos.ThreadTask<object>,long)
		// object Chaos.Entity.AddChildWithId<object,object>(long,object,bool)
		// object Chaos.Entity.AddComponent<object,int>(int,bool)
		// object Chaos.Entity.AddComponent<object,object,byte>(object,byte,bool)
		// object Chaos.Entity.AddComponent<object,object>(object,bool)
		// object Chaos.Entity.AddComponent<object>(bool)
		// object Chaos.Entity.AddComponentWithId<object,int>(long,int,bool)
		// object Chaos.Entity.AddComponentWithId<object,object,byte>(long,object,byte,bool)
		// object Chaos.Entity.AddComponentWithId<object,object>(long,object,bool)
		// object Chaos.Entity.AddComponentWithId<object>(long,bool)
		// object Chaos.Entity.GetChild<object>(long)
		// object Chaos.Entity.GetComponent<object>()
		// object Chaos.Entity.GetParent<object>()
		// System.Void Chaos.EntitySystemRegistry.Awake<int>(Chaos.Entity,int)
		// System.Void Chaos.EntitySystemRegistry.Awake<object,byte>(Chaos.Entity,object,byte)
		// System.Void Chaos.EntitySystemRegistry.Awake<object>(Chaos.Entity,object)
		// object Chaos.EventSystem.GetInvoker<object,Chaos.MailboxInvokeEventArgs>(long)
		// object Chaos.EventSystem.GetInvoker<object,Chaos.NetComponentOnReadEventArgs>(long)
		// System.Void Chaos.EventSystem.Invoke<Chaos.MailboxInvokeEventArgs>(long,Chaos.MailboxInvokeEventArgs)
		// System.Void Chaos.EventSystem.Invoke<Chaos.NetComponentOnReadEventArgs>(long,Chaos.NetComponentOnReadEventArgs)
		// System.Void Chaos.EventSystem.Publish<object,Chaos.UnitChangePositionEventArgs>(object,Chaos.UnitChangePositionEventArgs)
		// System.Void Chaos.EventSystem.Publish<object,Chaos.UnitChangeRotationEventArgs>(object,Chaos.UnitChangeRotationEventArgs)
		// Chaos.ThreadTask Chaos.EventSystem.PublishAsync<object,Chaos.AppStartInitializeFinishEventArgs>(object,Chaos.AppStartInitializeFinishEventArgs)
		// Chaos.ThreadTask Chaos.EventSystem.PublishAsync<object,Chaos.EntryClientPhaseEventArgs>(object,Chaos.EntryClientPhaseEventArgs)
		// Chaos.ThreadTask Chaos.EventSystem.PublishAsync<object,Chaos.EntryServerPhaseEventArgs>(object,Chaos.EntryServerPhaseEventArgs)
		// Chaos.ThreadTask Chaos.EventSystem.PublishAsync<object,Chaos.EntrySharedPhaseEventArgs>(object,Chaos.EntrySharedPhaseEventArgs)
		// Chaos.ThreadTask Chaos.EventSystem.PublishAsync<object,Chaos.WebUiInitializeFinishEventArgs>(object,Chaos.WebUiInitializeFinishEventArgs)
		// System.Void Chaos.MongoRegister.RegisterStruct<Unity.Mathematics.float2>()
		// System.Void Chaos.MongoRegister.RegisterStruct<Unity.Mathematics.float3>()
		// System.Void Chaos.MongoRegister.RegisterStruct<Unity.Mathematics.float4>()
		// System.Void Chaos.MongoRegister.RegisterStruct<Unity.Mathematics.quaternion>()
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.AppStartInitializeFinishEventHandler.<RunAsync>d__0>(object&,Chaos.AppStartInitializeFinishEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.Entry.<StartAsync>d__1>(object&,Chaos.Entry.<StartAsync>d__1&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.EntryClientPhaseEventHandler.<RunAsync>d__0>(object&,Chaos.EntryClientPhaseEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.EntrySharedPhaseEventHandler.<RunAsync>d__0>(object&,Chaos.EntrySharedPhaseEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.FiberInitialize_Client.<Handle>d__0>(object&,Chaos.FiberInitialize_Client.<Handle>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.FiberInitialize_Sample.<Handle>d__0>(object&,Chaos.FiberInitialize_Sample.<Handle>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.FiberInitialize_WebUI.<Handle>d__0>(object&,Chaos.FiberInitialize_WebUI.<Handle>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.MessageDispatcher.<HandleAsync>d__3>(object&,Chaos.MessageDispatcher.<HandleAsync>d__3&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.MessageHandler.<Handle>d__1<object,object,object>>(object&,Chaos.MessageHandler.<Handle>d__1<object,object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.MessageHandler.<Handle>d__1<object,object>>(object&,Chaos.MessageHandler.<Handle>d__1<object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<CallAsync>g__TimeoutAsync|0>d>(object&,Chaos.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<CallAsync>g__TimeoutAsync|0>d&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.SessionMessageHandler.<HandleAsync>d__2<object,object>>(object&,Chaos.SessionMessageHandler.<HandleAsync>d__2<object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.SessionMessageHandler.<HandleAsync>d__2<object>>(object&,Chaos.SessionMessageHandler.<HandleAsync>d__2<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.Web2UnityLoadedMessageHandler.<RunAsync>d__0>(object&,Chaos.Web2UnityLoadedMessageHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.Web2UnityVersionRequestHandler.<RunAsync>d__0>(object&,Chaos.Web2UnityVersionRequestHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.WebMessageHandler.<HandleAsync>d__2<object,object>>(object&,Chaos.WebMessageHandler.<HandleAsync>d__2<object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.WebMessageHandler.<HandleAsync>d__2<object>>(object&,Chaos.WebMessageHandler.<HandleAsync>d__2<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.WebUiInitializeFinishEventHandler.<RunAsync>d__0>(object&,Chaos.WebUiInitializeFinishEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.WebUiMessageSentEventHandler.<RunAsync>d__0>(object&,Chaos.WebUiMessageSentEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.AwaitUnsafeOnCompleted<object,Chaos.Wpf2UnityLoadedMessageHandler.<RunAsync>d__0>(object&,Chaos.Wpf2UnityLoadedMessageHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.MemoryMappedFileComponentSystem.<SendAsync>d__7>(object&,Chaos.MemoryMappedFileComponentSystem.<SendAsync>d__7&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.ObjectWaitSystem.<WaitAsync>d__2<object>>(object&,Chaos.ObjectWaitSystem.<WaitAsync>d__2<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.ProcessInnerMessageSenderInfo.<WaitAsync>d__13>(object&,Chaos.ProcessInnerMessageSenderInfo.<WaitAsync>d__13&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.ProcessInnerSenderSystem.<CallAsync>d__10>(object&,Chaos.ProcessInnerSenderSystem.<CallAsync>d__10&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.RequestInfo.<WaitAsync>d__5>(object&,Chaos.RequestInfo.<WaitAsync>d__5&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.SessionSystem.<CallAsync>d__3>(object&,Chaos.SessionSystem.<CallAsync>d__3&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.SessionSystem.<CallAsync>d__4>(object&,Chaos.SessionSystem.<CallAsync>d__4&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.WebUiComponentSystem.<SendAsync>d__4>(object&,Chaos.WebUiComponentSystem.<SendAsync>d__4&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.AwaitUnsafeOnCompleted<object,Chaos.WebViewComponentSystem.<SendAsync>d__8>(object&,Chaos.WebViewComponentSystem.<SendAsync>d__8&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.AppStartInitializeFinishEventHandler.<RunAsync>d__0>(Chaos.AppStartInitializeFinishEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.Entry.<StartAsync>d__1>(Chaos.Entry.<StartAsync>d__1&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.EntryClientPhaseEventHandler.<RunAsync>d__0>(Chaos.EntryClientPhaseEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.EntrySharedPhaseEventHandler.<RunAsync>d__0>(Chaos.EntrySharedPhaseEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.AppStartInitializeFinishEventArgs>>(Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.AppStartInitializeFinishEventArgs>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.EntryClientPhaseEventArgs>>(Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.EntryClientPhaseEventArgs>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.EntryServerPhaseEventArgs>>(Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.EntryServerPhaseEventArgs>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.EntrySharedPhaseEventArgs>>(Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.EntrySharedPhaseEventArgs>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.WebUiInitializeFinishEventArgs>>(Chaos.EventSystem.<PublishAsync>d__4<object,Chaos.WebUiInitializeFinishEventArgs>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.FiberInitialize_Client.<Handle>d__0>(Chaos.FiberInitialize_Client.<Handle>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.FiberInitialize_Sample.<Handle>d__0>(Chaos.FiberInitialize_Sample.<Handle>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.FiberInitialize_WebUI.<Handle>d__0>(Chaos.FiberInitialize_WebUI.<Handle>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.MessageDispatcher.<HandleAsync>d__3>(Chaos.MessageDispatcher.<HandleAsync>d__3&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.MessageHandler.<Handle>d__1<object,object,object>>(Chaos.MessageHandler.<Handle>d__1<object,object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.MessageHandler.<Handle>d__1<object,object>>(Chaos.MessageHandler.<Handle>d__1<object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<CallAsync>g__TimeoutAsync|0>d>(Chaos.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<CallAsync>g__TimeoutAsync|0>d&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.SessionMessageHandler.<HandleAsync>d__2<object,object>>(Chaos.SessionMessageHandler.<HandleAsync>d__2<object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.SessionMessageHandler.<HandleAsync>d__2<object>>(Chaos.SessionMessageHandler.<HandleAsync>d__2<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.Web2UnityLoadedMessageHandler.<RunAsync>d__0>(Chaos.Web2UnityLoadedMessageHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.Web2UnityVersionRequestHandler.<RunAsync>d__0>(Chaos.Web2UnityVersionRequestHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.WebMessageHandler.<HandleAsync>d__2<object,object>>(Chaos.WebMessageHandler.<HandleAsync>d__2<object,object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.WebMessageHandler.<HandleAsync>d__2<object>>(Chaos.WebMessageHandler.<HandleAsync>d__2<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.WebUiInitializeFinishEventHandler.<RunAsync>d__0>(Chaos.WebUiInitializeFinishEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.WebUiMessageSentEventHandler.<RunAsync>d__0>(Chaos.WebUiMessageSentEventHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder.Start<Chaos.Wpf2UnityLoadedMessageHandler.<RunAsync>d__0>(Chaos.Wpf2UnityLoadedMessageHandler.<RunAsync>d__0&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.CancelSignalExtensions.<TimeoutAsync>d__7<object>>(Chaos.CancelSignalExtensions.<TimeoutAsync>d__7<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.MemoryMappedFileComponentSystem.<SendAsync>d__7>(Chaos.MemoryMappedFileComponentSystem.<SendAsync>d__7&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.ObjectWaitSystem.<WaitAsync>d__2<object>>(Chaos.ObjectWaitSystem.<WaitAsync>d__2<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.ProcessInnerMessageSenderInfo.<WaitAsync>d__13>(Chaos.ProcessInnerMessageSenderInfo.<WaitAsync>d__13&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.ProcessInnerSenderSystem.<CallAsync>d__10>(Chaos.ProcessInnerSenderSystem.<CallAsync>d__10&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.RequestInfo.<WaitAsync>d__5>(Chaos.RequestInfo.<WaitAsync>d__5&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.SessionSystem.<CallAsync>d__3>(Chaos.SessionSystem.<CallAsync>d__3&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.SessionSystem.<CallAsync>d__4>(Chaos.SessionSystem.<CallAsync>d__4&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.ThreadTask.<GetContextAsync>d__31<object>>(Chaos.ThreadTask.<GetContextAsync>d__31<object>&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.WebUiComponentSystem.<SendAsync>d__4>(Chaos.WebUiComponentSystem.<SendAsync>d__4&)
		// System.Void Chaos.ThreadTaskAsyncMethodBuilder<object>.Start<Chaos.WebViewComponentSystem.<SendAsync>d__8>(Chaos.WebViewComponentSystem.<SendAsync>d__8&)
		// object Chaos.World.AddSingleton<object,object>(object)
		// object Chaos.World.AddSingleton<object>()
		// bool MemoryPack.MemoryPackFormatterProvider.IsRegistered<System.Nullable<int>>()
		// bool MemoryPack.MemoryPackFormatterProvider.IsRegistered<object>()
		// System.Void MemoryPack.MemoryPackFormatterProvider.Register<System.Nullable<int>>(MemoryPack.MemoryPackFormatter<System.Nullable<int>>)
		// System.Void MemoryPack.MemoryPackFormatterProvider.Register<object>(MemoryPack.MemoryPackFormatter<object>)
		// System.Void MemoryPack.MemoryPackReader.DangerousReadUnmanaged<System.Nullable<int>>(System.Nullable<int>&)
		// System.Void MemoryPack.MemoryPackReader.DangerousReadUnmanaged<byte,int,int,int,System.Nullable<int>,System.Nullable<int>>(byte&,int&,int&,int&,System.Nullable<int>&,System.Nullable<int>&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,byte>(byte&,byte&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int,int,int>(byte&,int&,int&,int&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,int>(byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int,long,long>(byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte,int>(byte&,int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<byte>(byte&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<int>(int&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackReader.ReadUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackWriter<object>.DangerousWriteUnmanagedWithObjectHeader<byte,int,int,int,System.Nullable<int>,System.Nullable<int>>(byte,byte&,int&,int&,int&,System.Nullable<int>&,System.Nullable<int>&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanaged<long,long>(long&,long&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanaged<long>(long&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanagedWithObjectHeader<byte,byte>(byte,byte&,byte&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanagedWithObjectHeader<byte,int,int,int,int>(byte,byte&,int&,int&,int&,int&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanagedWithObjectHeader<byte,int,int>(byte,byte&,int&,int&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanagedWithObjectHeader<byte,int,long,long>(byte,byte&,int&,long&,long&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanagedWithObjectHeader<byte,int>(byte,byte&,int&)
		// System.Void MemoryPack.MemoryPackWriter<object>.WriteUnmanagedWithObjectHeader<byte>(byte,byte&)
		// System.Void Serilog.ILogger.Write<Chaos.FiberInstanceId,int,object>(Serilog.Events.LogEventLevel,string,Chaos.FiberInstanceId,int,object)
		// System.Void Serilog.ILogger.Write<byte,object>(Serilog.Events.LogEventLevel,string,byte,object)
		// System.Void Serilog.ILogger.Write<byte>(Serilog.Events.LogEventLevel,string,byte)
		// System.Void Serilog.ILogger.Write<int>(Serilog.Events.LogEventLevel,string,int)
		// System.Void Serilog.ILogger.Write<long,object>(Serilog.Events.LogEventLevel,string,long,object)
		// System.Void Serilog.ILogger.Write<object,object,object>(Serilog.Events.LogEventLevel,string,object,object,object)
		// System.Void Serilog.ILogger.Write<object,object>(Serilog.Events.LogEventLevel,string,object,object)
		// System.Void Serilog.ILogger.Write<object>(Serilog.Events.LogEventLevel,string,object)
		// System.Void Serilog.ILogger.Write<ushort,int>(Serilog.Events.LogEventLevel,string,ushort,int)
		// System.Void Serilog.ILogger.Write<ushort,object>(Serilog.Events.LogEventLevel,string,ushort,object)
		// System.Void Serilog.ILogger.Write<ushort>(Serilog.Events.LogEventLevel,string,ushort)
		// System.Void Serilog.Log.Debug<int>(string,int)
		// System.Void Serilog.Log.Error<long,object>(string,long,object)
		// System.Void Serilog.Log.Error<object,object,object>(string,object,object,object)
		// System.Void Serilog.Log.Error<object,object>(string,object,object)
		// System.Void Serilog.Log.Error<object>(string,object)
		// System.Void Serilog.Log.Error<ushort,object>(string,ushort,object)
		// System.Void Serilog.Log.Error<ushort>(string,ushort)
		// System.Void Serilog.Log.Information<byte,object>(string,byte,object)
		// System.Void Serilog.Log.Information<byte>(string,byte)
		// System.Void Serilog.Log.Information<object,object>(string,object,object)
		// System.Void Serilog.Log.Information<object>(string,object)
		// System.Void Serilog.Log.Warning<Chaos.FiberInstanceId,int,object>(string,Chaos.FiberInstanceId,int,object)
		// System.Void Serilog.Log.Warning<long,object>(string,long,object)
		// System.Void Serilog.Log.Warning<object>(string,object)
		// System.Void Serilog.Log.Warning<ushort,int>(string,ushort,int)
		// System.Void Serilog.Log.Write<Chaos.FiberInstanceId,int,object>(Serilog.Events.LogEventLevel,string,Chaos.FiberInstanceId,int,object)
		// System.Void Serilog.Log.Write<byte,object>(Serilog.Events.LogEventLevel,string,byte,object)
		// System.Void Serilog.Log.Write<byte>(Serilog.Events.LogEventLevel,string,byte)
		// System.Void Serilog.Log.Write<int>(Serilog.Events.LogEventLevel,string,int)
		// System.Void Serilog.Log.Write<long,object>(Serilog.Events.LogEventLevel,string,long,object)
		// System.Void Serilog.Log.Write<object,object,object>(Serilog.Events.LogEventLevel,string,object,object,object)
		// System.Void Serilog.Log.Write<object,object>(Serilog.Events.LogEventLevel,string,object,object)
		// System.Void Serilog.Log.Write<object>(Serilog.Events.LogEventLevel,string,object)
		// System.Void Serilog.Log.Write<ushort,int>(Serilog.Events.LogEventLevel,string,ushort,int)
		// System.Void Serilog.Log.Write<ushort,object>(Serilog.Events.LogEventLevel,string,ushort,object)
		// System.Void Serilog.Log.Write<ushort>(Serilog.Events.LogEventLevel,string,ushort)
		// object System.Activator.CreateInstance<object>()
		// int System.Collections.Generic.CollectionExtensions.GetValueOrDefault<object,int>(System.Collections.Generic.IReadOnlyDictionary<object,int>,object,int)
		// System.Void System.IO.UnmanagedMemoryAccessor.Read<byte>(long,byte&)
		// int System.IO.UnmanagedMemoryAccessor.ReadArray<byte>(long,byte[],int,int)
		// System.Void System.IO.UnmanagedMemoryAccessor.WriteArray<byte>(long,byte[],int,int)
		// Chaos.RequestInfo[] System.Linq.Enumerable.ToArray<Chaos.RequestInfo>(System.Collections.Generic.IEnumerable<Chaos.RequestInfo>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Reflection.CustomAttributeExtensions.GetCustomAttributes<object>(System.Reflection.MemberInfo,bool)
		// bool System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<byte>()
		// byte& System.Runtime.CompilerServices.Unsafe.Add<byte>(byte&,int)
		// byte& System.Runtime.CompilerServices.Unsafe.As<byte,byte>(byte&)
		// System.Nullable<int> System.Runtime.CompilerServices.Unsafe.ReadUnaligned<System.Nullable<int>>(byte&)
		// byte System.Runtime.CompilerServices.Unsafe.ReadUnaligned<byte>(byte&)
		// int System.Runtime.CompilerServices.Unsafe.ReadUnaligned<int>(byte&)
		// long System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(byte&)
		// int System.Runtime.CompilerServices.Unsafe.SizeOf<System.Nullable<int>>()
		// int System.Runtime.CompilerServices.Unsafe.SizeOf<byte>()
		// int System.Runtime.CompilerServices.Unsafe.SizeOf<byte>()
		// int System.Runtime.CompilerServices.Unsafe.SizeOf<int>()
		// int System.Runtime.CompilerServices.Unsafe.SizeOf<long>()
		// System.Void System.Runtime.CompilerServices.Unsafe.WriteUnaligned<System.Nullable<int>>(byte&,System.Nullable<int>)
		// System.Void System.Runtime.CompilerServices.Unsafe.WriteUnaligned<byte>(byte&,byte)
		// System.Void System.Runtime.CompilerServices.Unsafe.WriteUnaligned<int>(byte&,int)
		// System.Void System.Runtime.CompilerServices.Unsafe.WriteUnaligned<long>(byte&,long)
		// uint System.Runtime.InteropServices.SafeBuffer.AlignedSizeOf<byte>()
		// byte System.Runtime.InteropServices.SafeBuffer.Read<byte>(ulong)
		// System.Void System.Runtime.InteropServices.SafeBuffer.ReadArray<byte>(ulong,byte[],int,int)
		// uint System.Runtime.InteropServices.SafeBuffer.SizeOf<byte>()
		// System.Void System.Runtime.InteropServices.SafeBuffer.WriteArray<byte>(ulong,byte[],int,int)
		// object System.Text.Json.JsonSerializer.Deserialize<object>(string,System.Text.Json.JsonSerializerOptions)
		// System.Text.Json.Serialization.Metadata.JsonTypeInfo<object> System.Text.Json.JsonSerializer.GetTypeInfo<object>(System.Text.Json.JsonSerializerOptions)
		// object System.Text.Json.JsonSerializer.ReadFromSpan<object>(System.ReadOnlySpan<System.Char>,System.Text.Json.Serialization.Metadata.JsonTypeInfo<object>)
		// object System.Text.Json.JsonSerializer.ReadFromSpan<object>(System.ReadOnlySpan<byte>,System.Text.Json.Serialization.Metadata.JsonTypeInfo<object>,System.Nullable<int>)
		// string System.Text.Json.JsonSerializer.Serialize<object>(object,System.Text.Json.JsonSerializerOptions)
		// string System.Text.Json.JsonSerializer.WriteString<object>(object&,System.Text.Json.Serialization.Metadata.JsonTypeInfo<object>)
		// object UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
		// object UnityEngine.AndroidJavaObject.FromJavaArrayDeleteLocalRef<object>(System.IntPtr)
		// object UnityEngine.AndroidJavaObject.GetStatic<object>(string)
		// object UnityEngine.AndroidJavaObject._GetStatic<object>(System.IntPtr)
		// object UnityEngine.AndroidJavaObject._GetStatic<object>(string)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.Resources.Load<object>(string)
		// object UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
	}
}