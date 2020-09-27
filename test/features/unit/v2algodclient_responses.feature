@unit
Feature: Algod REST API v2 Responses

  @unit.algod
  Scenario Outline: Pending Transaction Information response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Pending Transaction Information call
    Then expect error string to contain "<err>"
    And the parsed Pending Transaction Information response should have sender "<sender>"
    Examples:
      |jsonfiles                       |directory                  |err|sender|
      |pendingTransactionsByTxid.base64|v2algodclient_responsejsons||MUDNR6GODUNBSJDHTSB2L6MRZLYEKMO2ZDBJRE3BALB6YSM55UU6KFGKYQ  |

  @unit.algod
  Scenario Outline: Pending Transactions Information response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Pending Transactions Information call
    Then expect error string to contain "<err>"
    And the parsed Pending Transactions Information response should contain an array of len <len> and element number <idx> should have sender "<sender>"
    Examples:
      |jsonfiles                       |directory                  |err|len|idx|sender|
      |pendingTransactions.base64      |v2algodclient_responsejsons||3  |0  |MUDNR6GODUNBSJDHTSB2L6MRZLYEKMO2ZDBJRE3BALB6YSM55UU6KFGKYQ  |

  @unit.algod
  Scenario Outline: Send Raw Transaction response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Send Raw Transaction call
    Then expect error string to contain "<err>"
    And the parsed Send Raw Transaction response should have txid "<txid>"
    Examples:
      |jsonfiles  |directory                  |err|txid|
      |sendTx.json|v2algodclient_responsejsons||ITRPGGAIHEAADRU2W535P5UUEJHYHRN6LZRBJ7E56XSUKJRMZRSQ|

  @unit.algod
  Scenario Outline: Pending Transactions By Address response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Pending Transactions By Address call
    Then expect error string to contain "<err>"
    And the parsed Pending Transactions By Address response should contain an array of len <len> and element number <idx> should have sender "<sender>"
    Examples:
      |jsonfiles                       |directory                  |err|len|idx|sender|
      |pendingTransactionsByAddr.base64|v2algodclient_responsejsons||3  |0  |MUDNR6GODUNBSJDHTSB2L6MRZLYEKMO2ZDBJRE3BALB6YSM55UU6KFGKYQ  |

  @unit.algod
  Scenario Outline: Node Status response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Node Status call
    Then expect error string to contain "<err>"
    And the parsed Node Status response should have a last round of <roundNum>
    Examples:
      |jsonfiles       |directory                    |err|roundNum|
      |status.json     | v2algodclient_responsejsons ||6222190       |

  @unit.algod
  Scenario Outline: Ledger Supply response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Ledger Supply call
    Then expect error string to contain "<err>"
    And the parsed Ledger Supply response should have totalMoney <tot> onlineMoney <online> on round <roundNum>
    Examples:
      |jsonfiles        |directory                  |err|tot             |online          |roundNum|
      |ledgerSupply.json|v2algodclient_responsejsons||3093025985939942|2189181532333805|6222141 |

  @unit.algod
  Scenario Outline: Status After Block response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Status After Block call
    Then expect error string to contain "<err>"
    And the parsed Status After Block response should have a last round of <roundNum>
    Examples:
      |jsonfiles       |directory                    |err|roundNum|
      |status.json     | v2algodclient_responsejsons ||6222190       |

  @unit.algod
  Scenario Outline: Account Information response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Account Information call
    Then expect error string to contain "<err>"
    And the parsed Account Information response should have address "<address>"
    Examples:
      |jsonfiles              |directory                  |err|address|
      |accountInformation.json|v2algodclient_responsejsons||ALGORANDAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIN5DNAU   |

  @unit.algod
  Scenario Outline: Get Block response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Get Block call
    Then expect error string to contain "<err>"
    And the parsed Get Block response should have rewards pool "<pool>"
    Examples:
      |jsonfiles   |directory                  |err|pool    |
      |block.base64|v2algodclient_responsejsons||//////////////////////////////////////////8=|

  @unit.algod
  Scenario Outline: Suggested Transaction Parameters response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Suggested Transaction Parameters call
    Then expect error string to contain "<err>"
    And the parsed Suggested Transaction Parameters response should have first round valid of <roundNum>
    Examples:
      |jsonfiles           |directory                   |err|roundNum|
      |suggestedParams.json| v2algodclient_responsejsons||6222155|

  @unit.dryrun
  Scenario Outline: Dryrun response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any Dryrun call
    Then the parsed Dryrun Response should have global delta <key> with <action>
    Examples:
      | jsonfiles           | directory                   | key        | action |
      | dryrunResponse.json | v2algodclient_responsejsons | "Creator"  | 2      |
