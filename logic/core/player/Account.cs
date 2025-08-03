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

    private static Account currentAccount;
    public string SecretKey { get; private set; }

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
        if (ServerController.Instance?.IsServer??false) throw new InvalidOperationException("Server does not have an account.");
        if (currentAccount != null) return currentAccount;
        
        // temporary logic until login is implemented
        string[] args = OS.GetCmdlineArgs();
        string? accountIdArg = args.FirstOrDefault(arg => arg.StartsWith("accountId="));
        if (accountIdArg == null) throw new ArgumentException("Command line argument 'accountId' not found. Expected format: accountId=12345");
        accountIdArg = accountIdArg.Substring("accountId=".Length);
        long accountId = long.Parse(accountIdArg);
        
        currentAccount = FindById(accountId);
        currentAccount.LoadSecretKey();
        return currentAccount;
    }
    
    private void LoadSecretKey() {
        if (string.IsNullOrEmpty(SecretKey)) {
            // temporary logic until login is implemented
            string[] args = OS.GetCmdlineArgs();
            string? secretKeyArg = args.FirstOrDefault(arg => arg.StartsWith("secret="));
            if (secretKeyArg == null) throw new ArgumentException("Command line argument 'secretKey' not found. Expected format: secretKey=your_secret_key");
            secretKeyArg = secretKeyArg.Substring("secret=".Length);
            SecretKey = secretKeyArg;
        }
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