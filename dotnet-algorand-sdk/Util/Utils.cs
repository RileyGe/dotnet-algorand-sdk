using Algorand.Algod.Api;
using Algorand.Algod.Model;
using Algorand.V2.Algod.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AssetParams = Algorand.Algod.Model.AssetParams;

namespace Algorand
{
    /// <summary>
    /// Convenience methods for algorand sdk.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// wait transaction to complete
        /// </summary>
        /// <param name="instance">The algod api instance using algod v1 API</param>
        /// <param name="txID">transaction ID</param>
        /// <returns></returns>
        public static string WaitTransactionToComplete(AlgodApi instance, string txID)
        {
            while (true)
            {
                //Check the pending tranactions
                var b3 = instance.PendingTransactionInformation(txID);
                if (b3.Round != null && b3.Round > 0)
                {
                    return "Transaction " + b3.Tx + " confirmed in round " + b3.Round;
                }
                // loops 4 times per second, > 5 times per second will fail using TestNet Purestake Free verison  
                // also blocks are created in under 5 seconds so no real need to poll constantly - 
                // a few times per second should be fine
                System.Threading.Thread.Sleep(250);             
            }
        }
        /// <summary>
        /// utility function to wait on a transaction to be confirmed using algod v2 API
        /// </summary>
        /// <param name="instance">The algod api instance using algod v2 API</param>
        /// <param name="txID">transaction ID</param>
        /// <param name="timeout">how many rounds do you wish to check pending transactions for</param>
        /// <returns>The pending transaction response</returns>
        public static async Task<V2.Algod.Model.PendingTransactionResponse> WaitTransactionToComplete(V2.Algod.DefaultApi instance, string txID, ulong timeout = 3) 
        {

            if (instance == null || txID == null || txID.Length == 0 || timeout < 0)
            {
                throw new ArgumentException("Bad arguments for waitForConfirmation.");
            }
            V2.Algod.Model.NodeStatusResponse nodeStatusResponse = await instance.StatusAsync();            
            var startRound = nodeStatusResponse.LastRound + 1;
            var currentRound = startRound;
            while (currentRound < (startRound + timeout))
            {
                var pendingInfo = await instance.PendingGetAsync(txID,null);

                if (pendingInfo != null)
                {
                    if (pendingInfo.ConfirmedRound != null && pendingInfo.ConfirmedRound > 0)
                    {
                        // Got the completed Transaction
                        return pendingInfo;
                    }
                    if (pendingInfo.PoolError != null && pendingInfo.PoolError.Length > 0)
                    {
                        // If there was a pool error, then the transaction has been rejected!
                        throw new Exception("The transaction has been rejected with a pool error: " + pendingInfo.PoolError);
                    }
                }
                await instance.WaitForBlockAfterAsync(currentRound);
                currentRound++;
            }
            throw new Exception("Transaction not confirmed after " + timeout + " rounds!");
        }
        /// <summary>
        /// encode and submit signed transaction
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="signedTx"></param>
        /// <returns></returns>
        public static TransactionID SubmitTransaction(AlgodApi instance, SignedTransaction signedTx) //throws Exception
        {
            byte[] encodedTxBytes = Encoder.EncodeToMsgPack(signedTx);             
            return instance.RawTransaction(encodedTxBytes);
        }
        /// <summary>
        /// encode and submit signed transaction using algod v2 api
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="signedTx"></param>
        /// <returns></returns>
        public static async Task<V2.Algod.Model.PostTransactionsResponse> SubmitTransaction(V2.Algod.DefaultApi instance, SignedTransaction signedTx) //throws Exception
        {
            byte[] encodedTxBytes = Encoder.EncodeToMsgPack(signedTx);
            using (MemoryStream ms = new MemoryStream(encodedTxBytes))
            {
                return await instance.TransactionsAsync(ms);
            }
        }        
        /// <summary>
        /// encode and submit signed transactions using algod v2 api
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="signedTx"></param>
        /// <returns></returns>
        public static async Task<V2.Algod.Model.PostTransactionsResponse> SubmitTransactions(V2.Algod.DefaultApi instance, IEnumerable<SignedTransaction> signedTxs) //throws Exception
        {
            List<byte> byteList = new List<byte>();
            foreach (var signedTx in signedTxs)
            {
                byteList.AddRange(Algorand.Encoder.EncodeToMsgPack(signedTx));
            }
            using (MemoryStream ms = new MemoryStream(byteList.ToArray()))
            {
                return await instance.TransactionsAsync(ms);
            }
        }
        public static ulong AlgosToMicroalgos(double algos)
        {
            return Convert.ToUInt64(Math.Floor(algos * 1000000));
        }
        public static double MicroalgosToAlgos(ulong microAlgos)
        {
            return microAlgos / 1000000.0;
        }
        /// <summary>
        /// Get a payment transaction
        /// </summary>
        /// <param name="from">from address</param>
        /// <param name="to">to address</param>
        /// <param name="amount">amount(Unit:MicroAlgo)</param>
        /// <param name="message">message</param>
        /// <param name="trans">Transaction Params(use AlgodApi.TransactionParams() function to get the params)</param>
        /// <returns>payment transaction</returns>
        public static Transaction GetPaymentTransaction(Address from, Address to, ulong? amount, string message, TransactionParams trans)
        {
            if (trans is null)
                throw new Exception("The Transaction Params can not be null!");
            return GetPaymentTransactionWithInfo(from, to, amount, message, trans.Fee, trans.LastRound, trans.GenesisID, trans.Genesishashb64);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="amount"></param>
        /// <param name="message"></param>
        /// <param name="fee"></param>
        /// <param name="lastRound"></param>
        /// <param name="genesisId"></param>
        /// <param name="genesishashb64"></param>
        /// <returns></returns>
        public static Transaction GetPaymentTransactionWithInfo(Address from, Address to, ulong? amount, string message, 
            ulong? fee, ulong? lastRound, string genesisId, string genesishashb64)
        {
            var tx = GetPaymentTransactionWithFlatFee(from, to, amount, message, fee, lastRound, genesisId, genesishashb64);
            Account.SetFeeByFeePerByte(tx, fee);
            return tx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="amount"></param>
        /// <param name="message"></param>
        /// <param name="flatFee"></param>
        /// <param name="lastRound"></param>
        /// <param name="genesisId"></param>
        /// <param name="genesishashb64"></param>
        /// <returns></returns>
        public static Transaction GetPaymentTransactionWithFlatFee(Address from, Address to, ulong? amount, string message,
            ulong? flatFee, ulong? lastRound, string genesisId, string genesishashb64)
        {
            var notes = message is null ? null : Encoding.UTF8.GetBytes(message);
            var tx = new Transaction(from, flatFee, lastRound, lastRound + 1000,
                    notes, amount, to, genesisId, new Digest(genesishashb64));
            return tx;
        }

        /// <summary>
        /// Get a payment transaction
        /// </summary>
        /// <param name="from">from address</param>
        /// <param name="to">to address</param>
        /// <param name="amount">amount(Unit:MicroAlgo)</param>
        /// <param name="message">message</param>
        /// <param name="trans">Transaction Params(use AlgodApi.TransactionParams() function to get the params)</param>
        /// <returns>payment transaction</returns>
        public static Transaction GetPaymentTransaction(Address from, Address to, ulong? amount, string message, 
            TransactionParametersResponse trans)
        {
            if (trans is null)
                throw new Exception("The Transaction Params can not be null!");
            return GetPaymentTransactionWithInfo(from, to, amount, message,
                (ulong?)trans.Fee, (ulong?)trans.LastRound, trans.GenesisId, Convert.ToBase64String(trans.GenesisHash));
        }
        /// <summary>
        /// Generate a 32 bytes string for asset metadata hash
        /// </summary>
        /// <returns>a 32 bytes string</returns>
        public static string GetRandomAssetMetaHash()
        {
            Random rd = new Random();
            byte[] bts = new byte[32];
            rd.NextBytes(bts);
            //var base64 = Convert.ToBase64String(bts);
            return Convert.ToBase64String(bts);
        }
        /// <summary>
        /// Generate a create asset transaction
        /// </summary>
        /// <param name="asset">The asset infomation</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <param name="decimals">A value of 0 represents an asset that is not divisible, 
        /// while a value of 1 represents an asset that is divisible into tenths and so on, i.e, 
        /// the number of digits to display after the decimal place when displaying the asset. 
        /// This value must be between 0 and 19</param>
        /// <returns>transaction</returns>
        public static Transaction GetCreateAssetTransaction(AssetParams asset, TransactionParams trans, string message = "", int decimals = 0, ulong? flatFee = null)
        {

            ValidateAsset(asset);

            Address sender = new Address(asset.Creator);
            ulong? fee = trans.Fee;
            ulong? firstValid = trans.LastRound;
            ulong? lastValid = trans.LastRound + 1000;
            byte[] note = Encoding.UTF8.GetBytes(message);
            string genesisID = trans.GenesisID;
            Digest genesisHash = new Digest(trans.Genesishashb64);
            ulong? assetTotal = asset.Total;
            int assetDecimals = decimals;
            bool defaultFrozen = asset.Defaultfrozen ?? false;
            string assetUnitName = asset.Unitname;
            string assetName = asset.Assetname;
            string url = asset.Url;
            byte[] metadataHash = Convert.FromBase64String(asset.Metadatahash);
            Address manager = new Address(asset.Managerkey);
            Address reserve = new Address(asset.Reserveaddr);
            Address freeze = new Address(asset.Freezeaddr);
            Address clawback = new Address(asset.Clawbackaddr);

            // assetDecimals is not exist in api, so set as zero in this version
            var tx = Transaction.CreateAssetCreateTransaction(sender, fee, firstValid, lastValid, note,
                           genesisID, genesisHash, assetTotal, assetDecimals, defaultFrozen,
                           assetUnitName, assetName, url, metadataHash,
                           manager, reserve, freeze, clawback);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }

            return tx;
        }        

        /// <summary>
        /// Generate a create asset transaction V2
        /// </summary>
        /// <param name="asset">The asset infomation</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <param name="decimals">A value of 0 represents an asset that is not divisible, 
        /// while a value of 1 represents an asset that is divisible into tenths and so on, i.e, 
        /// the number of digits to display after the decimal place when displaying the asset. 
        /// This value must be between 0 and 19</param>
        /// <returns>transaction</returns>
        public static Transaction GetCreateAssetTransaction(V2.Algod.Model.AssetParams asset, V2.Algod.Model.TransactionParametersResponse trans, string message = "", ulong? flatFee = null)
        {
            ValidateAsset(asset);
            // assetDecimals is not exist in api, so set as zero in this version
            var tx = Transaction.CreateAssetCreateTransaction(new Address(asset.Creator), (ulong?)trans.Fee, (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000,
                Encoding.UTF8.GetBytes(message), trans.GenesisId, new Digest(trans.GenesisHash),
                asset.Total, (int)asset.Decimals, (bool)asset.DefaultFrozen, asset.UnitName, asset.Name, asset.Url,
                asset.MetadataHash, 
                asset.Manager==""||asset.Manager==null ? null : new Address(asset.Manager), 
                asset.Reserve==""||asset.Reserve==null ? null : new Address(asset.Reserve),
                asset.Freeze==""||asset.Freeze==null ? null : new Address(asset.Freeze), 
                asset.Clawback==""||asset.Clawback==null ? null : new Address(asset.Clawback));

            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }            
            return tx;
        }
        /// <summary>
        /// Generate an asset config transaction
        /// </summary>
        /// <param name="sender">The sender of the transaction, should be the manager of the asset.</param>
        /// <param name="assetId">Asset ID</param>
        /// <param name="asset">Asset infomation</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <returns>transaction</returns>
        public static Transaction GetConfigAssetTransaction(Address sender, ulong? assetId, AssetParams asset, TransactionParams trans, string message = "", ulong? flatFee = null)
        {
            ValidateAsset(asset);
            //sender must be manager
            var tx = Transaction.CreateAssetConfigureTransaction(sender, 1,
                trans.LastRound, trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID,
                new Digest(trans.Genesishashb64), assetId, new Address(asset.Managerkey), 
                new Address(asset.Reserveaddr), new Address(asset.Freezeaddr), new Address(asset.Clawbackaddr), false);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            return tx;
        }

        public static Transaction GetConfigAssetTransaction(Address sender, V2.Algod.Model.Asset asset, TransactionParametersResponse trans, 
            string message = "", ulong? flatFee = null)
        {
            ValidateAsset(asset.Params);
            //sender must be manager
            var tx = Transaction.CreateAssetConfigureTransaction(sender, 1,
                (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)asset.Index, new Address(asset.Params.Manager),
                new Address(asset.Params.Reserve), new Address(asset.Params.Freeze), new Address(asset.Params.Clawback), false);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            return tx;
        }

        /// <summary>
        /// DEPRECATED
        /// Please use GetAssetOptingInTransaction.
        /// Generate an opting in asset transaction
        /// All accounts that want recieve the new asset have to opt in
        /// </summary>
        /// <param name="sender">The account want to opt in</param>
        /// <param name="assetId">Asset ID</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <returns>transaction</returns>
        public static Transaction GetActivateAssetTransaction(Address sender, ulong? assetId, TransactionParams trans, 
            string message = "", ulong? flatFee = null)
        {            
            return GetAssetOptingInTransaction(sender, assetId, trans, message, flatFee);
        }
        /// <summary>
        /// Generate an opting in asset transaction
        /// All accounts that want recieve the new asset have to opt in
        /// </summary>
        /// <param name="sender">The account want to opt in</param>
        /// <param name="assetId">Asset ID</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <returns>transaction</returns>
        public static Transaction GetAssetOptingInTransaction(Address sender, ulong? assetId, TransactionParams trans, 
            string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetAcceptTransaction(sender, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID,
                new Digest(trans.Genesishashb64), assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetAssetOptingInTransaction(Address sender, ulong? assetID, TransactionParametersResponse trans, 
            string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetAcceptTransaction(sender, 1, (ulong?)trans.LastRound,
                (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)assetID);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            return tx;
        }

        public static Transaction GetTransferAssetTransaction(Address from, Address to, ulong? assetId, ulong amount, TransactionParams trans, 
            Address closeTo = null, string message = "", ulong? flatFee = null) {
            var tx = Transaction.CreateAssetTransferTransaction(from, to, closeTo, amount, 1,
                trans.LastRound, trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID, 
                new Digest(trans.Genesishashb64), assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetTransferAssetTransaction(Address from, Address to, ulong? assetId, ulong amount, 
            TransactionParametersResponse trans, Address closeTo = null, string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetTransferTransaction(from, to, closeTo, amount, 1,
                (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            return tx;
        }

        /// <summary>
        /// Generate an asset freeze transaction
        /// </summary>
        /// <param name="sender">The sender of the transaction, should be the freezer of the asset.</param>
        /// <param name="toFreeze">The account which hold the asset will be frozen.</param>
        /// <param name="assetId">Asset ID</param>
        /// <param name="freezeState">true to frozen the asset, false to unfrozen the asset.</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <returns>transaction</returns>
        public static Transaction GetFreezeAssetTransaction(Address sender, Address toFreeze, ulong? assetId, bool freezeState, 
            TransactionParams trans, string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetFreezeTransaction(sender, toFreeze, freezeState, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), new Digest(trans.Genesishashb64), assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }

        public static Transaction GetFreezeAssetTransaction(Address sender, Address toFreeze, ulong? assetId, bool freezeState, 
            TransactionParametersResponse trans, string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetFreezeTransaction(sender, toFreeze, freezeState, 1, (ulong?)trans.LastRound,
                (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), new Digest(trans.GenesisHash), (ulong?)assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            return tx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reserve"></param>
        /// <param name="revokedFrom"></param>
        /// <param name="receiver"></param>
        /// <param name="assetId"></param>
        /// <param name="amount"></param>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Transaction GetRevokeAssetTransaction(Address reserve, Address revokedFrom, Address receiver, ulong? assetId, 
            ulong amount, TransactionParams trans, string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetRevokeTransaction(reserve, revokedFrom, receiver, amount, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID, 
                new Digest(trans.Genesishashb64), assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reserve"></param>
        /// <param name="revokedFrom"></param>
        /// <param name="receiver"></param>
        /// <param name="assetId"></param>
        /// <param name="amount"></param>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        /// <param name="flatFee"></param>
        /// <returns></returns>
        public static Transaction GetRevokeAssetTransaction(Address reserve, Address revokedFrom, Address receiver, ulong? assetId, 
            ulong amount, TransactionParametersResponse trans, string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetRevokeTransaction(reserve, revokedFrom, receiver, amount, 1, (ulong?)trans.LastRound,
                (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            return tx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="assetId"></param>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Transaction GetDestroyAssetTransaction(Address manager, ulong? assetId, TransactionParams trans, 
            string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetDestroyTransaction(manager, 1, trans.LastRound, trans.LastRound + 1000, 
                Encoding.UTF8.GetBytes(message), new Digest(trans.Genesishashb64), assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetDestroyAssetTransaction(Address manager, ulong? assetId, TransactionParametersResponse trans, 
            string message = "", ulong? flatFee = null)
        {
            var tx = Transaction.CreateAssetDestroyTransaction(manager, 1, (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000,
                Encoding.UTF8.GetBytes(message), new Digest(trans.GenesisHash), (ulong?)assetId);
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            return tx;
        }
        private static void ValidateAsset(AssetParams asset)
        {
            if (asset.Creator is null || asset.Creator == "") throw new ArgumentException("The sender must be specified.");
            else if (!Address.IsValid(asset.Creator)) throw new ArgumentException("The sender address is not valid.");
            
            if (asset.Assetname is null || asset.Assetname == "") throw new ArgumentException("The asset name must be specified.");
            
            if (asset.Unitname is null || asset.Unitname == "") throw new ArgumentException("The unit name must be specified.");
            else if (asset.Unitname.Length > 8) throw new ArgumentException(string.Format("The length of unit name is {0} > 8.", asset.Unitname.Length));
            
            if (asset.Total is null || asset.Total < 1) throw new ArgumentException("The total number of the asset must be specified and bigger than zero.");
            
            if (asset.Managerkey is null) asset.Managerkey = asset.Creator;
            else if (asset.Managerkey != "" && !Address.IsValid(asset.Managerkey)) throw new ArgumentException("The manager address is not valid.");
            
            if (asset.Reserveaddr is null) asset.Reserveaddr = asset.Managerkey;
            else if (asset.Reserveaddr != "" && !Address.IsValid(asset.Reserveaddr)) throw new ArgumentException("The reserve address is not valid.");
            
            if (asset.Freezeaddr is null) asset.Freezeaddr = asset.Managerkey;
            else if (asset.Freezeaddr != "" && !Address.IsValid(asset.Freezeaddr)) throw new ArgumentException("The freeze address is not valid.");
            
            if (asset.Clawbackaddr is null) asset.Clawbackaddr = asset.Managerkey;
            else if (asset.Clawbackaddr != "" && !Address.IsValid(asset.Clawbackaddr)) throw new ArgumentException("The clawback address is not valid.");
            
            if (asset.Metadatahash is null || asset.Metadatahash == "")
                asset.Metadatahash = GetRandomAssetMetaHash();//auto generate metahash by sdk
            else if (Convert.FromBase64String(asset.Metadatahash).Length != 32)
                throw new ArgumentException("The metadata hash should be 32 bytes.");
        }

        private static void ValidateAsset(V2.Algod.Model.AssetParams asset)
        {
            if (asset.Creator is null || asset.Creator == "") throw new ArgumentException("The sender must be specified.");
            else if (!Address.IsValid(asset.Creator)) throw new ArgumentException("The sender address is not valid.");
            
            if (asset.Name is null || asset.Name == "") throw new ArgumentException("The asset name must be specified.");
            
            if (asset.UnitName is null || asset.UnitName == "") throw new ArgumentException("The unit name must be specified.");
            else if (asset.UnitName.Length > 8) throw new ArgumentException(string.Format("The length of unit name is {0} > 8.", asset.UnitName.Length));
            
            if (asset.Total is null || asset.Total < 1) throw new ArgumentException("The total number of the asset must be specified and bigger than zero.");
            
            if (asset.Manager is null) asset.Manager = asset.Creator;
            else if (asset.Manager != "" && !Address.IsValid(asset.Manager)) throw new ArgumentException("The manager address is not valid.");
            
            if (asset.Reserve is null) asset.Reserve = asset.Manager;
            else if (asset.Reserve != "" && !Address.IsValid(asset.Reserve)) throw new ArgumentException("The reserve address is not valid.");
            
            if (asset.Freeze is null) asset.Freeze = asset.Manager;
            else if (asset.Freeze != "" && !Address.IsValid(asset.Freeze)) throw new ArgumentException("The freeze address is not valid.");
            
            if (asset.Clawback is null) asset.Clawback = asset.Manager;
            else if (asset.Clawback != "" && !Address.IsValid(asset.Clawback)) throw new ArgumentException("The clawback address is not valid.");
            
            if (asset.MetadataHash is null || asset.MetadataHash.Length == 0)
                asset.MetadataHash = Encoding.UTF8.GetBytes(GetRandomAssetMetaHash());//auto generate metahash by sdk
            else if (asset.MetadataHash.Length != 32)
                throw new ArgumentException("The metadata hash should be 32 bytes.");

            if (asset.DefaultFrozen is null) asset.DefaultFrozen = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bidder"></param>
        /// <param name="auction"></param>
        /// <param name="signedBid"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static Transaction GetBidTransaction(Address bidder, Address auction, SignedBid signedBid, TransactionParams trans, ulong? flatFee = null)
        {
            var tx = new Transaction(bidder, auction, 0, trans.LastRound, trans.LastRound + 1000,
                trans.GenesisID, new Digest(trans.Genesishashb64))
            {
                note = Encoder.EncodeToMsgPack(signedBid)
            };
            if (flatFee is null || flatFee == 0)
            {
                Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            }
            else
            {
                tx.fee = flatFee;
            }
            //Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="firstValid"></param>
        ///// <param name="lastValid"></param>
        ///// <param name="genesishashb64"></param>
        ///// <param name="onCompletion"></param>
        ///// <param name="applicationId"></param>
        ///// <param name="applicationArgs"></param>
        ///// <param name="accounts"></param>
        ///// <param name="foreignApps"></param>
        ///// <param name="foreignAssets"></param>
        ///// <returns></returns>
        //public static Transaction GetApplicationTransaction(Address sender, long firstValid, long lastValid, string genesishashb64, 
        //    OnCompletion onCompletion, long applicationId, List<byte[]> applicationArgs, List<Address> accounts, List<long> foreignApps, List<long> foreignAssets)
        //{
        //    var txn = new Transaction
        //    {
        //        type = Transaction.Type.ApplicationCall,
        //        sender = sender,
        //        firstValid = (ulong)firstValid,
        //        lastValid = (ulong)lastValid,
        //        genesisHash = new Digest(genesishashb64)
        //    };

        //    // Global requirements
        //    //Objects.requireNonNull(onCompletion, "OnCompletion is required, please file a bug report.");
        //    //Objects.requireNonNull(applicationId);
        //    //JavaHelper<long>.RequireNotNull()

        //    if (applicationId >= 0) txn.applicationId = (ulong?)applicationId;
        //    else throw new ArgumentException("Please set right application Id.");
        //    //if (onCompletion != new OnCompletion()) 
        //        txn.onCompletion = onCompletion;
        //    //else throw new ArgumentException("OnCompletion is required, please file a bug report.");
        //    if (applicationArgs != null) txn.applicationArgs = applicationArgs;
        //    if (accounts != null) txn.accounts = accounts;
        //    if (foreignApps != null) txn.foreignApps = foreignApps;
        //    if (foreignAssets != null) txn.foreignAssets = foreignAssets;
        //    return txn;
        //}
        /// <summary>
        /// create application tranaction
        /// </summary>
        /// <param name="sender">sender of the transaction</param>
        /// <param name="approvalProgram">approval program</param>
        /// <param name="clearProgram">clear program</param>
        /// <param name="globalSchema">global schema</param>
        /// <param name="localSchema">local schema</param>
        /// <param name="trans">suggested transaction params</param>
        /// <returns>create application tranaction</returns>
        public static Transaction GetApplicationCreateTransaction(Address sender, TEALProgram approvalProgram, TEALProgram clearProgram,
            V2.Indexer.Model.StateSchema globalSchema, V2.Indexer.Model.StateSchema localSchema, TransactionParametersResponse trans)
        {
            var fee = (ulong?)trans.Fee;
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong?)trans.LastRound,
                lastValid = (ulong?)trans.LastRound + 1000,
                genesisID = trans.GenesisId,
                genesisHash = new Digest(trans.GenesisHash),
                fee = fee >= 1000 ? fee : 1000,
                approvalProgram = approvalProgram,
                clearStateProgram = clearProgram,
                onCompletion = V2.Indexer.Model.OnCompletion.Noop,
                globalStateSchema = globalSchema,
                localStateSchema = localSchema
            };
            return txn;
        }
        /// <summary>
        /// optin application tranaction
        /// </summary>
        /// <param name="sender">sender of the transaction</param>
        /// <param name="applicationId">application id</param>
        /// <param name="trans">suggested transaction params</param>
        /// <returns>optin application tranaction</returns>
        public static Transaction GetApplicationOptinTransaction(Address sender, ulong? applicationId, TransactionParametersResponse trans)
        {
            var fee = (ulong?)trans.Fee;
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong?)trans.LastRound,
                lastValid = (ulong?)trans.LastRound + 1000,
                genesisID = trans.GenesisId,
                genesisHash = new Digest(trans.GenesisHash),
                fee = fee >= 1000 ? fee : 1000,
                onCompletion = V2.Indexer.Model.OnCompletion.Optin,
                applicationId = applicationId
            };
            return txn;
        }
        /// <summary>
        /// call application transaction
        /// </summary>
        /// <param name="sender">sender of the transaction</param>
        /// <param name="applicationId">application id</param>
        /// <param name="trans">suggested transaction params</param>
        /// <param name="args">arguments</param>
        /// <returns>call application transaction</returns>
        public static Transaction GetApplicationCallTransaction(Address sender, ulong? applicationId, TransactionParametersResponse trans, List<byte[]> args = null)
        {
            var fee = (ulong?)trans.Fee;
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong?)trans.LastRound,
                lastValid = (ulong?)trans.LastRound + 1000,
                genesisID = trans.GenesisId,
                genesisHash = new Digest(trans.GenesisHash),
                fee = fee >= 1000 ? fee : 1000,
                applicationId = applicationId,
                applicationArgs = args
            };
            return txn;
        }
        /// <summary>
        /// update application transaction
        /// </summary>
        /// <param name="sender">sender of the transaction</param>
        /// <param name="applicationId">application id</param>
        /// <param name="approvalProgram">approval program</param>
        /// <param name="clearProgram">clear program</param>
        /// <param name="trans">suggested transaction params</param>
        /// <returns>update application transaction</returns>
        public static Transaction GetApplicationUpdateTransaction(Address sender, ulong? applicationId, 
            TEALProgram approvalProgram, TEALProgram clearProgram, TransactionParametersResponse trans)
        {
            var fee = (ulong?)trans.Fee;
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong?)trans.LastRound,
                lastValid = (ulong?)trans.LastRound + 1000,
                genesisID = trans.GenesisId,
                genesisHash = new Digest(trans.GenesisHash),
                fee = fee >= 1000 ? fee : 1000,
                applicationId = applicationId,
                onCompletion = V2.Indexer.Model.OnCompletion.Update,
                approvalProgram = approvalProgram,
                clearStateProgram = clearProgram
            };
            return txn;
        }
        /// <summary>
        /// delete transaction transaction
        /// </summary>
        /// <param name="sender">sender of the transaction</param>
        /// <param name="applicationId">application id</param>
        /// <param name="trans">suggested transaction params</param>
        /// <returns>delete transaction transaction</returns>
        public static Transaction GetApplicationDeleteTransaction(Address sender, ulong? applicationId, TransactionParametersResponse trans)
        {
            var fee = (ulong?)trans.Fee;
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong?)trans.LastRound,
                lastValid = (ulong?)trans.LastRound + 1000,
                genesisID = trans.GenesisId,
                genesisHash = new Digest(trans.GenesisHash),
                fee = fee >= 1000 ? fee : 1000,
                onCompletion = V2.Indexer.Model.OnCompletion.Delete,
                applicationId = applicationId
            };
            return txn;
        }
        /// <summary>
        /// clear transaction transaction
        /// </summary>
        /// <param name="sender">sender of the transaction</param>
        /// <param name="applicationId">application id</param>
        /// <param name="trans">suggested transaction params</param>
        /// <returns>clear transaction transaction</returns>
        public static Transaction GetApplicationClearTransaction(Address sender, ulong? applicationId, TransactionParametersResponse trans)
        {
            var fee = (ulong?)trans.Fee;
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong?)trans.LastRound,
                lastValid = (ulong?)trans.LastRound + 1000,
                genesisID = trans.GenesisId,
                genesisHash = new Digest(trans.GenesisHash),
                fee = fee >= 1000 ? fee : 1000,
                onCompletion = V2.Indexer.Model.OnCompletion.Clear,
                applicationId = applicationId
            };
            return txn;
        }
        /// <summary>
        /// close out transaction transaction
        /// </summary>
        /// <param name="sender">sender of the transaction</param>
        /// <param name="applicationId">application id</param>
        /// <param name="trans">suggested transaction params</param>
        /// <returns>close out transaction transaction</returns>
        public static Transaction GetApplicationCloseTransaction(Address sender, ulong? applicationId, TransactionParametersResponse trans)
        {
            var fee = (ulong?)trans.Fee;
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong?)trans.LastRound,
                lastValid = (ulong?)trans.LastRound + 1000,
                genesisID = trans.GenesisId,
                genesisHash = new Digest(trans.GenesisHash),
                fee = fee >= 1000 ? fee : 1000,
                onCompletion = V2.Indexer.Model.OnCompletion.Closeout,
                applicationId = applicationId
            };
            return txn;
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="lsig"></param>
        ///// <param name="receiver"></param>
        ///// <param name="trans"></param>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public static Transaction GetLogicSignatureTransaction(LogicsigSignature lsig, Address receiver, TransactionParams trans, string message = "")
        //{
        //    var tx = new Transaction(lsig.ToAddress(), receiver, 1, trans.LastRound, trans.LastRound + 1000,
        //        trans.GenesisID, new Digest(trans.Genesishashb64))
        //    {
        //        note = Encoding.UTF8.GetBytes(message)
        //    };
        //    Account.SetFeeByFeePerByte(tx, trans.Fee);
        //    return tx;
        //}
        //public static Transaction GetLogicSignatureTransaction(LogicsigSignature lsig, Address receiver, 
        //    TransactionParametersResponse trans, string message = "")
        //{
        //    var tx = new Transaction(lsig.ToAddress(), receiver, 1, (ulong?)trans.LastRound, 
        //        (ulong?)trans.LastRound + 1000, trans.GenesisId, new Digest(trans.GenesisHash))
        //    {
        //        note = Encoding.UTF8.GetBytes(message)
        //    };
        //    Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
        //    return tx;
        //}

        //public static Transaction GetLogicSignatureTransaction(Address signingAcct, Address receiver, TransactionParams trans, string message = "")
        //{
        //    var tx = new Transaction(signingAcct, receiver, 1, trans.LastRound, trans.LastRound + 1000,
        //        trans.GenesisID, new Digest(trans.Genesishashb64))
        //    {
        //        note = Encoding.UTF8.GetBytes(message)
        //    };
        //    Account.SetFeeByFeePerByte(tx, trans.Fee);
        //    return tx;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="stxn"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async static Task<V2.Algod.Model.DryrunResponse> GetDryrunResponse(V2.Algod.DefaultApi client, SignedTransaction stxn, byte[] source = null)
        {
            List<V2.Algod.Model.DryrunSource> sources = new List<V2.Algod.Model.DryrunSource>();
            List<SignedTransaction> stxns = new List<SignedTransaction>();
            //compiled 
            if (source is null)
            {
                stxns.Add(stxn);
            }
            // source
            else
            {
                sources.Add(new V2.Algod.Model.DryrunSource(){
                    FieldName= "lsig",
                    Source= Encoding.UTF8.GetString(source), TxnIndex= 0 });
                stxns.Add(stxn);
            }
            if (sources.Count < 1) sources = null;
            return await client.DryrunAsync(new V2.Algod.Model.DryrunRequest() { Txns = stxns, Sources = sources });
        }

        internal static byte[] CombineBytes(byte[] b1, byte[] b2)
        {
            byte[] ret = new byte[b1.Length + b2.Length];
            Buffer.BlockCopy(b1, 0, ret, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, ret, b1.Length, b2.Length);
            return ret;
        }
    }
}
