using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Algorand
{
    /// <summary>
    /// Create and manage secrets, and perform account-based work such as signing transactions.
    /// </summary>
    public class Account
    {
        private AsymmetricCipherKeyPair privateKeyPair;
        public Address Address { get; private set; }
        private const int PK_SIZE = 32;
        private const int SK_SIZE = 32;
        private const int SK_SIZE_BITS = SK_SIZE * 8;
        private static readonly byte[] BID_SIGN_PREFIX = Encoding.UTF8.GetBytes("aB");
        private static readonly byte[] BYTES_SIGN_PREFIX = Encoding.UTF8.GetBytes("MX");
        private const ulong MIN_TX_FEE_UALGOS = 1000;
        private static readonly byte[] PROGDATA_SIGN_PREFIX = Encoding.UTF8.GetBytes("ProgData");
        /// <summary>
        /// Rebuild the account from private key
        /// </summary>
        /// <param name="privateKey">Private Key</param>
        /// <returns>the rebuilded account</returns>
        public static Account AccountFromPrivateKey(byte[] privateKey)
        {
            if(privateKey.Length != SK_SIZE)            
                throw new ArgumentException("Incorrect private key length");
            
            var privateKeyRebuild = new Ed25519PrivateKeyParameters(privateKey, 0);
            var publicKeyRebuild = privateKeyRebuild.GeneratePublicKey();
            var act = new Account
            {
                privateKeyPair = new AsymmetricCipherKeyPair(publicKeyRebuild, privateKeyRebuild),
            };
            act.Address = new Address(act.GetClearTextPublicKey());
            return act;
        }
        /// <summary>
        /// get clear text private key
        /// </summary>
        /// <returns>the private key as length 32 byte array.</returns>
        public byte[] GetClearTextPrivateKey()
        {
            var privateKey = privateKeyPair.Private as Ed25519PrivateKeyParameters;

            byte[] b = privateKey.GetEncoded(); // X.509 prepended with ASN.1 prefix

            if (b.Length != SK_SIZE)
            {
                throw new Exception("Generated private key is the wrong size");
            }
            return b;
        }
        /// <summary>
        /// Generate a newc account, random account.
        /// </summary>
        public Account()
        {
            CreateAccountFromRandom(new SecureRandom());
        }
        /// <summary>
        /// Generate a newc account with seed(master derivation key)
        /// </summary>
        /// <param name="seed">seed(master derivation key)</param>
        public Account(byte[] seed)
        {
            // seed here corresponds to rfc8037 private key, corresponds to seed in go impl
            // BC for instance takes the seed as private key straight up
            CreateAccountFromRandom(new FixedSecureRandom(seed));
        }
        /// <summary>
        /// Create a new account with mnemonic
        /// </summary>
        /// <param name="mnemonic">the mnemonic</param>
        public Account(string mnemonic) : this(Mnemonic.ToKey(mnemonic)) { }

        private void CreateAccountFromRandom(SecureRandom srandom)
        {
            Ed25519KeyPairGenerator keyPairGenerator = new Ed25519KeyPairGenerator();
            keyPairGenerator.Init(new Ed25519KeyGenerationParameters(srandom));
            this.privateKeyPair = keyPairGenerator.GenerateKeyPair();
            byte[] raw = this.GetClearTextPublicKey();
            this.Address = new Address(raw);
        }

        /// <summary>
        /// Convenience method for getting the underlying public key for raw operations.
        /// </summary>
        /// <returns>the public key as length 32 byte array.</returns>
        public byte[] GetClearTextPublicKey()
        {
            var publicKey = privateKeyPair.Public as Ed25519PublicKeyParameters;

            byte[] b = publicKey.GetEncoded(); // X.509 prepended with ASN.1 prefix

            if (b.Length != PK_SIZE)
            {
                throw new Exception("Generated public key is the wrong size");
            }
            return b;
        }

        /// <summary>
        /// Get the public key
        /// </summary>
        /// <returns>public key</returns>
        public Ed25519PublicKeyParameters GetEd25519PublicKey()
        {
            return new Ed25519PublicKeyParameters(this.GetClearTextPublicKey(), 0);
        }

        /// <summary>
        /// Converts the 32 byte private key to a 25 word mnemonic, including a checksum.
        /// Refer to the mnemonic package for additional documentation.
        /// </summary>
        /// <returns>return string a 25 word mnemonic</returns>
        public string ToMnemonic()
        {
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(this.privateKeyPair.Private);
            byte[] X509enc = privateKeyInfo.ToAsn1Object().GetEncoded();
            PrivateKeyInfo pkinfo = PrivateKeyInfo.GetInstance(X509enc);
            var keyOcts = pkinfo.ParsePrivateKey();
            byte[] res = Asn1OctetString.GetInstance(keyOcts).GetOctets();
            return Mnemonic.FromKey(res);
        }

        /// <summary>
        /// Sign a transaction with this account
        /// </summary>
        /// <param name="tx">the transaction to sign</param>
        /// <returns>a signed transaction</returns>
        public SignedTransaction SignTransaction(Transaction tx)
        {
            byte[] prefixEncodedTx = tx.BytesToSign();
            Signature txSig = RawSignBytes(prefixEncodedTx);
            var stx = new SignedTransaction(tx, txSig, tx.TxID());
            if (!tx.sender.Equals(this.Address))
                stx.authAddr = this.Address;
            return stx;
        }
        /// <summary>
        /// Sign a transaction with this account, replacing the fee with the given feePerByte.
        /// </summary>
        /// <param name="tx">transaction to sign</param>
        /// <param name="feePerByte">feePerByte fee per byte, often returned as a suggested fee</param>
        /// <returns>a signed transaction</returns>
        public SignedTransaction SignTransactionWithFeePerByte(Transaction tx, ulong feePerByte)
        {
            SetFeeByFeePerByte(tx, feePerByte);
            return this.SignTransaction(tx);
        }
        /// <summary>
        /// Sign a bid with this account
        /// </summary>
        /// <param name="bid">the bid to sign</param>
        /// <returns>signed bid</returns>
        public SignedBid SignBid(Bid bid)
        {
            byte[] encodedBid = Encoder.EncodeToMsgPack(bid);
            List<byte> prefixEncodedBid = new List<byte>(BID_SIGN_PREFIX);
            prefixEncodedBid.AddRange(encodedBid);
            Signature bidSig = RawSignBytes(prefixEncodedBid.ToArray());
            return new SignedBid(bid, bidSig);
        }
        /// <summary>
        /// Sets the transaction fee according to suggestedFeePerByte * estimateTxSize.
        /// </summary>
        /// <param name="tx">transaction to populate fee field</param>
        /// <param name="suggestedFeePerByte">suggestedFee given by network</param>
        static public void SetFeeByFeePerByte(Transaction tx, ulong? suggestedFeePerByte)
        {
            ulong? newFee = suggestedFeePerByte * (ulong)EstimatedEncodedSize(tx);
            if (newFee < MIN_TX_FEE_UALGOS)
            {
                newFee = MIN_TX_FEE_UALGOS;
            }
            tx.fee = newFee;
        }
        /// <summary>
        /// EstimateEncodedSize returns the estimated encoded size of the transaction including the signature.
        /// This function is useful for calculating the fee from suggested fee per byte.
        /// </summary>
        /// <param name="tx">the transaction</param>
        /// <returns>an estimated byte size for the transaction.</returns>
        public static int EstimatedEncodedSize(Transaction tx)
        {
            Account acc = new Account();
            return Encoder.EncodeToMsgPack(
                new SignedTransaction(tx, acc.RawSignBytes(tx.BytesToSign()), tx.TxID())).Length;
        }
        /// <summary>
        /// Sign the given bytes, and wrap in Signature.
        /// </summary>
        /// <param name="bytes">bytes the data to sign</param>
        /// <returns>a signature</returns>
        private Signature RawSignBytes(byte[] bytes)
        {
            var signer = new Ed25519Signer();
            signer.Init(true, privateKeyPair.Private);
            signer.BlockUpdate(bytes, 0, bytes.Length);
            byte[] signature = signer.GenerateSignature();
            return new Signature(signature);
        }
        /// <summary>
        /// Sign the given bytes, and wrap in signature. The message is prepended with "MX" for domain separation.
        /// </summary>
        /// <param name="bytes">bytes the data to sign</param>
        /// <returns>signature</returns>
        public Signature SignBytes(byte[] bytes) //throws NoSuchAlgorithmException
        {
            List<byte> retByte = new List<byte>();
            retByte.AddRange(BYTES_SIGN_PREFIX);
            retByte.AddRange(bytes);
            return RawSignBytes(retByte.ToArray());
        }
        #region Multisignature support
        /// <summary>
        /// SignMultisigTransaction creates a multisig transaction from the input and the multisig account.
        /// </summary>
        /// <param name="from">sign as this multisignature account</param>
        /// <param name="tx">the transaction to sign</param>
        /// <returns>SignedTransaction a partially signed multisig transaction</returns>
        public SignedTransaction SignMultisigTransaction(MultisigAddress from, Transaction tx) //throws NoSuchAlgorithmException
        {
            // check that from addr of tx matches multisig preimage
            if (!tx.sender.ToString().Equals(from.ToString()))
            {
                throw new ArgumentException("Transaction sender does not match multisig account");
            }
            // check that account secret key is in multisig pk list
            var myPK = this.GetEd25519PublicKey();
            byte[] myEncoded = myPK.GetEncoded();
            int myI = -1;
            for (int i = 0; i < from.publicKeys.Count; i++)
                if (Enumerable.SequenceEqual(myEncoded, from.publicKeys[i].GetEncoded()))
                {
                    myI = i;
                    break;
                }

            if (myI == -1)
            {
                throw new ArgumentException("Multisig account does not contain this secret key");
            }
            // now, create the multisignature
            SignedTransaction txSig = this.SignTransaction(tx);
            MultisigSignature mSig = new MultisigSignature(from.version, from.threshold);
            for (int i = 0; i < from.publicKeys.Count; i++)
            {
                if (i == myI)
                {
                    mSig.subsigs.Add(new MultisigSubsig(myPK, txSig.sig));
                }
                else
                {
                    mSig.subsigs.Add(new MultisigSubsig(from.publicKeys[i]));
                }
            }
            return new SignedTransaction(tx, mSig, txSig.transactionID);
        }
        /// <summary>
        /// MergeMultisigTransactions merges the given (partially) signed multisig transactions.
        /// </summary>
        /// <param name="txs">partially signed multisig transactions to merge. Underlying transactions may be mutated.</param>
        /// <returns>merged multisig transaction</returns>
        public static SignedTransaction MergeMultisigTransactions(params SignedTransaction[] txs)
        {
            if (txs.Length < 2)
            {
                throw new ArgumentException("cannot merge a single transaction");
            }
            SignedTransaction merged = txs[0];
            for (int i = 0; i < txs.Length; i++)
            {
                // check that multisig parameters match
                SignedTransaction tx = txs[i];
                if (tx.mSig.version != merged.mSig.version ||
                        tx.mSig.threshold != merged.mSig.threshold)
                {
                    throw new ArgumentException("transaction msig parameters do not match");
                }
                for (int j = 0; j < tx.mSig.subsigs.Count; j++)
                {
                    MultisigSubsig myMsig = merged.mSig.subsigs[j];
                    MultisigSubsig theirMsig = tx.mSig.subsigs[j];
                    if (!theirMsig.key.Equals(myMsig.key))
                    {
                        throw new ArgumentException("transaction msig public keys do not match");
                    }
                    if (myMsig.sig.Equals(new Signature()))
                    {
                        myMsig.sig = theirMsig.sig;
                    }
                    else if (!myMsig.sig.Equals(theirMsig.sig) &&
                          !theirMsig.sig.Equals(new Signature()))
                    {
                        throw new ArgumentException("transaction msig has mismatched signatures");
                    }
                    merged.mSig.subsigs[j] = myMsig;
                }
            }
            return merged;
        }
        /// <summary>
        /// AppendMultisigTransaction appends our signature to the given multisig transaction.
        /// </summary>
        /// <param name="from">the multisig public identity we are signing for</param>
        /// <param name="signedTx">the partially signed msig tx to which to append signature</param>
        /// <returns>merged multisig transaction</returns>
        public SignedTransaction AppendMultisigTransaction(MultisigAddress from, SignedTransaction signedTx)
        {
            SignedTransaction sTx = this.SignMultisigTransaction(from, signedTx.tx);
            return MergeMultisigTransactions(sTx, signedTx);
        }

        /// <summary>
        /// a convenience method for working directly with raw transaction files.
        /// </summary>
        /// <param name="txsBytes">list of multisig transactions to merge</param>
        /// <returns>an encoded, merged multisignature transaction</returns>
        public static byte[] MergeMultisigTransactionBytes(params byte[][] txsBytes)
        {

            SignedTransaction[] sTxs = new SignedTransaction[txsBytes.Length];
            for (int i = 0; i < txsBytes.Length; i++) {
                sTxs[i] = Encoder.DecodeFromMsgPack<SignedTransaction>(txsBytes[i]);
            }
            SignedTransaction merged = Account.MergeMultisigTransactions(sTxs);
            return Encoder.EncodeToMsgPack(merged);
        }
        /// <summary>
        /// a convenience method for directly appending our signature to a raw tx file
        /// </summary>
        /// <param name="from">the public identity we are signing as</param>
        /// <param name="txBytes">the multisig transaction to append signature to</param>
        /// <returns>merged multisignature transaction inclukding our signature</returns>
        public byte[] AppendMultisigTransactionBytes(MultisigAddress from, byte[] txBytes)
        {
            SignedTransaction inTx = Encoder.DecodeFromMsgPack<SignedTransaction>(txBytes);
            SignedTransaction appended = this.AppendMultisigTransaction(from, inTx);
            return Encoder.EncodeToMsgPack(appended);
        }
        #endregion

        #region LogicSig
        /// <summary>
        /// Sign LogicSig with account's secret key
        /// </summary>
        /// <param name="lsig">LogicsigSignature to sign</param>
        /// <returns>LogicsigSignature with updated signature</returns>
        public LogicsigSignature SignLogicsig(LogicsigSignature lsig)
        {
            byte[] bytesToSign = lsig.BytesToSign();
            Signature sig = this.RawSignBytes(bytesToSign);
            lsig.sig = sig;
            return lsig;
        }

        /// <summary>
        /// Sign LogicSig as multisig
        /// </summary>
        /// <param name="lsig">LogicsigSignature to sign</param>
        /// <param name="ma">MultisigAddress to format multi signature from</param>
        /// <returns>LogicsigSignature</returns>
        public LogicsigSignature SignLogicsig(LogicsigSignature lsig, MultisigAddress ma)
        {
            var pk = this.GetEd25519PublicKey();
            int pkIndex = -1;
            for (int i = 0; i < ma.publicKeys.Count; i++)
            {
                if (Enumerable.SequenceEqual(pk.GetEncoded(), ma.publicKeys[i].GetEncoded())){
                    pkIndex = i;
                    break;
                }
            }

            if (pkIndex == -1)
            {
                throw new ArgumentException("Multisig account does not contain this secret key");
            }
            // now, create the multisignature
            byte[] bytesToSign = lsig.BytesToSign();
            Signature sig = this.RawSignBytes(bytesToSign);
            MultisigSignature mSig = new MultisigSignature(ma.version, ma.threshold);
            for (int i = 0; i < ma.publicKeys.Count; i++)
            {
                if (i == pkIndex)
                {
                    mSig.subsigs.Add(new MultisigSubsig(pk, sig));
                }
                else
                {
                    mSig.subsigs.Add(new MultisigSubsig(ma.publicKeys[i]));
                }
            }
            lsig.msig = mSig;
            return lsig;
        }

        /// <summary>
        /// Appends a signature to multisig logic signed transaction
        /// </summary>
        /// <param name="lsig">LogicsigSignature append to</param>
        /// <returns>LogicsigSignature</returns>
        public LogicsigSignature AppendToLogicsig(LogicsigSignature lsig)
        {
            var pk = this.GetEd25519PublicKey();
            int pkIndex = -1;
            for (int i = 0; i < lsig.msig.subsigs.Count; i++)
            {
                MultisigSubsig subsig = lsig.msig.subsigs[i];
                if (Enumerable.SequenceEqual(subsig.key.GetEncoded(), pk.GetEncoded()))
                {
                    pkIndex = i;
                }
            }
            if (pkIndex == -1)
            {
                throw new ArgumentException("Multisig account does not contain this secret key");
            }
            // now, create the multisignature
            byte[] bytesToSign = lsig.BytesToSign();
            Signature sig = this.RawSignBytes(bytesToSign);
            lsig.msig.subsigs[pkIndex] = new MultisigSubsig(pk, sig);
            return lsig;
        }
        /// <summary>
        /// Creates SignedTransaction from LogicsigSignature and Transaction.
        /// LogicsigSignature must be valid and verifiable against transaction sender field.
        /// </summary>
        /// <param name="lsig">LogicsigSignature</param>
        /// <param name="tx">Transaction</param>
        /// <returns>SignedTransaction</returns>
        public static SignedTransaction SignLogicsigTransaction(LogicsigSignature lsig, Transaction tx)
        {
            if (!lsig.Verify(tx.sender))
            {
                throw new ArgumentException("verification failed");
            }
            return new SignedTransaction(tx, lsig, tx.TxID());
        }

        //public static SignedTransaction SignLogicsigDelegatedTransaction(LogicsigSignature lsig, Transaction tx)
        //{

        //    return new SignedTransaction(tx, lsig, tx.TxID());
        //}
        #endregion

        /// <summary>
        /// Creates Signature compatible with ed25519verify TEAL opcode from data and contract address(program hash).
        /// </summary>
        /// <param name="data">data byte[]</param>
        /// <param name="contractAddress">contractAddress Address</param>
        /// <returns>Signature</returns>
        public Signature TealSign(byte[] data, Address contractAddress)
        {
            byte[] rawAddress = contractAddress.Bytes;
            List<byte> baos = new List<byte>();
            baos.AddRange(PROGDATA_SIGN_PREFIX);
            baos.AddRange(rawAddress);
            baos.AddRange(data);
            return this.RawSignBytes(baos.ToArray());
        }

        /// <summary>
        /// Creates Signature compatible with ed25519verify TEAL opcode from data and program bytes
        /// </summary>
        /// <param name="data">data byte[]</param>
        /// <param name="program">program byte[]</param>
        /// <returns>Signature</returns>
        public Signature TealSignFromProgram(byte[] data, byte[] program)
        {
            LogicsigSignature lsig = new LogicsigSignature(program);
            return this.TealSign(data, lsig.Address);
        }
}

    // Return a pre-set seed in response to nextBytes or generateSeed
    class FixedSecureRandom : SecureRandom
    {
        private byte[] fixedValue;
        private int index = 0;

        public FixedSecureRandom(byte[] fixedValue)
        {
            this.fixedValue = fixedValue;
        }
        public override void NextBytes(byte[] bytes)
        {
            if (this.index >= this.fixedValue.Length)
            {
                // no more data to copy
                return;
            }
            int len = bytes.Length;
            if (len > this.fixedValue.Length - this.index)
            {
                len = this.fixedValue.Length - this.index;
            }
            JavaHelper<byte>.SystemArrayCopy(this.fixedValue, this.index, bytes, 0, len);
            this.index += bytes.Length;
        }

        public override byte[] GenerateSeed(int numBytes)
        {
            byte[] bytes = new byte[numBytes];
            this.NextBytes(bytes);
            return bytes;
        }
    }

}
