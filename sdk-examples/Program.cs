using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using Algorand;
using Account = Algorand.Account;
using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using Algorand.Algod.Client;
using Transaction = Algorand.Transaction;
using System.Text;
using Newtonsoft.Json;

namespace sdk_examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //var sss = Encoding.UTF8.GetBytes("this is a test");
            //var strddd = Algorand.Encoder.EncodeToMsgPack(new SignedTransaction());

            //Employee joe = new Employee
            //{
            //    Name = "Joe Employee"
            //};
            //Employee mike = new Employee
            //{
            //    Name = "Mike Manager"
            //};

            //joe.Manager = mike;

            //// mike is his own manager
            //// ShouldSerialize will skip this property
            //mike.Manager = mike;
            //var setting = new JsonSerializerSettings()
            //{
            //    ContractResolver = ShouldSerializeContractResolver.Instance
            //};
            //var str = JsonConvert.SerializeObject(mike, setting);

            //int i = 100;


            //Sha512tDigest digest = new Sha512tDigest(256);
            //var abc = digest.AlgorithmName;

            //byte[] data = new byte[0];
            //digest.BlockUpdate(data, 0, data.Length);
            //byte[] output = new byte[32];
            //digest.DoFinal(output, 0);
            //var hex = BitConverter.ToString(output);


            //SHA512 sha = new SHA512CryptoServiceProvider();
            //var shaaa = sha.ComputeHash(data);
            //var hex = BitConverter.ToString(shaaa);
            //HMACSHA512 shaa512 = new HMACSHA512();

            //var shaaa12 = BitConverter.ToString(shaa512.ComputeHash(data));
            //if (args.Length != 2)
            //{
            //    Console.WriteLine("Required parameters: ALGOD_API_ADDR ALGOD_API_TOKEN");
            //    Console.WriteLine("The parameter values can be found in algod.net and algod.token files within the data directory of your algod install.");
            //    return;
            //}

            // If the protocol is not specified in the address, http is added.
            String algodApiAddrTmp = args[0];
            if (algodApiAddrTmp.IndexOf("//") == -1)
            {
                algodApiAddrTmp = "http://" + algodApiAddrTmp;
            }

            string ALGOD_API_ADDR = algodApiAddrTmp;
            string ALGOD_API_TOKEN = args[1];
            string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
            string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";

            //Console.WriteLine("Hello World!");

            AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

            try
            {
                Supply supply = algodApiInstance.GetSupply();
                Console.WriteLine("Total Algorand Supply: " + supply.TotalMoney);
                Console.WriteLine("Online Algorand Supply: " + supply.OnlineMoney);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling algod#getSupply");
                Console.WriteLine(e.StackTrace);
            }


            // Generate a new transaction using randomly generated accounts (this is invalid, since src has no money...)
            Console.WriteLine("Attempting an invalid transaction: overspending using randomly generated accounts.");
            Console.WriteLine("Expecting overspend exception.");

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
            ulong? amount = 100000;
            ulong? lastRound = firstRound + 1000; // 1000 is the max tx window
            Account src = new Account(SRC_ACCOUNT);
            Console.WriteLine("My account address is:" + src.Address.ToString());
            var str = src.ToMnemonic();
            var bytes = Encoding.UTF8.GetBytes("examples");
            var siguture = src.SignBytes(bytes);
            //Account newAcc = new Account();
            Transaction tx = new Transaction(src.Address, new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
            SignedTransaction signedTx = src.SignTransactionWithFeePerByte(tx, feePerByte);
            Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

            // send the transaction to the network
            try
            {
                var encodedMsg = Algorand.Encoder.EncodeToMsgPack(signedTx);
                //encodedMsg = "{\"sig\":\"I8Lw9Y2jTyDexYYD9pSU+ufPCBFlHiHgpczYCClhwYwwRTnsqv5lFl+giu+cd0FQVSif3EjynFyVTp3orMZKBw==\",\"txn\":{\"amt\":100000,\"fee\":1000,\"fv\":1519490,\"gen\":\"testnet-v1.0\",\"gh\":\"SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=\",\"lv\":1520490,\"rcv\":\"VXVzKZc2E+fbEAd3/gcIgFe3NiySAReIokkAwB4Erh4=\",\"snd\":\"18tanNp1HaLZ/pgO5Dzzfusz0yrp6d5xy89Xk2oyAwQ=\",\"type\":\"pay\"}}";
                //TransactionID id = algodApiInstance.RawTransaction(encodedMsg);
                //Console.WriteLine("Successfully sent tx with id: " + id);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }







            // let's create a transaction group
            //Digest gid = TxGroup.ComputeGroupID(new Transaction[] { tx, tx });
            //tx.AssignGroupID(gid);
            //signedTx = src.SignTransactionWithFeePerByte(tx, feePerByte);
            //try
            //{
            //    byte[] encodedTxBytes = Algorand.Encoder.EncodeToMsgPack(signedTx);
            //    byte[] concat = JavaHelper<byte>.ArrayCopyOf(encodedTxBytes, encodedTxBytes.Length + encodedTxBytes.Length);
            //    JavaHelper<byte>.SyatemArrayCopy(encodedTxBytes, 0, concat, encodedTxBytes.Length, encodedTxBytes.Length);
            //    TransactionID id = algodApiInstance.RawTransaction(concat);
            //    Console.WriteLine("Successfully sent tx group with first tx id: " + id);
            //}
            //catch (ApiException e)
            //{
            //    // This is generally expected, but should give us an informative error message.
            //    Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            //}

            //// format and send logic sig
            //byte[] program = { 0x01, 0x20, 0x01, 0x00, 0x22 };

            //LogicsigSignature lsig = new LogicsigSignature(program, null);
            //Console.WriteLine("Escrow address: " + lsig.ToAddress().ToString());

            //tx = new Transaction(lsig.ToAddress(), new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
            //if (!lsig.Verify(tx.sender))
            //{
            //    String msg = "Verification failed";
            //    Console.WriteLine(msg);
            //}
            //else
            //{
            //    try
            //    {
            //        SignedTransaction stx = Account.SignLogicsigTransaction(lsig, tx);
            //        byte[] encodedTxBytes = Algorand.Encoder.EncodeToMsgPack(signedTx);
            //        TransactionID id = algodApiInstance.RawTransaction(encodedTxBytes);
            //        Console.WriteLine("Successfully sent tx logic sig tx id: " + id);
            //    }
            //    catch (ApiException e)
            //    {
            //        // This is generally expected, but should give us an informative error message.
            //        Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            //    }
            //}
        }
    }



}
