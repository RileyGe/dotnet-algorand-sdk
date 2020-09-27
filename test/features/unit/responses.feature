@unit
Feature: v2 REST Client Responses

  Scenario Outline: <client>: <endpoint> - <jsonfile>
    Given mock http responses in "<jsonfile>" loaded from "generated_responses" with status <status>.
    When we make any "<client>" call to "<endpoint>".
    Then the parsed response should equal the mock response.
    @unit.responses
    Examples:
      | jsonfile                                                | status | client  | endpoint                        |
      | indexer_applications_AccountResponse_0.json             | 200    | indexer | lookupAccountByID               |
      | indexer_applications_AccountsResponse_0.json            | 200    | indexer | searchForAccounts               |
      | indexer_applications_ApplicationResponse_0.json         | 200    | indexer | lookupApplicationByID           |
      | indexer_applications_ApplicationsResponse_0.json        | 200    | indexer | searchForApplications           |
      | indexer_applications_AssetBalancesResponse_0.json       | 200    | indexer | lookupAssetBalances             |
      | indexer_applications_AssetResponse_0.json               | 200    | indexer | lookupAssetByID                 |
      | indexer_applications_AssetsResponse_0.json              | 200    | indexer | searchForAssets                 |
      | indexer_applications_TransactionsResponse_0.json        | 200    | indexer | lookupAccountTransactions       |
      | indexer_applications_TransactionsResponse_0.json        | 200    | indexer | lookupAssetTransactions         |
      | indexer_applications_TransactionsResponse_0.json        | 200    | indexer | searchForTransactions           |
      | indexer_applications_TransactionsResponse_1.json        | 200    | indexer | lookupAccountTransactions       |
      | indexer_applications_TransactionsResponse_1.json        | 200    | indexer | lookupAssetTransactions         |
      | indexer_applications_TransactionsResponse_1.json        | 200    | indexer | searchForTransactions           |
      | indexer_applications_TransactionsResponse_2.json        | 200    | indexer | lookupAccountTransactions       |
      | indexer_applications_TransactionsResponse_2.json        | 200    | indexer | lookupAssetTransactions         |
      | indexer_applications_TransactionsResponse_2.json        | 200    | indexer | searchForTransactions           |
      | indexer_applications_ErrorResponse_0.json               | 500    | indexer | any                             |
      | algod_applications_NodeStatusResponse_0.json            | 200    | algod   | GetStatus                       |
      | algod_applications_NodeStatusResponse_0.json            | 200    | algod   | WaitForBlock                    |
      | algod_applications_CompileResponse_0.json               | 200    | algod   | TealCompile                     |
      | algod_applications_PostTransactionsResponse_0.json      | 200    | algod   | RawTransaction                  |
      | algod_applications_SupplyResponse_0.json                | 200    | algod   | GetSupply                       |
      | algod_applications_TransactionParametersResponse_0.json | 200    | algod   | TransactionParams               |
      | algod_applications_ApplicationResponse_0.json           | 200    | algod   | GetApplicationByID              |
      | algod_applications_AssetResponse_0.json                 | 200    | algod   | GetAssetByID                    |
      | algod_applications_ErrorResponse_0.json                 | 500    | algod   | any                             |

    @unit.responses.messagepack
    Examples:
      | jsonfile                                                | status | client  | endpoint                        |
      | algod_applications_PendingTransactionResponse_0.base64  | 200    | algod   | PendingTransactionInformation   |
      | algod_applications_PendingTransactionResponse_1.base64  | 200    | algod   | PendingTransactionInformation   |
      | algod_applications_PendingTransactionsResponse_0.base64 | 200    | algod   | GetPendingTransactions          |
      | algod_applications_PendingTransactionsResponse_1.base64 | 200    | algod   | GetPendingTransactions          |
      | algod_applications_PendingTransactionsResponse_0.base64 | 200    | algod   | GetPendingTransactionsByAddress |
      | algod_applications_PendingTransactionsResponse_1.base64 | 200    | algod   | GetPendingTransactionsByAddress |
      # -- These are missing proper response objects, so they were not generated.
      # GetBlock
      # health check
      # metrics
      # Register participation keys
      # ShutdownNode
      # -- Missing a proper path definition
      # Versions
      # -- This is still WIP?
      # Dryrun

