using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Godot;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.networking;

public class SerializationTest {
    
    private static T TryCast<T>(object obj) {
        return (T)obj;
    }

    public static void Main() {
        object surrogate = new NodeDataSurrogate<Player>();
        Player p = TryCast<Player>(surrogate);
        return;
        
        
        ProtoBufSettings.Set();
        
        // IdentifiableSurrogate<NodeTest>.printDebugMessages = true;
        // IdentifiableSurrogate<IIdentifiableTest>.printDebugMessages = true;
        // try {
        //     NodeTest test = NodeTest.Create();
        //     // Console.WriteLine($"Parent({((IIdentifiable)test).GetId()})[Child({((IIdentifiable)test.Child).GetId()})]");
        //     // TestParentIdentifiable test = new TestParentIdentifiable(10);
        //     using MemoryStream inStream = new MemoryStream();
        //     Serializer.Serialize(inStream, test);
        //     byte[] inBytes = inStream.ToArray();
        //     using MemoryStream outStream = new MemoryStream(inBytes);
        //     NodeTest testNode = Serializer.Deserialize<NodeTest>(outStream);
        //     Console.WriteLine("Received NodeTest: " + testNode);
        // } catch (Exception e) {
        //     Console.WriteLine(e.GetType() + ": " + e.Message);
        //     Console.WriteLine(e.StackTrace);
        // }
        // return;
        
        IdentifiableSurrogate<TestParentIdentifiable>.printDebugMessages = true;
        IdentifiableSurrogate<TestChildIdentifiable>.printDebugMessages = true;
        Console.WriteLine("---START---");
        TestParentIdentifiable preEdit = new TestParentIdentifiable(5);
        Console.WriteLine("---SERIALIZING---");
        using MemoryStream stream1 = new MemoryStream();
        Serializer.Serialize(stream1, preEdit);
        byte[] bytes = stream1.ToArray();
        Console.WriteLine("---DESERIALIZING---");
        using MemoryStream stream2 = new MemoryStream(bytes);
        TestParentIdentifiable toEdit = Serializer.Deserialize<TestParentIdentifiable>(stream2);
        toEdit.ParentValue = 42;
        toEdit.Children[0].ChildValue = 99;
        Console.WriteLine("---SERIALIZING---");
        using MemoryStream stream3 = new MemoryStream();
        Serializer.Serialize(stream3, toEdit);
        byte[] editedBytes = stream3.ToArray();
        // toEdit.ParentValue = 50;
        IIdentifiable.UnregisterInstance(toEdit);
        Console.WriteLine("---DESERIALIZING---");
        using MemoryStream stream4 = new MemoryStream(editedBytes);
        TestParentIdentifiable postEdit = Serializer.Deserialize<TestParentIdentifiable>(stream4);
        Console.WriteLine("---END---");
        
        Console.WriteLine("Pre-Edit: " + preEdit);
        Console.WriteLine("Post-Edit: " + postEdit);
        Console.WriteLine("Same Object? " + (preEdit == postEdit));
    }
    
}


[ProtoContract]
public class TestParentIdentifiable : IIdentifiable {
    public string Id { get; set; }
    
    [ProtoMember(1)] public int ParentValue { get; set; }
    
    [ProtoMember(2)] public TestChildIdentifiable[] Children { get; set; }

    public TestParentIdentifiable() { }
    
    public TestParentIdentifiable(int childrenCount) {
        ParentValue = childrenCount;
        Children = new TestChildIdentifiable[childrenCount];
        for (int i = 0; i < 5; i++) {
            Children[i] = new TestChildIdentifiable(i);
        }
    }

    public override string ToString() {
        return base.ToString() + "@" + Id + "\n" +
               "   -> ParentValue: " + ParentValue + "\n" +
               "   -> Children:\n      " + string.Join<TestChildIdentifiable>("\n      ", Children);
    }
}

[ProtoContract]
public class TestChildIdentifiable : IIdentifiable {
    public string Id { get; set; }
    
    [ProtoMember(1)] public int ChildValue { get; set; }
    [ProtoMember(2)] public IIdentifiableTest Test { get; set; }
    
    public TestChildIdentifiable() { }

    public TestChildIdentifiable(int childValue) {
        ChildValue = childValue;
        Test = new IIdentifiableTest { Id = "Test" + childValue, Value = childValue * 10 };
    }
    
    public override string ToString() {
        return base.ToString() + "@" + Id + "  -> value: " + ChildValue;
    }
}

[ProtoContract]
public class NodeTest : IIdentifiable {
    public string Id { get; set; }
    
    [ProtoMember(1)] public IIdentifiableTest[] Child { get; set; }
    [ProtoMember(2)] public int Value { get; set; }

    public static NodeTest Create() {
        NodeTest test = new NodeTest();
        // test.Child = new Player();
        // test.Child.Account = new Account(1, "TestPlayer");
        // test.Child.Gold = 99999;
        // test.Child.Name = "TestPlayer";
        // ServerController.Instance.AddChild(test.Child);
        test.Child = new IIdentifiableTest[] { new IIdentifiableTest() };
        test.Child[0].Value = 9999;
        test.Value = 50;
        return test;
    }

    public override string ToString() {
        // return $"NodeTest[Value={Value} | Gold={Child?.Gold.ToString() ?? "null"}]";
        return $"NodeTest[Value={Value} | Child={Child[0]}]";
    }
}

[ProtoContract]
public class IIdentifiableTest : IIdentifiable {
    public string Id { get; set; }
    
    [ProtoMember(1)] public int Value { get; set; }
    
    public override string ToString() {
        return $"IdentifiableTest[Id={Id} | Value={Value}]";
    }
}