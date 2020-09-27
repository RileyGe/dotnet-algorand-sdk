@unit
Feature: Indexer Client v2 Responses

  @unit.indexer
  Scenario Outline: LookupAssetBalances response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any LookupAssetBalances call
    Then expect error string to contain "<err>"
    And the parsed LookupAssetBalances response should be valid on round <roundNum>, and contain an array of len <len> and element number <idx> should have address "<address>" amount <amount> and frozen state "<frozenState>"
    Examples:
      |jsonfiles                 |directory                    |err|roundNum      |len |idx|address                                                         |amount      |frozenState|
      |lookupAssetBalances_0.json|v2indexerclient_responsejsons||6222956       |22  |1  |B2IID7Y54ROV2H6OXO3IXVSF64CBJ2GLEKCHYE2ZZE63RBMROO5FARL4IQ      |4900000     |      false|

  @unit.indexer
  Scenario Outline: LookupAssetTransactions response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any LookupAssetTransactions call
    Then expect error string to contain "<err>"
    And the parsed LookupAssetTransactions response should be valid on round <roundNum>, and contain an array of len <len> and element number <idx> should have sender "<sender>"
    Examples:
      |jsonfiles                     |directory                    |err|roundNum|len    |idx|sender|
      |lookupAssetTransactions_0.json|v2indexerclient_responsejsons||6222957 |39  |0  |ARCC3TMGVD7KXY7GYTE7U5XXUJXFRD2SXLAWRV57XJ6HWHRR37GNGNMPSY  |

  @unit.indexer
  Scenario Outline: LookupAccountTransactions response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any LookupAccountTransactions call
    Then expect error string to contain "<err>"
    And the parsed LookupAccountTransactions response should be valid on round <roundNum>, and contain an array of len <len> and element number <idx> should have sender "<sender>"
    Examples:
      |jsonfiles                       |directory                       |err|roundNum      |len|idx|sender|
      |lookupAccountTransactions_0.json|  v2indexerclient_responsejsons ||6222956       |0  |0  | N/A  |
    
  @unit.indexer
  Scenario Outline: LookupBlock response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any LookupBlock call
    Then expect error string to contain "<err>"
    And the parsed LookupBlock response should have previous block hash "<prevHash>"
    Examples:
      |jsonfiles          |directory                      |err|prevHash|
      |lookupBlock_0.json | v2indexerclient_responsejsons ||jIYt/5YlDB/fzOoLIZXKwCD9e9Y37YXoz3umPCoo7Rw=    |

  @unit.indexer
  Scenario Outline: LookupAccountByID response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any LookupAccountByID call
    Then expect error string to contain "<err>"
    And the parsed LookupAccountByID response should have address "<address>"
    Examples:
      |jsonfiles                |directory                      |err|address|
      |lookupAccountByID_0.json | v2indexerclient_responsejsons ||BZNKXBBXIVZ7GFAXHNC26ERYUD5TQWWV327IU2N4SKG6WLNFGMMCRQEFZE    |

  @unit.indexer
  Scenario Outline: LookupAssetByID response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any LookupAssetByID call
    Then expect error string to contain "<err>"
    And the parsed LookupAssetByID response should have index <index>
    Examples:
      |jsonfiles               |directory                      |err|index  |
      |lookupAssetByID_0.json  | v2indexerclient_responsejsons ||163650|

  @unit.indexer
  Scenario Outline: SearchAccounts response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any SearchAccounts call
    Then expect error string to contain "<err>"
    And the parsed SearchAccounts response should be valid on round <roundNum> and the array should be of len <len> and the element at index <index> should have address "<address>"
    Examples:
      |jsonfiles                |directory                      |err|roundNum  | len | index | address |
      |searchForAccounts_0.json |  v2indexerclient_responsejsons||6222956   |1    |0      |BZNKXBBXIVZ7GFAXHNC26ERYUD5TQWWV327IU2N4SKG6WLNFGMMCRQEFZE|

  @unit.indexer
  Scenario Outline: SearchForTransactions response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any SearchForTransactions call
    Then expect error string to contain "<err>"
    And the parsed SearchForTransactions response should be valid on round <roundNum> and the array should be of len <len> and the element at index <index> should have sender "<sender>"
    Examples:
      |jsonfiles                    |directory                    |err|roundNum        | len  | index | sender                                                        |
      |searchForTransactions_0.json |v2indexerclient_responsejsons||6222958         |10    |0      |I3345FUQQ2GRBHFZQPLYQQX5HJMMRZMABCHRLWV6RCJYC6OO4MOLEUBEGU     |

  @unit.indexer
  Scenario Outline: SearchForAssets response
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any SearchForAssets call
    Then expect error string to contain "<err>"
    And the parsed SearchForAssets response should be valid on round <roundNum> and the array should be of len <len> and the element at index <index> should have asset index <assetIndex>
    Examples:
      |jsonfiles                  |directory                     |err|roundNum   | len | index | assetIndex  |
      |searchForAssets_0.json     |v2indexerclient_responsejsons ||6222956    |3    |0      | 5           |

  @unit.indexer.rekey
  Scenario Outline: SearchForAccounts response, authorizing address
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any SearchAccounts call
    And the parsed SearchAccounts response should be valid on round <roundNum> and the array should be of len <len> and the element at index <index> should have authorizing address "<authaddr>"
    Examples:
      |jsonfiles                |directory                      |err|roundNum  | len | index | authaddr |
      |searchForAccounts_1.json |  v2indexerclient_responsejsons||6222956   |1    |0      |PRIC4GIQTJFD2SZIEQGAYBV2KUJ7YQR3EV3KSOZKLOHPDNRDXXVWMHDAQA|

  @unit.indexer.rekey
  Scenario Outline: SearchForTransactions response, rekey-to
    Given mock http responses in "<jsonfiles>" loaded from "<directory>"
    When we make any SearchForTransactions call
    And the parsed SearchForTransactions response should be valid on round <roundNum> and the array should be of len <len> and the element at index <index> should have rekey-to "<rekeyto>"
    Examples:
      |jsonfiles                    |directory                    |err|roundNum        | len  | index | rekeyto |
      |searchForTransactions_1.json |v2indexerclient_responsejsons||6222958         |10    |1      |PRIC4GIQTJFD2SZIEQGAYBV2KUJ7YQR3EV3KSOZKLOHPDNRDXXVWMHDAQA     |
