@rekey
Feature: Sending transactions
  Background:
    Given an algod client
    And a kmd client
    And wallet information

  Scenario Outline: Sending transactions
    Given default transaction with parameters <amt> "<note>"
    When I get the private key
    And I add a rekeyTo field with address "<rekeyTo>"
    And I sign the transaction with the private key
    And I send the transaction
    Then the transaction should go through
    Given default transaction with parameters <amt> "<note>"
    And mnemonic for private key "<mn>"
    And I sign the transaction with the private key
    And I send the transaction
    Then the transaction should go through
    Given default transaction with parameters <amt> "<note>"
    When I get the private key
    And I add a rekeyTo field with the private key algorand address
    And mnemonic for private key "<mn>"
    And I sign the transaction with the private key
    And I send the transaction
    Then the transaction should go through

    Examples:
      | amt | note | rekeyTo | mn |
      | 0   | X4Bl4wQ9rCo= | AAEHXUN5L4MJ6Y5O3RIIHST6BZHJ2RGMIHQKQK2K2W2CSHCABV3MFUFBGA | cute elevator romance type flight broccoli hub engage hundred brick add cage crouch turtle cake service heart cube like hidden dizzy lonely include abandon oven   |


