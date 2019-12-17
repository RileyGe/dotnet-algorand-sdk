# Dotnet Algorand SDK

大家期待的.Net版的Algorand SDK终于上线了，大家和我一起来尝尝鲜吧。

1. 依赖

本类库依赖于.net core 2.0，所以理论上大家可以在任何支持平台.net core的平台上使用该类库。

2. 如何引用

打开NuGet包管理器搜索“Algorand”或者使用命令行：

```powershell
Install-Package Algorand
```

3. 快速开始

```csharp
string ALGOD_API_ADDR = "your algod api address";
//string ALGOD_API_ADDR = "http://hackathon.algodev.network:9100";  //hackathon
string ALGOD_API_TOKEN = "your algod api token"; //find in the algod.token
//string ALGOD_API_TOKEN = "ef920e2e7e002953f4b29a8af720efe8e4ecc75ff102b165e0472834b25832c1";
AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
```

然后你就可以从Algorand上获取信息了：

``` csharp
try
{
    Supply supply = algodApiInstance.GetSupply();
    Console.WriteLine("Total Algorand Supply: " + supply.TotalMoney);
    Console.WriteLine("Online Algorand Supply: " + supply.OnlineMoney);
}
catch (ApiException e)
{
    Console.WriteLine("Exception when calling algod#getSupply: " + e.Message);
}
ulong? feePerByte;
string genesisID;
Digest genesisHash;
ulong? firstRound = 301;
try
{
    TransactionParams transParams = algodApiInstance.TransactionParams();
    feePerByte = transParams.Fee;
    genesisHash = new Digest(Convert.FromBase64String(transParams.Genesishashb64));
    genesisID = transParams.GenesisID;
    Console.WriteLine("Suggested Fee: " + feePerByte);
    NodeStatus s = algodApiInstance.GetStatus();
    firstRound = s.LastRound;
    Console.WriteLine("Current Round: " + firstRound);
}
catch (ApiException e)
{
    throw new Exception("Could not get params", e);
}
```

为了做更多的事，你必须有一个账号。你可以用Account acc = new Account();来创建一个随机的账号，也可以用Mnemonic来创建帐户，下面的例子使用Mnemonic来创建一个账户，并向另一个帐号发送一定量的Algo：

```csharp
ulong? amount = 100000;
ulong? lastRound = firstRound + 1000; // 1000 is the max tx window
string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
Account src = new Account(SRC_ACCOUNT);
Console.WriteLine("My account address is:" + src.Address.ToString());
string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
Transaction tx = new Transaction(src.Address, new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
//将数据发送到区块链之前先要进行签名
SignedTransaction signedTx = src.SignTransactionWithFeePerByte(tx, feePerByte);
Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);
// send the transaction to the network
try
{
    //先对数据进行编码
    var encodedMsg = Algorand.Encoder.EncodeToMsgPack(signedTx);    
    TransactionID id = algodApiInstance.RawTransaction(encodedMsg);
    Console.WriteLine("Successfully sent tx with id: " + id.TxId);
}
catch (ApiException e)
{
    // This is generally expected, but should give us an informative error message.
    Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
}
```

这样我们就完成了一次转帐，是不是很简单。你还能从sdk-examples项目中发现更多的例子。