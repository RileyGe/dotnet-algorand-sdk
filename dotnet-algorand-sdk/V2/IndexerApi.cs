using Algorand.Client;
using Algorand.V2.Indexer;
using Algorand.V2.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand.V2
{
    /// <summary>
    /// an IndexerClient for communicating with the REST API.
    /// </summary>
    public class IndexerApi
    {
        /// <summary>
        /// Construct an AlgodClient for communicating with the REST API.
        /// </summary>
        /// <param name="host">using a URI format.If the scheme is not supplied the client will use HTTP.</param>
        /// <param name="token">authentication token.</param>
        /// <param name="timeout">time out.</param>
        public IndexerApi(string host, string token, int timeout = -1)
        {
            Configuration config = new Configuration
            {
                BasePath = host
            };
            if (host.Contains("algorand.api.purestake.io"))
                //config.ApiKey.Add(new KeyValuePair<string, string>("X-API-Key", apiToken)); //purestake
                config.AddDefaultHeader("X-API-Key", token);

            config.ApiKey.Add(new KeyValuePair<string, string>("X-Algo-API-Token", token));

            if (timeout > 0)
            {
                config.Timeout = timeout;
            }
            this._config = config;
            //super(host, port, token, "X-Algo-API-Token");
        }
        private Configuration _config;
        private CommonApi _commonapi = null;
        private LookupApi _lookupapi = null;
        private SearchApi _searchapi = null;

        /// <summary>
        /// Returns 200 if healthy. 
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>HealthCheck</returns>
        public HealthCheck MakeHealthCheck()
        {
            if (_commonapi is null) _commonapi = new CommonApi(_config);
            return _commonapi.MakeHealthCheck();
        }

        /// <summary>
        ///  Lookup account information.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountId">account string</param>
        /// <param name="round">Include results for the specified round. (optional)</param>
        /// <returns>AccountResponse</returns>
        public AccountResponse LookupAccountByID(string accountId, long? round = null)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupAccountByID(accountId, round);
        }

        /// <summary>
        ///  Lookup account transactions.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accountId">account string</param>
        /// <param name="limit">Maximum number of results to return. (optional)</param>
        /// <param name="next">The next page of results. Use the next token provided by the previous results. (optional)</param>
        /// <param name="notePrefix">Specifies a prefix which must be contained in the note field. (optional)</param>
        /// <param name="txType"> (optional)</param>
        /// <param name="sigType">SigType filters just results using the specified type of signature: * sig - Standard * msig - MultiSig * lsig - LogicSig (optional)</param>
        /// <param name="txid">Lookup the specific transaction by ID. (optional)</param>
        /// <param name="round">Include results for the specified round. (optional)</param>
        /// <param name="minRound">Include results at or after the specified min-round. (optional)</param>
        /// <param name="maxRound">Include results at or before the specified max-round. (optional)</param>
        /// <param name="assetId">Asset ID (optional)</param>
        /// <param name="beforeTime">Include results before the given time. Must be an RFC 3339 formatted string. (optional)</param>
        /// <param name="afterTime">Include results after the given time. Must be an RFC 3339 formatted string. (optional)</param>
        /// <param name="currencyGreaterThan">Results should have an amount greater than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="currencyLessThan">Results should have an amount less than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="rekeyTo">Include results which include the rekey-to field. (optional)</param>
        /// <returns>TransactionsResponse</returns>
        public TransactionsResponse LookupAccountTransactions(string accountId, long? limit = null, 
            string next = null, string notePrefix = null, string txType = null, string sigType = null, 
            string txid = null, long? round = null, long? minRound = null, long? maxRound = null, 
            long? assetId = null, DateTime? beforeTime = null, DateTime? afterTime = null, 
            long? currencyGreaterThan = null, long? currencyLessThan = null, bool? rekeyTo = null)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupAccountTransactions(accountId, limit, next, notePrefix, txType, sigType,
                txid, round, minRound, maxRound, assetId, beforeTime,
                afterTime, currencyGreaterThan, currencyLessThan, rekeyTo);
        }

        /// <summary>
        ///  Lookup application.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <returns>ApplicationResponse</returns>
        public ApplicationResponse LookupApplicationByID(long? applicationId)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupApplicationByID(applicationId);
        }
        /// <summary>
        ///  Lookup asset information.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assetId"></param>
        /// <returns>AssetResponse</returns>
        public AssetResponse LookupAssetByID(long? assetId)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupAssetByID(assetId);
        }
        /// <summary>
        ///  Lookup the list of accounts who hold this asset 
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assetId"></param>
        /// <param name="limit">Maximum number of results to return. (optional)</param>
        /// <param name="next">The next page of results. Use the next token provided by the previous results. (optional)</param>
        /// <param name="round">Include results for the specified round. (optional)</param>
        /// <param name="currencyGreaterThan">Results should have an amount greater than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="currencyLessThan">Results should have an amount less than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <returns>AssetBalancesResponse</returns>
        public AssetBalancesResponse LookupAssetBalances(long? assetId, long? limit = null, string next = null, long? round = null, long? currencyGreaterThan = null, long? currencyLessThan = null)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupAssetBalances(assetId, limit, next, round, currencyGreaterThan, currencyLessThan);
        }

        /// <summary>
        ///  Lookup transactions for an asset.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assetId"></param>
        /// <param name="limit">Maximum number of results to return. (optional)</param>
        /// <param name="next">The next page of results. Use the next token provided by the previous results. (optional)</param>
        /// <param name="notePrefix">Specifies a prefix which must be contained in the note field. (optional)</param>
        /// <param name="txType"> (optional)</param>
        /// <param name="sigType">SigType filters just results using the specified type of signature: * sig - Standard * msig - MultiSig * lsig - LogicSig (optional)</param>
        /// <param name="txid">Lookup the specific transaction by ID. (optional)</param>
        /// <param name="round">Include results for the specified round. (optional)</param>
        /// <param name="minRound">Include results at or after the specified min-round. (optional)</param>
        /// <param name="maxRound">Include results at or before the specified max-round. (optional)</param>
        /// <param name="beforeTime">Include results before the given time. Must be an RFC 3339 formatted string. (optional)</param>
        /// <param name="afterTime">Include results after the given time. Must be an RFC 3339 formatted string. (optional)</param>
        /// <param name="currencyGreaterThan">Results should have an amount greater than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="currencyLessThan">Results should have an amount less than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="address">Only include transactions with this address in one of the transaction fields. (optional)</param>
        /// <param name="addressRole">Combine with the address parameter to define what type of address to search for. (optional)</param>
        /// <param name="excludeCloseTo">Combine with address and address-role parameters to define what type of address to search for. The close to fields are normally treated as a receiver, if you would like to exclude them set this parameter to true. (optional)</param>
        /// <param name="rekeyTo">Include results which include the rekey-to field. (optional)</param>
        /// <returns>TransactionsResponse</returns>
        public TransactionsResponse LookupAssetTransactions(long? assetId, long? limit = null, string next = null, 
            string notePrefix = null, string txType = null, string sigType = null, string txid = null, 
            long? round = null, long? minRound = null, long? maxRound = null, DateTime? beforeTime = null, 
            DateTime? afterTime = null, long? currencyGreaterThan = null, long? currencyLessThan = null, 
            string address = null, string addressRole = null, bool? excludeCloseTo = null, bool? rekeyTo = null)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupAssetTransactions(assetId, limit, next, notePrefix, txType, sigType, txid, 
                round, minRound, maxRound, beforeTime, afterTime, currencyGreaterThan, currencyLessThan, 
                address, addressRole, excludeCloseTo, rekeyTo);
        }

        /// <summary>
        /// Lookup block.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="roundNumber">Round number</param>
        /// <returns>Block</returns>
        public Block LookupBlock(long? roundNumber)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupBlock(roundNumber);
        }

        /// <summary>
        /// Lookup a single transaction.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="txid"></param>
        /// <returns>TransactionResponse</returns>
        public TransactionResponse LookupTransactions(string txid)
        {
            if (_lookupapi is null) _lookupapi = new LookupApi(_config);
            return _lookupapi.LookupTransactions(txid);
        }
        /// <summary>
        ///  Search for accounts.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assetId">Asset ID (optional)</param>
        /// <param name="limit">Maximum number of results to return. (optional)</param>
        /// <param name="next">The next page of results. Use the next token provided by the previous results. (optional)</param>
        /// <param name="currencyGreaterThan">Results should have an amount greater than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="currencyLessThan">Results should have an amount less than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="authAddr">Include accounts configured to use this spending key. (optional)</param>
        /// <param name="round">Include results for the specified round. For performance reasons, this parameter may be disabled on some configurations. (optional)</param>
        /// <param name="applicationId">Application ID (optional)</param>
        /// <returns>AccountsResponse</returns>
        public AccountsResponse SearchForAccounts(long? assetId = null, long? limit = null, string next = null, long? currencyGreaterThan = null, long? currencyLessThan = null, string authAddr = null, long? round = null, long? applicationId = null)
        {
            if (_searchapi is null) _searchapi = new SearchApi(_config);
            return _searchapi.SearchForAccounts(assetId, limit, next, currencyGreaterThan, currencyLessThan, authAddr, round, applicationId);
        }
        /// <summary>
        ///  Search for applications
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId">Application ID (optional)</param>
        /// <param name="limit">Maximum number of results to return. (optional)</param>
        /// <param name="next">The next page of results. Use the next token provided by the previous results. (optional)</param>
        /// <returns>ApplicationsResponse</returns>
        public ApplicationsResponse SearchForApplications(long? applicationId = null, long? limit = null, string next = null)
        {
            if (_searchapi is null) _searchapi = new SearchApi(_config);
            return _searchapi.SearchForApplications(applicationId, limit, next);
        }
        /// <summary>
        ///  Search for assets.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit">Maximum number of results to return. (optional)</param>
        /// <param name="next">The next page of results. Use the next token provided by the previous results. (optional)</param>
        /// <param name="creator">Filter just assets with the given creator address. (optional)</param>
        /// <param name="name">Filter just assets with the given name. (optional)</param>
        /// <param name="unit">Filter just assets with the given unit. (optional)</param>
        /// <param name="assetId">Asset ID (optional)</param>
        /// <returns>AssetsResponse</returns>
        public AssetsResponse SearchForAssets(long? limit = null, string next = null, string creator = null, string name = null, string unit = null, long? assetId = null)
        {
            if (_searchapi is null) _searchapi = new SearchApi(_config);
            return _searchapi.SearchForAssets(limit, next, creator, name, unit, assetId);
        }

        /// <summary>
        ///  Search for transactions.
        /// </summary>
        /// <exception cref="Algorand.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit">Maximum number of results to return. (optional)</param>
        /// <param name="next">The next page of results. Use the next token provided by the previous results. (optional)</param>
        /// <param name="notePrefix">Specifies a prefix which must be contained in the note field. (optional)</param>
        /// <param name="txType"> (optional)</param>
        /// <param name="sigType">SigType filters just results using the specified type of signature: * sig - Standard * msig - MultiSig * lsig - LogicSig (optional)</param>
        /// <param name="txid">Lookup the specific transaction by ID. (optional)</param>
        /// <param name="round">Include results for the specified round. (optional)</param>
        /// <param name="minRound">Include results at or after the specified min-round. (optional)</param>
        /// <param name="maxRound">Include results at or before the specified max-round. (optional)</param>
        /// <param name="assetId">Asset ID (optional)</param>
        /// <param name="beforeTime">Include results before the given time. Must be an RFC 3339 formatted string. (optional)</param>
        /// <param name="afterTime">Include results after the given time. Must be an RFC 3339 formatted string. (optional)</param>
        /// <param name="currencyGreaterThan">Results should have an amount greater than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="currencyLessThan">Results should have an amount less than this value. MicroAlgos are the default currency unless an asset-id is provided, in which case the asset will be used. (optional)</param>
        /// <param name="address">Only include transactions with this address in one of the transaction fields. (optional)</param>
        /// <param name="addressRole">Combine with the address parameter to define what type of address to search for. (optional)</param>
        /// <param name="excludeCloseTo">Combine with address and address-role parameters to define what type of address to search for. The close to fields are normally treated as a receiver, if you would like to exclude them set this parameter to true. (optional)</param>
        /// <param name="rekeyTo">Include results which include the rekey-to field. (optional)</param>
        /// <param name="applicationId">Application ID (optional)</param>
        /// <returns>TransactionsResponse</returns>
        public TransactionsResponse SearchForTransactions(long? limit = null, string next = null, string notePrefix = null, string txType = null, string sigType = null, string txid = null, long? round = null, long? minRound = null, long? maxRound = null, long? assetId = null, DateTime? beforeTime = null, DateTime? afterTime = null, long? currencyGreaterThan = null, long? currencyLessThan = null, string address = null, string addressRole = null, bool? excludeCloseTo = null, bool? rekeyTo = null, long? applicationId = null)
        {
            if (_searchapi is null) _searchapi = new SearchApi(_config);
            return _searchapi.SearchForTransactions(limit, next, notePrefix, txType, sigType, txid, round, minRound, maxRound, assetId, beforeTime, afterTime, currencyGreaterThan, currencyLessThan, address, addressRole, excludeCloseTo, rekeyTo, applicationId);
        }
    }
}
