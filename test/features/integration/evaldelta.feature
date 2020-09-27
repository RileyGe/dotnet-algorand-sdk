Feature: EvalDelta
  # These are optional for the SDKs to implement because
  # their primary purpose is testing algod / indexer

   Background:
      Given a kmd client
      And wallet information
      And an algod v2 client connected to "localhost" port 60000 with token "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
      And I create a new transient account and fund it with 100000000 microalgos.
      And indexer client 3 at "localhost" port 60002 with token "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

  @applications.evaldelta
  Scenario Outline:Set '<arg>' in <state-location> state
      # Create app
      Given I build an application transaction with the transient account, the current application, suggested params, operation "create_optin", approval-program "<program>", clear-program "programs/one.teal.tok", global-bytes <global-bytes>, global-ints <global-ints>, local-bytes <local-bytes>, local-ints <local-ints>, app-args "<arg>", foreign-apps "", foreign-assets "", app-accounts ""
      And I sign and submit the transaction, saving the txid. If there is an error it is "".
      Then the unconfirmed pending transaction by ID should have no apply data fields.
      And I wait for the transaction to be confirmed.
      Then the confirmed pending transaction by ID should have a "<state-location>" state change for "Zm9v" to "<value>", indexer 3 should also confirm this.

    Examples:
      | program                         | state-location | global-bytes | global-ints | local-bytes | local-ints | arg       | value    |
      | programs/globwrite.teal.tok     | global         | 1            | 0           | 0           | 0          | str:hello | aGVsbG8= |
      | programs/globwrite_int.teal.tok | global         | 0            | 1           | 0           | 0          | int:90000 | 90000    |
      | programs/locwrite.teal.tok      | local          | 0            | 0           | 1           | 0          | str:hello | aGVsbG8= |
      | programs/locwrite_int.teal.tok  | local          | 0            | 0           | 0           | 1          | int:90000 | 90000    |
