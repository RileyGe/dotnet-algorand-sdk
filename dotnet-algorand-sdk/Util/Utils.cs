using Algorand.Algod.Client;
using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand
{
    public class Utils
    {
        public static string WaitTransactionToComplete(AlgodApi instance, string txID) //throws Exception
        {
            while (true)
            {
                //try {
                //Check the pending tranactions
                Algod.Client.Model.Transaction b3 = instance.PendingTransactionInformation(txID);
                //algodApiInstance.p
                if (b3.Round != null && b3.Round > 0)
                {
                    //Got the completed Transaction
                    return "Transaction " + b3.Tx + " confirmed in round " + b3.Round;
                }
                //} catch (Exception e) {
                //    throw (e);
                //}
            }
        }
        public static TransactionID SubmitTransaction(AlgodApi instance, SignedTransaction signedTx) //throws Exception
        {
            //try {
            // Msgpack encode the signed transaction
            byte[] encodedTxBytes = Encoder.EncodeToMsgPack(signedTx);             
            return instance.RawTransaction(encodedTxBytes);
            //} catch (ApiException e) {
            //    throw (e);
            //}
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
            var notes = Encoding.UTF8.GetBytes(message);
            var tx = new Transaction(from, trans.Fee, trans.LastRound, trans.LastRound + 1000,
                    notes, amount, to, trans.GenesisID, new Digest(Convert.FromBase64String(trans.Genesishashb64)));
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="name"></param>
        /// <param name="unitName"></param>
        /// <param name="total"></param>
        /// <param name="trans"></param>
        /// <param name="decimals"></param>
        /// <param name="frozen"></param>
        /// <param name="url"></param>
        /// <param name="metadataHash"></param>
        /// <param name="manager"></param>
        /// <param name="reserve"></param>
        /// <param name="freeze"></param>
        /// <param name="clawback"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Transaction GetCreateAssetTransaction(AssetParams asset, TransactionParams trans, string message = "")
        {
            ValidateAsset(asset);

            //asset.
            //return Transaction.CreateAssetCreateTransaction(new Address(asset.Creator), trans.Fee, trans.LastRound, trans.LastRound + 1000,
            //    Encoding.UTF8.GetBytes(message), trans.GenesisID, new Digest(Convert.FromBase64String(trans.Genesishashb64)),
            //    asset.Total, asset.assetDecimals, asset.Defaultfrozen, asset.Unitname, asset.Assetname, asset.Url,
            //    Encoding.UTF8.GetBytes(asset.Metadatahash), new Address(asset.Managerkey), new Address(asset.Reserveaddr),
            //    new Address(asset.Freezeaddr), new Address(asset.Clawbackaddr));

            // assetDecimals is not exist in api, so set as zero in this version
            var tx = Transaction.CreateAssetCreateTransaction(new Address(asset.Creator), trans.Fee, trans.LastRound, trans.LastRound + 1000,
                Encoding.UTF8.GetBytes(message), trans.GenesisID, new Digest(Convert.FromBase64String(trans.Genesishashb64)),
                asset.Total, 0, (bool)asset.Defaultfrozen, asset.Unitname, asset.Assetname, asset.Url,
                Encoding.UTF8.GetBytes(asset.Metadatahash), new Address(asset.Managerkey), new Address(asset.Reserveaddr),
                new Address(asset.Freezeaddr), new Address(asset.Clawbackaddr));
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="assetId"></param>
        /// <param name="asset"></param>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Transaction GetConfigAssetTransaction(Address sender, ulong? assetId, AssetParams asset, TransactionParams trans, string message = "")
        {
            ValidateAsset(asset);
            //sender must be manager
            var tx = Transaction.CreateAssetConfigureTransaction(sender, 1,
                trans.LastRound, trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID,
                new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId, new Address(asset.Managerkey), 
                new Address(asset.Reserveaddr), new Address(asset.Freezeaddr), new Address(asset.Clawbackaddr), false);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="assetId"></param>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Transaction GetActivateAssetTransaction(Address sender, ulong? assetId, TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetAcceptTransaction(sender, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID, 
                new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetTransferAssetTransaction(Address from, Address to, ulong? assetId, ulong amount, TransactionParams trans, 
            Address closeTo = null, string message = "") {
            var tx = Transaction.CreateAssetTransferTransaction(from, to, closeTo, amount, 1,
                trans.LastRound, trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID, 
                new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetFreezeAssetTransaction(Address sender, Address toFreeze, ulong? assetId, bool freezeState, 
            TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetFreezeTransaction(sender, toFreeze, freezeState, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetRevokeAssetTransaction(Address reserve, Address revokedFrom, Address receiver, ulong? assetId, 
            ulong amount, TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetRevokeTransaction(reserve, revokedFrom, receiver, amount, 1, trans.LastRound,
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), trans.GenesisID, 
                new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        public static Transaction GetDestroyAssetTransaction(Address manager, ulong? assetId, TransactionParams trans, string message = "")
        {
            var tx = Transaction.CreateAssetDestroyTransaction(manager, 1, trans.LastRound, trans.LastRound + 1000, 
                Encoding.UTF8.GetBytes(message), new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
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
            if (asset.Managerkey is null || asset.Managerkey == "") asset.Managerkey = asset.Creator;
            else if (!Address.IsValid(asset.Managerkey)) throw new ArgumentException("The manager address is not valid.");
            if (asset.Reserveaddr is null || asset.Reserveaddr == "") asset.Reserveaddr = asset.Managerkey;
            else if (!Address.IsValid(asset.Reserveaddr)) throw new ArgumentException("The reserve address is not valid.");
            if (asset.Freezeaddr is null || asset.Freezeaddr == "") asset.Freezeaddr = asset.Managerkey;
            else if (!Address.IsValid(asset.Freezeaddr)) throw new ArgumentException("The freeze address is not valid.");
            if (asset.Clawbackaddr is null || asset.Clawbackaddr == "") asset.Clawbackaddr = asset.Managerkey;
            else if (!Address.IsValid(asset.Clawbackaddr)) throw new ArgumentException("The clawback address is not valid.");
        }  
    }
}
