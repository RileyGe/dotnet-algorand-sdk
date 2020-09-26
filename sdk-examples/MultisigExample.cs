using Algorand;
using Algorand.Client;
using Algorand.Algod.Api;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace sdk_examples
{
    public class MultisigExample
    {
        public static void Main(params string[] args)
        {
            string algodApiAddrTmp = args[0];
            if (algodApiAddrTmp.IndexOf("//") == -1)
            {
                algodApiAddrTmp = "http://" + algodApiAddrTmp;
            }

            string ALGOD_API_ADDR = algodApiAddrTmp;
            string ALGOD_API_TOKEN = args[1];

            //    AlgodClient client = new AlgodClient().SetBasePath(ALGOD_API_ADDR);
            //ApiKeyAuth api_key = (ApiKeyAuth)client.getAuthentication("api_key");
            //api_key.setApiKey(ALGOD_API_TOKEN);
            AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

            //MultisigTransaction
            // List for Pks for multisig account
            List<Ed25519PublicKeyParameters> publicKeys = new List<Ed25519PublicKeyParameters>();

            // Create 3 random new account
            Account act1 = new Account("solve gravity slight leader net silver enlist harsh apple shoulder question child material network lumber lion wagon filter cabin shoot raven barely next abandon tired");
            Account act2 = new Account("scan acoustic prefer duck this error intact blush nominee woman retreat install picture lion fruit consider sail basic kind owner grocery ginger piece abandon wife");
            Account act3 = new Account("advance main into silver law unfold cable indoor hockey legend chat pelican hobby knock symptom until travel olive quality melody toast pizza inspire absorb limit");
            string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";

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

            //ulong? feePerByte;
            //string genesisID;
            //Digest genesisHash;
            //ulong? firstRound = 0;
            //Algorand.Algod.Client.Model.TransactionParams transParams = null;
            var amount = Utils.AlgosToMicroalgos(1);
            Transaction tx = null;
            try
            {
                tx = Utils.GetPaymentTransaction(new Address(msa.ToString()), new Address(DEST_ADDR), amount, "this is a multisig trans",
                    algodApiInstance.TransactionParams());        
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not get params", e.Message);
            }
            //BigInteger amount = BigInteger.valueOf(2000000);
            //BigInteger lastRound = firstRound.add(BigInteger.valueOf(1000)); // 1000 is the max tx window
            // Setup Transaction
            // Use a fee of 0 as we will set the fee per
            // byte when we sign the tx and overwrite it
            
            //var tx = Utils.GetPaymentTransaction(new Address(msa.ToString()), new Address(DEST_ADDR), amount, "this is a multisig trans", transParams);
            //Transaction tx = new Transaction(new Address(msa.ToString()), transParams.Fee, transParams.LastRound, transParams.LastRound + 1000,
            //        notes, amount, new Address(DEST_ADDR), transParams.GenesisID, new Digest(transParams.Genesishashb64));
            // Sign the Transaction for two accounts
            SignedTransaction signedTx = act1.SignMultisigTransaction(msa, tx);
            SignedTransaction completeTx = act2.AppendMultisigTransaction(msa, signedTx);

            // send the transaction to the network
            try
            {
                var id = Utils.SubmitTransaction(algodApiInstance, completeTx);
                Console.WriteLine("Successfully sent tx with id: " + id);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            Console.ReadKey();
        }
    }
}
