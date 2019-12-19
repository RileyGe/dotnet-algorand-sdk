using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using System;
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
        /// Generate a 32 bytes string for asset metadata hash
        /// </summary>
        /// <returns>a 32 bytes string</returns>
        public static string GetRandomAssetMetaHash()
        {
            Random rd = new Random();
            byte[] bts = new byte[24];
            rd.NextBytes(bts);
            var base64 = Convert.ToBase64String(bts);
            return base64.Substring(0, 32);
        }
        /// <summary>
        /// Generate a create asset transaction
        /// </summary>
        /// <param name="asset">The asset infomation</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <returns>transaction</returns>
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
                new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId, new Address(asset.Managerkey), 
                new Address(asset.Reserveaddr), new Address(asset.Freezeaddr), new Address(asset.Clawbackaddr), false);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        /// <summary>
        /// Generate an asset activation transaction
        /// All accounts that want recieve the new asset have to activate the asset
        /// </summary>
        /// <param name="sender">The account want to activate the asset</param>
        /// <param name="assetId">Asset ID</param>
        /// <param name="trans">The blockchain infomation</param>
        /// <param name="message">The message for the transaction(have no affect to the assect)</param>
        /// <returns>transaction</returns>
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
                trans.LastRound + 1000, Encoding.UTF8.GetBytes(message), new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
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
                new Digest(Convert.FromBase64String(trans.Genesishashb64)), assetId);
            Account.SetFeeByFeePerByte(tx, trans.Fee);
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
            if (asset.Metadatahash is null || asset.Metadatahash == "")
                asset.Metadatahash = GetRandomAssetMetaHash();//auto generate metahash by sdk
            else if (Encoding.UTF8.GetByteCount(asset.Metadatahash) != 32)
                throw new ArgumentException("The metadata hash should be 32 bytes.");
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
                trans.GenesisID, new Digest(Convert.FromBase64String(trans.Genesishashb64)))
            {
                note = Encoder.EncodeToMsgPack(signedBid)
            };
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsig"></param>
        /// <param name="receiver"></param>
        /// <param name="trans"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Transaction GetLogicSignatureTransaction(LogicsigSignature lsig, Address receiver, TransactionParams trans, string message = "")
        {
            var tx = new Transaction(lsig.ToAddress(), receiver, 1, trans.LastRound, trans.LastRound + 1000,
                trans.GenesisID, new Digest(Convert.FromBase64String(trans.Genesishashb64)))
            {
                note = Encoding.UTF8.GetBytes(message)
            };
            Account.SetFeeByFeePerByte(tx, trans.Fee);
            return tx;
        }
    }
}
