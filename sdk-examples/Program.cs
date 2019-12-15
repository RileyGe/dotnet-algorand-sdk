using System;
using System.Collections.Generic;
using Algorand;
using Account = Algorand.Account;
using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using Algorand.Algod.Client;
using Transaction = Algorand.Transaction;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;

namespace sdk_examples
{
    class Program
    {
        static void Main(string[] args)
        {
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
                Console.WriteLine("Exception when calling algod#getSupply:" + e.Message);
                //Console.WriteLine(e.StackTrace);
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
            var adrvalid = Address.IsValid(DEST_ADDR);
            Transaction tx = new Transaction(src.Address, new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
            SignedTransaction signedTx = src.SignTransactionWithFeePerByte(tx, feePerByte);
            //var signedTrans = src.SignTransaction(tx);

            //Console.WriteLine("signed transaction:" + signedTrans.transactionID);
            Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

            // send the transaction to the network
            try
            {
                var encodedMsg = Algorand.Encoder.EncodeToMsgPack(signedTx);
                //var str222 = Algorand.Encoder.EncodeToJson(signedTx);
                //encodedMsg = "{\"sig\":\"I8Lw9Y2jTyDexYYD9pSU+ufPCBFlHiHgpczYCClhwYwwRTnsqv5lFl+giu+cd0FQVSif3EjynFyVTp3orMZKBw==\",\"txn\":{\"amt\":100000,\"fee\":1000,\"fv\":1519490,\"gen\":\"testnet-v1.0\",\"gh\":\"SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=\",\"lv\":1520490,\"rcv\":\"VXVzKZc2E+fbEAd3/gcIgFe3NiySAReIokkAwB4Erh4=\",\"snd\":\"18tanNp1HaLZ/pgO5Dzzfusz0yrp6d5xy89Xk2oyAwQ=\",\"type\":\"pay\"}}";
                TransactionID id = algodApiInstance.RawTransaction(encodedMsg);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }


            // let's create a transaction group
            Transaction tx2 = new Transaction(src.Address, new Address("OAMCXDCH7LIVYUF2HSNQLPENI2ZXCWBSOLUAOITT47E4FAMFGAMI4NFLYU"), amount, firstRound, lastRound, genesisID, genesisHash);
            //SignedTransaction signedTx2 = src.SignTransactionWithFeePerByte(tx2, feePerByte);
            Digest gid = TxGroup.ComputeGroupID(new Transaction[] { tx, tx2 });
            tx.AssignGroupID(gid);
            tx2.AssignGroupID(gid);
            // already updated the groupid, sign again
            signedTx = src.SignTransactionWithFeePerByte(tx, feePerByte);
            var signedTx2 = src.SignTransactionWithFeePerByte(tx2, feePerByte);
            try
            {
                List<byte> byteList = new List<byte>(Algorand.Encoder.EncodeToMsgPack(signedTx));
                //byte[] encodedTxBytes = ;
                //byte[] concat = JavaHelper<byte>.ArrayCopyOf(encodedTxBytes, encodedTxBytes.Length + encodedTxBytes.Length);
                //JavaHelper<byte>.SyatemArrayCopy(encodedTxBytes, 0, concat, encodedTxBytes.Length, encodedTxBytes.Length);
                byteList.AddRange(Algorand.Encoder.EncodeToMsgPack(signedTx2));
                TransactionID id = algodApiInstance.RawTransaction(byteList.ToArray());
                Console.WriteLine("Successfully sent tx group with first tx id: " + id);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }

            // format and send logic sig
            byte[] program = { 0x01, 0x20, 0x01, 0x00, 0x22 };

            LogicsigSignature lsig = new LogicsigSignature(program, null);
            Console.WriteLine("Escrow address: " + lsig.ToAddress().ToString());

            tx = new Transaction(lsig.ToAddress(), new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
            if (!lsig.Verify(tx.sender))
            {
                string msg = "Verification failed";
                Console.WriteLine(msg);
            }
            else
            {
                try
                {
                    SignedTransaction stx = Account.SignLogicsigTransaction(lsig, tx);
                    byte[] encodedTxBytes = Algorand.Encoder.EncodeToMsgPack(signedTx);
                    TransactionID id = algodApiInstance.RawTransaction(encodedTxBytes);
                    Console.WriteLine("Successfully sent tx logic sig tx id: " + id);
                }
                catch (ApiException e)
                {
                    // This is generally expected, but should give us an informative error message.
                    Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
                }
            }


            //MultisigTransaction
            // List for Pks for multisig account
            List<Ed25519PublicKeyParameters> publicKeys = new List<Ed25519PublicKeyParameters>();

            // Create 3 random new account
            Account act1 = new Account("solve gravity slight leader net silver enlist harsh apple shoulder question child material network lumber lion wagon filter cabin shoot raven barely next abandon tired");
            Account act2 = new Account("scan acoustic prefer duck this error intact blush nominee woman retreat install picture lion fruit consider sail basic kind owner grocery ginger piece abandon wife");
            Account act3 = new Account("advance main into silver law unfold cable indoor hockey legend chat pelican hobby knock symptom until travel olive quality melody toast pizza inspire absorb limit");

            publicKeys.Add(act1.GetEd25519PublicKey());
            publicKeys.Add(act2.GetEd25519PublicKey());
            publicKeys.Add(act3.GetEd25519PublicKey());

            // Instantiate the the Multisig Accout
            MultisigAddress msa = new MultisigAddress(1, 2, publicKeys);
            Console.WriteLine("Multisignature Address: " + msa.ToString());
            Console.WriteLine("no algo in the random adress, use TestNet Dispenser to add funds");
            //no algo in the random adress, use TestNet Dispenser to add funds
            //Console.ReadKey();

            // add some notes to the transaction
            byte[] notes = Encoding.UTF8.GetBytes("These are some notes encoded in some way!");//.getBytes();

            //BigInteger amount = BigInteger.valueOf(2000000);
            //BigInteger lastRound = firstRound.add(BigInteger.valueOf(1000)); // 1000 is the max tx window
                                                                             // Setup Transaction
                                                                             // Use a fee of 0 as we will set the fee per
                                                                             // byte when we sign the tx and overwrite it
            Transaction tx3 = new Transaction(new Address(msa.ToString()), 1000, firstRound, lastRound,
                    notes, amount, new Address(DEST_ADDR), genesisID, genesisHash);
            // Sign the Transaction for two accounts
            SignedTransaction signedTx3 = act1.SignMultisigTransaction(msa, tx3);
            SignedTransaction completeTx = act2.AppendMultisigTransaction(msa, signedTx3);

            // send the transaction to the network
            try
            {
                // Msgpack encode the signed transaction
                byte[] encodedTxBytes = Algorand.Encoder.EncodeToMsgPack(completeTx);
                string encodedStr = Algorand.Encoder.EncodeToJson(completeTx);
                TransactionID id = algodApiInstance.RawTransaction(encodedTxBytes);
                Console.WriteLine("Successfully sent tx with id: " + id);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }
        }
    }



}
