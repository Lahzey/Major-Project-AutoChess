using System;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.environment;
using ProtoBuf;

namespace MPAutoChess.logic.core.player;

[ProtoContract]
public struct Account : IEquatable<Account> {

    [ProtoMember(1)] public long Id { get; private set; }
    
    [ProtoMember(2)] public string Name { get; private set; }
    
    [ProtoMember(3)] public ArenaType Arena { get; private set; }

    private static Account currentAccount;

    public Account() : this(0, "") { } // for ProtoBuf

    public Account(long id, string name) {
        Id = id;
        Name = name;
        Arena = ArenaType.DEFAULT;
    }
    
    public static Account FindById(long id) {
        // TODO: load from DB
        return new Account(id, $"Account[{id}]");
    }

    public static Account GetCurrent() {
        if (currentAccount.Id <= 0) return currentAccount;
        
        // temporary logic until login is implemented
        string[] args = OS.GetCmdlineArgs();
        string? accountIdArg = args.FirstOrDefault(arg => arg.StartsWith("accountId="));
        if (accountIdArg == null) throw new ArgumentException("Command line argument 'accountId' not found. Expected format: accountId=12345");
        accountIdArg = accountIdArg.Substring("players=".Length);
        long accountId = long.Parse(accountIdArg);
        
        currentAccount = FindById(accountId);
        return currentAccount;
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