using Algorand.Algod.Api;
using Algorand.Algod.Model;
using Algorand.V2.Model;
using System;
using System.Collections.Generic;
using System.Text;
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
        /// <param name="instance"></param>
        /// <param name="txID"></param>
        /// <returns></returns>
        public static string WaitTransactionToComplete(AlgodApi instance, string txID) //throws Exception
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
        /// wait transaction to complete using algod v2 api
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="txID"></param>
        /// <returns></returns>
        public static string WaitTransactionToComplete(V2.AlgodApi instance, string txID) //throws Exception
        {
            while (true)
            {
                //Check the pending tranactions
                var b3 = instance.PendingTransactionInformation(txID);
                
                if (b3.ConfirmedRound != null && b3.ConfirmedRound > 0)
                {
                    return "Transaction " + txID + " confirmed in round " + b3.ConfirmedRound;
                }
                // loops 4 times per second, > 5 times per second will fail using TestNet Purestake Free verison  
                // also blocks are created in under 5 seconds so no real need to poll constantly - 
                // a few times per second should be fine
                System.Threading.Thread.Sleep(250);
            }
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
        public static PostTransactionsResponse SubmitTransaction(V2.AlgodApi instance, SignedTransaction signedTx) //throws Exception
        {
            byte[] encodedTxBytes = Encoder.EncodeToMsgPack(signedTx);
            return instance.RawTransaction(encodedTxBytes);
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
            var notes = Encoding.UTF8.GetBytes(message);
            var tx = new Transaction(from, fee, lastRound, lastRound + 1000,
                    notes, amount, to, genesisId, new Digest(genesishashb64));
            Account.SetFeeByFeePerByte(tx, fee);
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
        public static Transaction GetCreateAssetTransaction(AssetParams asset, TransactionParams trans, string message = "", int decimals = 0)
        {
            ValidateAsset(asset);
            // assetDecimals is not exist in api, so set as zero in this version
            var tx = Transaction.CreateAssetCreateTransaction(new Address(asset.Creator), trans.Fee, trans.LastRound, trans.LastRound + 1000,
                Encoding.UTF8.GetBytes(message), trans.GenesisID, new Digest(trans.Genesishashb64),
                asset.Total, decimals, (bool)asset.Defaultfrozen, asset.Unitname, asset.Assetname, asset.Url,
                Convert.FromBase64String(asset.Metadatahash), new Address(asset.Managerkey), new Address(asset.Reserveaddr),
                new Address(asset.Freezeaddr), new Address(asset.Clawbackaddr));
            Account.SetFeeByFeePerByte(tx, trans.Fee);
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
        public static Transaction GetCreateAssetTransaction(V2.Model.AssetParams asset, V2.Model.TransactionParametersResponse trans, string message = "")
        {
            ValidateAsset(asset);
            // assetDecimals is not exist in api, so set as zero in this version
            var tx = Transaction.CreateAssetCreateTransaction(new Address(asset.Creator), (ulong?)trans.Fee, (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000,
                Encoding.UTF8.GetBytes(message), trans.GenesisId, new Digest(trans.GenesisHash),
                asset.Total, (int)asset.Decimals, (bool)asset.DefaultFrozen, asset.UnitName, asset.Name, asset.Url,
                asset.MetadataHash, new Address(asset.Manager), new Address(asset.Reserve),
                new Address(asset.Freeze), new Address(asset.Clawback));
            Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
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
        public static Transaction GetConfigAssetTransaction(Address sender, ulong? assetId, AssetParams asset, TransactionParams trans, string message = "")
        {
            ValidateAsset(asset);
            //sender must be manager
            var tx = Transaction.CreateAssetConfigureTransaction(sender, 1,
                trans.LastRound, trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID,
                new Digest(trans.Genesishashb64), assetId, new Address(asset.Managerkey), 
                new Address(asset.Reserveaddr), new Address(asset.Freezeaddr), new Address(asset.Clawbackaddr), false);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }

        public static Transaction GetConfigAssetTransaction(Address sender, V2.Model.Asset asset, TransactionParametersResponse trans, string message = "")
        {
            ValidateAsset(asset.Params);
            //sender must be manager
            var tx = Transaction.CreateAssetConfigureTransaction(sender, 1,
                (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)asset.Index, new Address(asset.Params.Manager),
                new Address(asset.Params.Reserve), new Address(asset.Params.Freeze), new Address(asset.Params.Clawback), false);
            Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
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
        public static Transaction GetActivateAssetTransaction(Address sender, ulong? assetId, TransactionParams trans, string message = "")
        {            
            return GetAssetOptingInTransaction(sender, assetId, trans, message);
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
        public static Transaction GetAssetOptingInTransaction(Address sender, ulong? assetId, TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetAcceptTransaction(sender, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID,
                new Digest(trans.Genesishashb64), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetAssetOptingInTransaction(Address sender, long? assetID, TransactionParametersResponse trans, string message = "")
        {
            var tx = Transaction.CreateAssetAcceptTransaction(sender, 1, (ulong?)trans.LastRound,
                (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)assetID);
            Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
            return tx;
        }

        public static Transaction GetTransferAssetTransaction(Address from, Address to, ulong? assetId, ulong amount, TransactionParams trans, 
            Address closeTo = null, string message = "") {
            var tx = Transaction.CreateAssetTransferTransaction(from, to, closeTo, amount, 1,
                trans.LastRound, trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID, 
                new Digest(trans.Genesishashb64), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetTransferAssetTransaction(Address from, Address to, long? assetId, ulong amount, 
            TransactionParametersResponse trans, Address closeTo = null, string message = "")
        {
            var tx = Transaction.CreateAssetTransferTransaction(from, to, closeTo, amount, 1,
                (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)assetId);
            Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
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
            TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetFreezeTransaction(sender, toFreeze, freezeState, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), new Digest(trans.Genesishashb64), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }

        public static Transaction GetFreezeAssetTransaction(Address sender, Address toFreeze, long? assetId, bool freezeState, 
            TransactionParametersResponse trans, string message = "")
        {
            var tx = Transaction.CreateAssetFreezeTransaction(sender, toFreeze, freezeState, 1, (ulong?)trans.LastRound,
                (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), new Digest(trans.GenesisHash), (ulong?)assetId);
            Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
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
            ulong amount, TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetRevokeTransaction(reserve, revokedFrom, receiver, amount, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID, 
                new Digest(trans.Genesishashb64), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }

        public static Transaction GetRevokeAssetTransaction(Address reserve, Address revokedFrom, Address receiver, long? assetId, 
            ulong amount, TransactionParametersResponse trans, string message = "")
        {
            var tx = Transaction.CreateAssetRevokeTransaction(reserve, revokedFrom, receiver, amount, 1, (ulong?)trans.LastRound,
                (ulong?)trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisId,
                new Digest(trans.GenesisHash), (ulong?)assetId);
            Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
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
        public static Transaction GetDestroyAssetTransaction(Address manager, ulong? assetId, TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetDestroyTransaction(manager, 1, trans.LastRound, trans.LastRound + 1000, 
                Encoding.UTF8.GetBytes(message), new Digest(trans.Genesishashb64), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetDestroyAssetTransaction(Address manager, long? assetId, TransactionParametersResponse trans, string message = "")
        {
            var tx = Transaction.CreateAssetDestroyTransaction(manager, 1, (ulong?)trans.LastRound, (ulong?)trans.LastRound + 1000,
                Encoding.UTF8.GetBytes(message), new Digest(trans.GenesisHash), (ulong?)assetId);
            Account.SetFeeByFeePerByte(tx, (ulong?)trans.Fee);
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

        private static void ValidateAsset(V2.Model.AssetParams asset)
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
        public static Transaction GetBidTransaction(Address bidder, Address auction, SignedBid signedBid, TransactionParams trans)
        {
            var tx = new Transaction(bidder, auction, 0, trans.LastRound, trans.LastRound + 1000,
                trans.GenesisID, new Digest(trans.Genesishashb64))
            {
                note = Encoder.EncodeToMsgPack(signedBid)
            };
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetApplicationTransaction(Address sender, long firstValid, long lastValid, string genesishashb64, 
            OnCompletion onCompletion, long applicationId, List<byte[]> applicationArgs, List<Address> accounts, List<long> foreignApps, List<long> foreignAssets)
        {
            var txn = new Transaction
            {
                type = Transaction.Type.ApplicationCall,
                sender = sender,
                firstValid = (ulong)firstValid,
                lastValid = (ulong)lastValid,
                genesisHash = new Digest(genesishashb64)
            };

            // Global requirements
            //Objects.requireNonNull(onCompletion, "OnCompletion is required, please file a bug report.");
            //Objects.requireNonNull(applicationId);
            //JavaHelper<long>.RequireNotNull()

            if (applicationId >= 0) txn.applicationId = applicationId;
            else throw new ArgumentException("Please set right application Id.");
            //if (onCompletion != new OnCompletion()) 
                txn.onCompletion = onCompletion;
            //else throw new ArgumentException("OnCompletion is required, please file a bug report.");
            if (applicationArgs != null) txn.applicationArgs = applicationArgs;
            if (accounts != null) txn.accounts = accounts;
            if (foreignApps != null) txn.foreignApps = foreignApps;
            if (foreignAssets != null) txn.foreignAssets = foreignAssets;
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

        public static DryrunResponse GetDryrunResponse(V2.AlgodApi client, SignedTransaction stxn, byte[] source = null)
        {
            List<DryrunSource> sources = new List<DryrunSource>();
            List<SignedTransaction> stxns = new List<SignedTransaction>();
            //compiled 
            if (source is null)
            {
                stxns.Add(stxn);
            }
            // source
            else
            {
                sources.Add(new DryrunSource(fieldName: "lsig",
                    source: Encoding.UTF8.GetString(source), txnIndex: 0));
                stxns.Add(stxn);
            }
            if (sources.Count < 1) sources = null;
            return client.TealDryrun(new DryrunRequest(txns: stxns, sources: sources));
        }
    }
}
