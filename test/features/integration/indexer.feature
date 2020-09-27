Feature: Indexer Integration Tests

  For all queries, parameters will not be set for default values as defined by:
    * Numeric inputs: 0
    * String inputs: ""

  Background:
    Given indexer client 1 at "localhost" port 59999 with token "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
    Given indexer client 2 at "localhost" port 59998 with token "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

  @indexer
  Scenario Outline: /health
    When I use <indexer> to check the services health
    Then I receive status code <status>

    Examples:
      | indexer | status |
      | 1       | 200    |

  #
  # /blocks/{round-number}
  #
  @indexer
  Scenario Outline: /blocks/<number>
    When I use <indexer> to lookup block <number>
    Then The block was confirmed at <timestamp>, contains <num> transactions, has the previous block hash "<hash>"

    Examples:
      | indexer | number | timestamp  | num | hash                                         |
      | 1       | 7      | 1585684086 | 8   | PpPusF+bU/uNLS5ODG/hG0pP0vehdSSlBcnnyZDr770= |
      | 1       | 20     | 1585684138 | 2   | 9jzxFIKLoTGkFl60aqGwyzO0AVyMBnbs/Wb5R9hPrsA= |
      | 1       | 100    | 1585684463 | 0   | rEWRbwgzDagT5wYTf9TuiuC+VR3XLLy4S73vInxkmrA= |

  #
  # /accounts/{account-id}
  #
  @indexer
  Scenario Outline: has asset - /account/<account>
    When I use <indexer> to lookup account "<account>" at round 0
    Then The account has <num> assets, the first is asset <index> has a frozen status of "<frozen>" and amount <units>.

    Examples:
      | indexer | account                                                    | num | index | frozen | units        |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 1   | 9     | false  | 999931337000 |
      | 1       | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA | 1   | 9     | false  | 68663000     |

  @indexer
  Scenario Outline: creator - /account/<account>
    When I use <indexer> to lookup account "<account>" at round 0
    Then The account created <num> assets, the first is asset <index> is named "<name>" with a total amount of <total> "<unit>"

    Examples:
      | indexer | account                                                    | num | index | name     | total         | unit |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 1   | 9     | bogocoin | 1000000000000 | bogo |

  @indexer
  Scenario Outline: lookup - /account/<account>
    When I use <indexer> to lookup account "<account>" at round 0
    Then The account has <μalgos> μalgos and <num> assets, 0 has 0

    Examples:
      | indexer | account                                                    | μalgos           | num |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 999899126000     | 1   |
      | 1       | FROJFIFQRARWEHOL6GR3MBFCDJY76CPF3UY55HM3PCK42AD5HA5SKKXLLA | 4992999999993000 | 0   |

  #
  # /accounts/{account-id} - at round (rollback test)
  #
  @indexer
  Scenario Outline: rewind - /accounts/{account-id}?round=<round>
    When I use <indexer> to lookup account "<account>" at round <round>
    Then The account has <μalgos> μalgos and <num> assets, <asset-id> has <asset-amount>

    Examples:
      | indexer | account                                                    | round | μalgos        | num | asset-id | asset-amount  |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 351   | 999899126000  | 1   | 9        | 999931337000  |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 30    | 999899126000  | 1   | 9        | 999931337000  |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 29    | 999898989000  | 1   | 9        | 999900000000  |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 27    | 999998990000  | 1   | 9        | 999900000000  |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 25    | 999998991000  | 1   | 9        | 1000000000000 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 21    | 999999992000  | 1   | 9        | 1000000000000 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 19    | 999998995000  | 1   | 9        | 1000000000000 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 17    | 999998995000  | 1   | 9        | 999999000000  |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 15    | 999998996000  | 1   | 9        | 1000000000000 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 11    | 999999997000  | 1   | 9        | 1000000000000 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 9     | 999899998000  | 1   | 9        | 1000000000000 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 6     | 999999999000  | 1   | 9        | 1000000000000 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 4     | 1000000000000 | 1   | 9        | 0             |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 2     | 0             | 1   | 9        | 0             |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 1     | 0             | 1   | 9        | 0             |

  #
  # /assets/{asset-id}
  #
  @indexer
  Scenario Outline: lookup - /assets/<asset-id>
    When I use <indexer> to lookup asset <asset-id>
    Then The asset found has: "<name>", "<units>", "<creator>", <decimals>, "<default-frozen>", <total>, "<clawback>"

    Examples:
      | indexer | asset-id | name     | units | creator                                                    | decimals | default-frozen | total         | clawback                                                   |
      | 1       | 9        | bogocoin | bogo  | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 0        | false          | 1000000000000 | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 |

  #
  # /assets/{asset-id}/balances
  #
  @indexer
  Scenario Outline: balances - /assets/<asset-id>/balances?gt=<currency-gt>&lt=<currency-lt>&limit=<limit>
    When I use <indexer> to lookup asset balances for <asset-id> with <currency-gt>, <currency-lt>, <limit> and token ""
    Then There are <num-accounts> with the asset, the first is "<account>" has "<is-frozen>" and <amount>

    Examples:
      | indexer | asset-id | currency-gt | currency-lt | limit | num-accounts | account                                                    | is-frozen | amount       |
      | 1       | 9        | 0           | 0           | 0     | 2            | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | false     | 999931337000 |
      | 1       | 9        | 0           | 0           | 1     | 1            | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | false     | 999931337000 |
      | 1       | 9        | 0           | 0           | 1     | 1            | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | false     | 999931337000 |
      | 1       | 9        | 68663000    | 0           | 0     | 1            | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | false     | 999931337000 |
      | 1       | 9        | 0           | 68663001    | 0     | 1            | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA | false     | 68663000     |


  @indexer
  Scenario Outline: /assets/{asset-id}/balances?next=token
    When I use <indexer> to lookup asset balances for <asset-id> with <currency-gt>, <currency-lt>, <limit> and token ""
    And I get the next page using <indexer> to lookup asset balances for <asset-id> with <currency-gt>, <currency-lt>, <limit>
    Then There are <num-accounts> with the asset, the first is "<account>" has "<is-frozen>" and <amount>

    Examples:
      | indexer | asset-id | currency-gt | currency-lt | limit | num-accounts | account                                                    | is-frozen | amount   |
      | 1       | 9        | 0           | 0           | 1     | 1            | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA | false     | 68663000 |

    #####################

  #
  # /accounts
  #
  @indexer
  Scenario Outline: general - /accounts?asset-id=<asset-id>&limit=<limit>&gt=<currency-gt>&lt=<currency-lt>
    When I use <indexer> to search for an account with <asset-id>, <limit>, <currency-gt>, <currency-lt> and token ""
    Then There are <num>, the first has <pending-rewards>, <rewards-base>, <rewards>, <without-rewards>, "<address>", <amount>, "<status>", "<type>"

    Examples:
      | indexer | asset-id | limit | currency-gt | currency-lt | num | pending-rewards | rewards-base | rewards | without-rewards | address                                                    | amount       | status  | type |
  # These changed when adding 'ORDER BY' in the backend
  #| 1       | 0        | 0     | 0           | 0           | 32  | 0               | 0            | 0       | 1000000         | XKWNJ6MDJWB5WEIARTAJI6GMCX3ETHBSM4OZ2NYACFEXHQJ2RHTC4SHH5A | 1000000      | Offline |      |
  #| 1       | 0        | 10    | 0           | 0           | 10  | 0               | 0            | 0       | 1000000         | XKWNJ6MDJWB5WEIARTAJI6GMCX3ETHBSM4OZ2NYACFEXHQJ2RHTC4SHH5A | 1000000      | Offline |      |
      | 1       | 0        | 0     | 0           | 0           | 32  | 0               | 0            | 0       | 0                | A5QNF7MATDBZHXVYROXVZ6WTWMMDGX5RPEUCYAQEINOS3LQUW7NQGUJ4OI | 0           | Offline | lsig |
      | 1       | 0        | 10    | 0           | 0           | 10  | 0               | 0            | 0       | 0                | A5QNF7MATDBZHXVYROXVZ6WTWMMDGX5RPEUCYAQEINOS3LQUW7NQGUJ4OI | 0           | Offline | lsig |
      | 1       | 9        | 0     | 68663000    | 0           | 1   | 0               | 0            | 0       | 999899126000    | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 999899126000 | Offline | sig  |
      | 1       | 9        | 0     | 0           | 68663001    | 1   | 0               | 0            | 0       | 998000          | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA | 998000       | Offline | lsig |
      | 1       | 0        | 0     | 798999      | 799001      | 1   | 0               | 0            | 0       | 799000          | RRHDAJKO5HQBLHPCVK6K7U54LENDIP2JKM3RNRYX2G254VUXBRQD35CK4E | 799000       | Offline | msig |

  #
  # /accounts - online
  #
  @indexer
  Scenario Outline: online - /accounts?asset-id=<asset-id>&limit=<limit>&gt=<currency-gt>&lt=<currency-lt>
    When I use <indexer> to search for an account with <asset-id>, <limit>, <currency-gt>, <currency-lt> and token ""
    Then The first account is online and has "<address>", <key-dilution>, <first-valid>, <last-valid>, "<vote-key>", "<selection-key>"

    Examples:
      | indexer | asset-id | limit | currency-gt      | currency-lt | address                                                    | key-dilution | first-valid | last-valid | vote-key                                     | selection-key                                |
      | 1       | 0        | 0     | 998999           | 999001      | NNFTUMXU5EMDOSFRGQ55TOGOJIS7P7POIDHJTQNQUBVVYJ6GCIPHOMAMQE | 10000        | 0           | 100        | h0wDwh1yhWiWu0S79wEiQaWXnNLCMjcce5MPeWPRQ/Q= | Ob0jBcHd0uh6nMjls6bOHlissWvPlINGiREJ+gaEOSg= |
  # These changed when adding 'ORDER BY' in the backend
  #| 1       | 0        | 0     | 4992999999992999 | 0           | FROJFIFQRARWEHOL6GR3MBFCDJY76CPF3UY55HM3PCK42AD5HA5SKKXLLA | 10000        | 0           | 3000000    | mQzj8cwerZh1QzdCR9WBteLQ6MQszzLP4MAjSi5wuD4= | NRnpzxRIGUnTICoPloP9eWU1W6OPksR0ReEDRTwzoYg= |
      | 1       | 0        | 0     | 4992999999992999 | 0           | BYP7VVRIBDOOFKEYICNYIM43S6DW7RIZC73XNMKF3KT5YUITDDMH3W5D5Q | 10000        | 0           | 3000000    | 9OO2S7ikfESeDZg8Z9mrzdN2Lh52UBSVH9uD7XqQHhs= | BkTjDJB2Su5Fi9uwJTODkxpEjrhCJSYtF10m0ee6THU= |


  #
  # /accounts - paging
  #
  @indexer
  Scenario Outline: paging - /accounts?asset-id=<asset-id>&limit=<limit>&gt=<currency-gt>&lt=<currency-lt>
    When I use <indexer> to search for an account with <asset-id>, <limit>, <currency-gt>, <currency-lt> and token ""
    Then I get the next page using <indexer> to search for an account with <asset-id>, <limit>, <currency-gt> and <currency-lt>
    Then There are <num>, the first has <pending-rewards>, <rewards-base>, <rewards>, <without-rewards>, "<address>", <amount>, "<status>", "<type>"

    Examples:
      | indexer | asset-id | limit | currency-gt | currency-lt | num | pending-rewards | rewards-base | rewards | without-rewards | address                                                    | amount       | status           | type |
      | 1       | 0        | 1     | 0           | 0           | 1   | 0               | 0            | 0       | 149234          | A7NMWS3NT3IUDMLVO26ULGXGIIOUQ3ND2TXSER6EBGRZNOBOUIQXHIBGDE | 149234       | NotParticipating |      |
  # These changed when adding 'ORDER BY' in the backend
  #| 1       | 0        | 10    | 0           | 0           | 10  | 0               | 0            | 0       | 99862000        | M7E3Z6MJ7LZT725IK3ZQ6YE64TUTVU6VPPHFMT3DSD5KRDYRE44BE6GY5A | 99862000     | Offline          | lsig |
      | 1       | 0        | 10    | 0           | 0           | 10  | 0               | 0            | 0       |  999899996766   | LQU5S7HMDXLQUQD5BKIMPPZYK7LYXPC5AVGIWNVNTBVQHL3GCXFVXZFJ3A | 999899996766 | Offline          | sig  |

  #
  # /accounts - paging multiple times
  #
  @indexer
  Scenario Outline: paging 6 times - /accounts?asset-id=<asset-id>&limit=<limit>&gt=<currency-gt>&lt=<currency-lt>
    When I use <indexer> to search for an account with <asset-id>, <limit>, <currency-gt>, <currency-lt> and token ""
    Then I get the next page using <indexer> to search for an account with <asset-id>, <limit>, <currency-gt> and <currency-lt>
    Then I get the next page using <indexer> to search for an account with <asset-id>, <limit>, <currency-gt> and <currency-lt>
    Then I get the next page using <indexer> to search for an account with <asset-id>, <limit>, <currency-gt> and <currency-lt>
    Then I get the next page using <indexer> to search for an account with <asset-id>, <limit>, <currency-gt> and <currency-lt>
    Then I get the next page using <indexer> to search for an account with <asset-id>, <limit>, <currency-gt> and <currency-lt>
    Then I get the next page using <indexer> to search for an account with <asset-id>, <limit>, <currency-gt> and <currency-lt>
    Then There are <num>, the first has <pending-rewards>, <rewards-base>, <rewards>, <without-rewards>, "<address>", <amount>, "<status>", "<type>"

    Examples:
      | indexer | asset-id | limit | currency-gt | currency-lt | num | pending-rewards | rewards-base | rewards | without-rewards | address                                                    | amount       | status  | type |
      | 1       | 0        | 1     | 0           | 0           | 1   | 0               | 0            | 0       | 0               | GP44P6YCVSRK4IYIEZYDYO5POY3QO5VTATZIMRI6DFLMO2EPK7GBBNQRCM | 0            | Offline | lsig |
      | 1       | 0        | 2     | 0           | 0           | 2   | 0               | 0            | 0       | 999000          | NNFTUMXU5EMDOSFRGQ55TOGOJIS7P7POIDHJTQNQUBVVYJ6GCIPHOMAMQE | 999000       | Online | sig  |

  #
  # /transactions
  #  When I use <indexer> to search for transactions with <limit>, "<note-prefix>", "<tx-type>", "<sig-type>", "<tx-id>", <round>, <min-round>, <max-round>, <asset-id>, "<before-time>", "<after-time>", <currency-gt>, <currency-lt>, "<address>", "<address-role>", "<exclude-close-to>"  and token "<token>"
  #
  @indexer
  Scenario Outline: /transactions?note-prefix
    When I use <indexer> to search for transactions with 0, "<note-prefix>", "", "", "", 0, 0, 0, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".

    Examples:
      | indexer | note-prefix | num  | txid                                                 |
      | 1       | XQ==        | 2    | U2KNU7B55LZU6SWX66VHIZMJT4OX6YMNCIKBLV4BLYVTBCU3ZW2A |
      | 1       | VA==        | 3    | IMFJQCCF5T2DOVSKHP2NHDKV5A2VGVIW24LNQUBDOH33UMIE545Q |
      | 1       | 1111        | 0    |                                                      |

  @indexer
  Scenario Outline: /transactions?tx-type=<tx-type>
    When I use <indexer> to search for transactions with 0, "", "<tx-type>", "", "", 0, 0, 0, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction has tx-type "<tx-type>"

    Examples:
      | indexer | tx-type | num  | txid                                                 |
      | 1       | pay     | 41   | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | keyreg  | 1    | HG3DLU47GVRCLIG3SIHM6TTWINUN7VOKKAJWZRYZQA4NDO7PNXMA |
      | 1       | acfg    | 1    | KGG5ZGQQ57Y2ZDH5CFRYMJODPJ4TVIQBAPKT3HK3PIS6A6K4T5GQ |
      | 1       | axfer   | 6    | IIWBLLEXCFDQQHENIU2JBXSFDNNHLO5C2M5PE3UIHQ3YXN2TWRUA |
      | 1       | afrz    | 0    |                                                      |

  @indexer
  Scenario Outline: /transactions?sig-type=<sig-type>
    When I use <indexer> to search for transactions with 0, "", "", "<sig-type>", "", 0, 0, 0, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction has sig-type "<sig-type>"

    Examples:
      | indexer | sig-type | num  | txid                                                 |
      | 1       | sig      | 25   | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | lsig     | 24   | SPRRY5NZETQFP3C7MHEDURQRUUC5JVESSRXRXVJAHENZ6OJJKMXQ |
      | 1       | msig     | 0    |                                                      |

  @indexer
  Scenario Outline: /transactions?tx-id=<txid>
    When I use <indexer> to search for transactions with 0, "", "", "", "<txid>", 0, 0, 0, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".

    Examples:
      | indexer | num | txid                                                 |
      | 1       | 0   | DN7MBMCL5JQ3PFUQS7TMX5AH4EEKOBJVDUF4TCV6WERATKFLQF4M |
      | 1       | 1   | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 1   | SPRRY5NZETQFP3C7MHEDURQRUUC5JVESSRXRXVJAHENZ6OJJKMXQ |

  @indexer
  Scenario Outline: /transactions?round=<round>
    When I use <indexer> to search for transactions with 0, "", "", "", "", <round>, 0, 0, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction has round <round>

    Examples:
      | indexer | round | num | txid                                                 |
      | 1       | 10    | 2   | TF5YEFGB7AQT4ZXVGBEY76TGV7D2QF4HMSGAVGUJE764ZIGXS3NQ |
      | 1       | 22    | 3   | DWSC3DPKFU7TIPMOVJZM25XYBUNJQASGVDYS7QREO33UEA6FYSNA |
      | 1       | 30    | 2   | 5UWAFFNPPECDJHAYYRRLE3WIWGO2WVH4LZMPLVQBS4E76UERRATA |

  @indexer
  Scenario Outline: /transactions?min-round=<min-round>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, <min-round>, 0, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction has round >= <min-round>

    Examples:
      | indexer | min-round | num | txid                                                 |
      | 1       | 10        | 25  | TF5YEFGB7AQT4ZXVGBEY76TGV7D2QF4HMSGAVGUJE764ZIGXS3NQ |
      | 1       | 22        | 10  | DWSC3DPKFU7TIPMOVJZM25XYBUNJQASGVDYS7QREO33UEA6FYSNA |
      | 1       | 30        | 2   | 5UWAFFNPPECDJHAYYRRLE3WIWGO2WVH4LZMPLVQBS4E76UERRATA |

  @indexer
  Scenario Outline: /transactions?max-round=<max-round>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, <max-round>, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction has round <= <max-round>

    Examples:
      | indexer | max-round | num | txid                                                 |
      | 1       | 10        | 26  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 22        | 42  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 30        | 49  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |


  @indexer
  Scenario Outline: /transactions?max-round=<max-round>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, <max-round>, 0, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction has round <= <max-round>

    Examples:
      | indexer | max-round | num | txid                                                 |
      | 1       | 10        | 26  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 22        | 42  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 30        | 49  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |

  @indexer
  Scenario Outline: /transactions?asset-id=<asset-id>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, 0, <asset-id>, "", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction works with asset-id <asset-id>

    Examples:
      | indexer | asset-id | num | txid                                                 |
      | 1       | 9        | 7   | KGG5ZGQQ57Y2ZDH5CFRYMJODPJ4TVIQBAPKT3HK3PIS6A6K4T5GQ |

  @indexer
  Scenario Outline: /transactions?before-time=<before>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, 0, 0, "<before>", "", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction is older than "<before>"

    Examples:
      | indexer | before               | num | txid                                                 |
      | 1       | 2020-03-31T19:47:49Z | 0   |                                                      |
      | 1       | 2020-03-31T19:48:49Z | 35  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 2021-03-31T19:47:49Z | 49  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |

  @indexer
  Scenario Outline: /transactions?after-time=<after>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, 0, 0, "", "<after>", 0, 0, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction is newer than "<after>"

    Examples:
      | indexer | after                | num | txid                                                 |
      | 1       | 2019-01-01T01:01:01Z | 49  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 2020-03-31T19:48:49Z | 14  | GLEDN6PCACB6WI72ABZ34CEAIXZCZQ7HLVZI7SLWIR46JHUHXQJA |
      | 1       | 2029-01-01T01:01:01Z | 0   |                                                      |

  @indexer
  Scenario Outline: /transactions?currency-gt=<currency-gt>&currency-lt=<currency-lt>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, 0, 0, "", "", <currency-gt>, <currency-lt>, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction moves between <currency-gt> and <currency-lt> currency

    Examples:
      | indexer | currency-gt | currency-lt | num | txid                                                 |
      | 1       | 0           | 10          | 2   | TK3KPYVH7CCDMGG4TP66HGQTJKA2ETCECEBEQ4QXFTLQZZR6XVMA |
      | 1       | 1           | 0           | 34  | 3LC3FNFWZVKLOSQQLKTTAPHHWKKHEVJDWBVIWAMYP7MNQQHZP5BA |
      | 1       | 10000       | 1000000     | 2   | VUOIU472GVEML5AS22TP5GSBIEITZFRZWVXVQQ7UD33QR7A5K3ZA |

  @indexer
  Scenario Outline: /transactions?asset-id=<asset-id>&currency-gt=<currency-gt>&currency-lt=<currency-lt>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, 0, <asset-id>, "", "", <currency-gt>, <currency-lt>, "", "", "" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".
    And Every transaction moves between <currency-gt> and <currency-lt> currency

    Examples:
      | indexer | asset-id | currency-gt | currency-lt | num | txid                                                 |
      | 1       | 9        | 1           | 0           | 3   | E36LBI7IN5OJGEFWKPCEQ2L436DFYHKAMEMASZXEVMP64F76HVNA |
      | 1       | 9        | 0           | 100000000   | 2   | E36LBI7IN5OJGEFWKPCEQ2L436DFYHKAMEMASZXEVMP64F76HVNA |
      | 1       | 9        | 1000000     | 100000000   | 1   | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |

  @indexer
  Scenario Outline: account filter /transactions?address=<address>&address-role=<address-role>&exclude-close-to=<exclude-close-to>
    When I use <indexer> to search for transactions with 0, "", "", "", "", 0, 0, 0, 0, "", "", 0, 0, "<address>", "<address-role>", "<exclude-close-to>" and token ""
    Then there are <num> transactions in the response, the first is "<txid>".

    Examples:
      | indexer | address                                                    | address-role | exclude-close-to | num | txid                                                 |
      | 1       | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA |              |                  | 4   | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |
      | 1       | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA | sender       |                  | 2   | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |
      | 1       | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA | receiver     |                  | 3   | 53CGXAKDKMPOXQHMFK5CFZAYP3KJLD3LMG3DFZBVIT6XR7O3ATVA |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 |              |                  | 13  | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | receiver     |                  | 6   | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | receiver     | true             | 3   | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | sender       |                  | 7   | UIMZOGVGZYQQIXIGZ3QJBL3TPP2P5T4GUMMFG5HCA7MFQSPYR6QA |

  #
  # /accounts/{account-id}/transactions - same as /transactions but the validation just ensures that all results include the specified account
  #
  @indexer
  Scenario Outline: /accounts/<account-id>/transactions
    When I use <indexer> to search for all "<account-id>" transactions
    Then there are <num> transactions in the response, the first is "<txid>".

    Examples:
      | indexer | account-id                                                 | num | txid                                                 |
      | 1       | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 | 13  | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |
      | 1       | ZBBRQD73JH5KZ7XRED6GALJYJUXOMBBP3X2Z2XFA4LATV3MUJKKMKG7SHA | 4   | 3BMTOZIYGTS3XS33MXDZO6UMNUTJOLFC3527ONBCVAHY3IMEWSUA |


  #
  # /assets/{asset-id}/transactions - same as /transactions but the validation just ensures that all results are asset xfer's for the specified asset.
  #
  @indexer
  Scenario Outline: /assets/<asset-id>/transactions
    When I use <indexer> to search for all <asset-id> asset transactions
    Then there are <num> transactions in the response, the first is "<txid>".

    Examples:
      | indexer | asset-id | num | txid                                                 |
      | 1       | 9        | 7   | KGG5ZGQQ57Y2ZDH5CFRYMJODPJ4TVIQBAPKT3HK3PIS6A6K4T5GQ |

  #
  # /transaction paging
  #
  @indexer
  Scenario Outline: /transactions?limit=<limit>&next=token
    When I use <indexer> to search for transactions with <limit>, "", "", "", "", 0, 0, <max-round>, 0, "", "", 0, 0, "", "", "" and token ""
    And I get the next page using <indexer> to search for transactions with <limit> and <max-round>
    And I get the next page using <indexer> to search for transactions with <limit> and <max-round>
    And I get the next page using <indexer> to search for transactions with <limit> and <max-round>
    And I get the next page using <indexer> to search for transactions with <limit> and <max-round>
    Then there are <num> transactions in the response, the first is "<txid>".

    Examples:
      | indexer | limit | max-round | num | txid                                                 |
      | 1       | 1     | 10        | 1   | QW2BAFU3JR7YDKDHG6BDAFJZTY7V7XBMZKHUGX2CBUG3PKHGAVMA |
      | 1       | 5     | 10        | 5   | KHTHBNR3MVPNVLGMHPDO73N6FL43WBQHPMW2PFY2LQIAQJZF77FA |

  #
  # /assets
  #
  @indexer
  Scenario Outline: /assets
    When I use <indexer> to search for assets with 0, <asset-id-in>, "<creator>", "<name>", "<unit>", and token ""
    Then there are <num> assets in the response, the first is <asset-id-out>.

    Examples:
      | indexer | asset-id-in | creator                                                    | name     | unit | num | asset-id-out |
      | 1       | 0           |                                                            |          |      | 1   | 9            |
      | 1       | 8           |                                                            |          |      | 0   | 0            |
      | 1       | 9           |                                                            |          |      | 1   | 9            |
      | 1       | 0           |                                                            | bogocoin |      | 1   | 9            |
      | 1       | 0           |                                                            | BoGoCoIn |      | 1   | 9            |
      | 1       | 0           |                                                            |   GoCo   |      | 1   | 9            |
      | 1       | 0           |                                                            |          | bogo | 1   | 9            |
      | 1       | 0           |                                                            |          | boGO | 1   | 9            |
      | 1       | 0           |                                                            |          |  oG  | 1   | 9            |
      | 1       | 0           | OSY2LBBSYJXOBAO6T5XGMGAJM77JVPQ7OLRR5J3HEPC3QWBTQZNWSEZA44 |          |      | 1   | 9            |
      | 1       | 0           |                                                            | none     |      | 0   | 9            |
  
  @indexer.applications
  Scenario Outline: /applications?id=<application-id>&limit=<limit>&next=<token>
    When I use <indexer> to search for applications with <limit>, <application-id>, and token "<token>"
    Then the parsed response should equal "<jsonfile>".

    Examples:
      | indexer | application-id | limit | token | jsonfile                                                         |
      | 2       | 22             | 0     |       | v2indexerclient_responsejsons/indexer_v2_app_search_22.json      |
      | 2       | 70             | 0     |       | v2indexerclient_responsejsons/indexer_v2_app_search_70.json      |
      | 2       | 0              | 3     |       | v2indexerclient_responsejsons/indexer_v2_app_search_limit_3.json |
      | 2       | 0              | 1     | 25    | v2indexerclient_responsejsons/indexer_v2_app_search_next_25.json |

  @indexer.applications
  Scenario Outline: /applications/<application-id>
    When I use <indexer> to lookup application with <application-id>
    Then the parsed response should equal "<jsonfile>".

    Examples:
      | indexer | application-id |  jsonfile                                                    |
      | 2       | 22             |  v2indexerclient_responsejsons/indexer_v2_app_lookup_22.json |
      | 2       | 70             |  v2indexerclient_responsejsons/indexer_v2_app_lookup_70.json |

  #
  # /transactions
  #
  @indexer.applications
  Scenario Outline: /transactions?everything
    #When I use <indexer> to search for transactions with <limit>, "<note-prefix>", "<tx-type>", "<sig-type>", "<tx-id>", <round>, <min-round>, <max-round>, <asset-id>, "<before-time>", "<after-time>", <currency-gt>, <currency-lt>, "<address>", "<address-role>", "<exclude-close-to>", <application-id> and token "<token>"
    When I use <indexer> to search for transactions with <limit>, "", "", "", "", 0, 0, 0, 0, "", "", 0, 0, "", "", "", <application-id> and token ""
    Then the parsed response should equal "<jsonfile>".

    Examples:
      | indexer | limit | application-id | jsonfile                                                             |
      | 2       | 0     | 70             | v2indexerclient_responsejsons/indexer_v2_tx_search_app_70.json       |
      | 2       | 3     | 70             | v2indexerclient_responsejsons/indexer_v2_tx_search_app_70_lim_3.json |

  @indexer.applications
  Scenario Outline: /accounts?asset-id=<asset-id>&limit=<limit>&gt=<currency-gt>&lt=<currency-lt>&auth-addr=<auth-addr>&app-id=<application-id>
    When I use <indexer> to search for an account with 0, 0, 0, 0, "", <application-id> and token ""
    Then the parsed response should equal "<jsonfile>".

    Examples:
      | indexer | application-id | jsonfile                                                         |
      | 2       | 70             | v2indexerclient_responsejsons/indexer_v2_acct_search_app_70.json |

  # Paging tests:
  #  - assets (our test dataset only has 1 asset)

  # Error/edge cases (mixed up min/max, ...?)
  #  - No results
  #  - Invalid parameters (invalid enum)
  #  - Mixed up min/max
