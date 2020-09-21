# Dotnet Algorand SDK

The SDK already update to version 0.2 and add the support of Algorand Api 2.0 and the Indexer Api.



Finally, the dotnet algorand sdk is ready for use, give it a try!

1. Prerequisites

This library is compliant to .Net Standard 2.0, so you can use this library on any planform which can use the .Net Standard 2.0.

2. How to Install

Open the NuGet command line and type:

```powershell
Install-Package Algorand
```

3. Quick Start

```csharp
string ALGOD_API_ADDR = "your algod api address";
//string ALGOD_API_ADDR = "http://hackathon.algodev.network:9100";  //hackathon
string ALGOD_API_TOKEN = "your algod api token"; //find in the algod.token
//string ALGOD_API_TOKEN = "ef920e2e7e002953f4b29a8af720efe8e4ecc75ff102b165e0472834b25832c1";
AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
```

Get information from algorand blockchain:

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
ulong? firstRound = 0;
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

If you want to go furfure, you should have an account.  You can use `Account acc = new Account();` to generate a random account. Surely you can use mnemonic to create an account. The example below using mnemonic to create an account and send some algos to another address.

```csharp
ulong? amount = 100000;
ulong? lastRound = firstRound + 1000; // 1000 is the max tx window
string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
Account src = new Account(SRC_ACCOUNT);
Console.WriteLine("My account address is:" + src.Address.ToString());
string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
Transaction tx = new Transaction(src.Address, new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
//sign the transaction before send it to the blockchain
SignedTransaction signedTx = src.SignTransactionWithFeePerByte(tx, feePerByte);
Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);
// send the transaction to the network
try
{
    //encode to msg-pack
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

That's all? Yes this is a complete example, you can find more examples in the sdk-examples project.