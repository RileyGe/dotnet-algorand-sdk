@assets
Feature: Assets
  Background:
    Given an algod client
    And a kmd client
    And wallet information
    And asset test fixture

  Scenario Outline: Asset creation
    Given default asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    And I get the asset info
    Then the asset info should match the expected asset info

    Examples:
      | total |
      | 1 |

  Scenario Outline: Asset reconfigure
    Given default asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    And I get the asset info
    Then the asset info should match the expected asset info
    When I create a no-managers asset reconfigure transaction
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I get the asset info
    Then the asset info should match the expected asset info

    Examples:
      | total |
      | 1 |

  Scenario Outline: Asset destroy
    Given default asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    And I get the asset info
    And I create an asset destroy transaction
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    And I should be unable to get the asset info

    Examples:
      | total |
      | 1 |

  Scenario Outline: Asset acceptance
    Given default asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    And I create a transaction for a second account, signalling asset acceptance
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction transferring <amount> assets from creator to a second account
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    And the creator should have <expected balance> assets remaining

    Examples:
      | total | amount | expected balance |
      | 100   | 50     | 50               |

  Scenario Outline: Asset non-acceptance
    Given default asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    When I create a transaction transferring <amount> assets from creator to a second account
    And I sign the transaction with kmd
    And I send the bogus kmd-signed transaction
    Then the transaction should not go through
    And the creator should have <expected balance> assets remaining

    Examples:
      | total | amount | expected balance  |
      | 100   | 50     | 100               |

  Scenario Outline: Asset freeze and unfreeze
    Given default asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    And I create a transaction for a second account, signalling asset acceptance
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction transferring <amount> assets from creator to a second account
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    And the creator should have <expected balance> assets remaining
    When I create a freeze transaction targeting the second account
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction transferring <amount> assets from creator to a second account
    And I sign the transaction with kmd
    And I send the bogus kmd-signed transaction
    Then the transaction should not go through
    And the creator should have <expected balance> assets remaining
    When I create a transaction transferring <amount> assets from a second account to creator
    And I renew the wallet handle
    And I sign the transaction with kmd
    And I send the bogus kmd-signed transaction
    Then the transaction should not go through
    And the creator should have <expected balance> assets remaining
    When I create an un-freeze transaction targeting the second account
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction transferring <amount> assets from a second account to creator
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    And the creator should have <final expected balance> assets remaining

    Examples:
      | total | amount | expected balance  | final expected balance |
      | 100   | 50     | 50                | 100                    |

  Scenario Outline: Frozen by default
    Given default-frozen asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    When I create a transaction for a second account, signalling asset acceptance
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction transferring <amount> assets from creator to a second account
    And I send the bogus kmd-signed transaction
    Then the transaction should not go through
    And the creator should have <expected balance> assets remaining
    When I create an un-freeze transaction targeting the second account
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction transferring <amount> assets from creator to a second account
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    And the creator should have <final expected balance> assets remaining

    Examples:
      | total | amount | expected balance   | final expected balance |
      | 100   | 50     | 100                | 50                     |

  Scenario Outline: Asset revocation
    Given default asset creation transaction with total issuance <total>
    When I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I update the asset index
    And I create a transaction for a second account, signalling asset acceptance
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction transferring <amount> assets from creator to a second account
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    When I create a transaction revoking <amount> assets from a second account to creator
    And I sign the transaction with kmd
    And I send the kmd-signed transaction
    Then the transaction should go through
    And the creator should have <expected balance> assets remaining

    Examples:
      | total | amount | expected balance  |
      | 100   | 50     | 100               |