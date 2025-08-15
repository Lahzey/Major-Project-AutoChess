using System;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.networking;
using ProtoBuf;

namespace MPAutoChess.logic.core.player;

[ProtoContract]
public class Account : IEquatable<Account> {

    [ProtoMember(1)] public long Id { get; private set; }
    
    [ProtoMember(2)] public string Name { get; private set; }
    
    [ProtoMember(3)] public ArenaType Arena { get; private set; }
    
    [ProtoMember(4)] public Texture2D ProfilePicture { get; private set; }

    private static Account currentAccount;
    public string SecretKey { get; private set; }

    public Account() : this(0, "", "") { } // for ProtoBuf

    public Account(long id, string name, string secret) {
        Id = id;
        Name = name;
        SecretKey = secret;
        Arena = ArenaType.DEFAULT;
        ProfilePicture = ResourceLoader.Load<Texture2D>("res://assets/profile_pictures/default.png");
    }

    public static Account GetCurrent() {
        if (ServerController.Instance?.IsServer??false) throw new InvalidOperationException("Server does not have an account.");
        return currentAccount;
    }
    
    public static void SetCurrent(Account account) {
        if (ServerController.Instance?.IsServer??false) throw new InvalidOperationException("Server does not have an account.");
        currentAccount = account;
    }

    public ArenaType GetSelectedArenaType() {
        return Arena;
    }

    public override bool Equals(object obj) {
        return obj is Account account && Id == account.Id;
    }

    public bool Equals(Account other) {
        return Id == other.Id;
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }
}