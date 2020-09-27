@unit
Feature: Algod REST API v2 Paths
  Background:
    Given mock server recording request paths
  # SendRaw, Status, Supply, SuggestedParams, and PendingTransactionsInformation omitted - the path never mutates, they're constant

  @unit.algod
  Scenario Outline: Pending Transaction Information
    When we make a Pending Transaction Information against txid "<txid>" with format "<format>"
    Then expect the path used to be "<path>"
    Examples:
      |path | txid| format |
      |/v2/transactions/pending/5FJDJD5LMZC3EHUYYJNH5I23U4X6H2KXABNDGPIL557ZMJ33GZHQ?format=msgpack | 5FJDJD5LMZC3EHUYYJNH5I23U4X6H2KXABNDGPIL557ZMJ33GZHQ| msgpack |

  @unit.algod
  Scenario Outline: Pending Transaction Information
    When we make a Pending Transaction Information with max <max> and format "<format>"
    Then expect the path used to be "<path>"
    Examples:
      |path | max| format |
      |/v2/transactions/pending?format=msgpack | 0  | msgpack |
      |/v2/transactions/pending?format=msgpack&max=1 | 1  | msgpack |

  @unit.algod
  Scenario Outline: Pending Transactions By Address
    When we make a Pending Transactions By Address call against account "<account>" and max <max> and format "<format>"
    Then expect the path used to be "<path>"
    Examples:
      |path | account| max | format |
      |/v2/accounts/7ZUECA7HFLZTXENRV24SHLU4AVPUTMTTDUFUBNBD64C73F3UHRTHAIOF6Q/transactions/pending?format=msgpack | 7ZUECA7HFLZTXENRV24SHLU4AVPUTMTTDUFUBNBD64C73F3UHRTHAIOF6Q   | 0  | msgpack |
      |/v2/accounts/7ZUECA7HFLZTXENRV24SHLU4AVPUTMTTDUFUBNBD64C73F3UHRTHAIOF6Q/transactions/pending?format=msgpack&max=1 | 7ZUECA7HFLZTXENRV24SHLU4AVPUTMTTDUFUBNBD64C73F3UHRTHAIOF6Q   | 1  | msgpack |

  @unit.algod
  Scenario Outline: Status After Block
    When we make a Status after Block call with round <round>
    Then expect the path used to be "<path>"
    Examples:
      |path | round|
      |/v2/status/wait-for-block-after/0 | 0    |

  @unit.algod
  Scenario Outline: Account Information
    When we make an Account Information call against account "<account>"
    Then expect the path used to be "<path>"
    Examples:
      |path | account|
      |/v2/accounts/7ZUECA7HFLZTXENRV24SHLU4AVPUTMTTDUFUBNBD64C73F3UHRTHAIOF6Q | 7ZUECA7HFLZTXENRV24SHLU4AVPUTMTTDUFUBNBD64C73F3UHRTHAIOF6Q   |

  @unit.algod
  Scenario Outline: Get Block
    When we make a Get Block call against block number <round> with format "<format>"
    Then expect the path used to be "<path>"
    Examples:
      |path | round| format |
      |/v2/blocks/0?format=msgpack | 0    | msgpack |

  @unit.applications
  Scenario Outline: GetAssetByID
    When we make a GetAssetByID call for assetID <asset-id>
    Then expect the path used to be "<path>"
    Examples:
      |path            | asset-id |
      |/v2/assets/1234 | 1234     |

  @unit.applications
  Scenario Outline: GetApplicationByID
    When we make a GetApplicationByID call for applicationID <application-id>
    Then expect the path used to be "<path>"
    Examples:
      |path                  | application-id |
      |/v2/applications/1234 | 1234           |
