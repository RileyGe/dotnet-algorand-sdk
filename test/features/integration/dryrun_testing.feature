@dryrun.testing
Feature: Dryrun Testing
  Background:
    Given an algod v2 client

  Scenario Outline: Dryrun test case with simple assert
    Given dryrun test case with <program> of type <kind>
    Then status assert of <status> is succeed
    Scenarios:
      | program                  | kind       | status   |
      | "programs/one.teal.tok"  | "lsig"     | "PASS"   |
      | "programs/one.teal.tok"  | "approv"   | "PASS"   |
      | "programs/one.teal.tok"  | "clearp"   | "PASS"   |
      | "programs/zero.teal.tok" | "lsig"     | "REJECT" |
      | "programs/zero.teal.tok" | "approv"   | "REJECT" |
      | "programs/zero.teal.tok" | "clearp"   | "REJECT" |
      | "programs/one.teal"      | "lsig"     | "PASS"   |
      | "programs/one.teal"      | "approv"   | "PASS"   |
      | "programs/one.teal"      | "clearp"   | "PASS"   |
      | "programs/zero.teal"     | "lsig"     | "REJECT" |
      | "programs/zero.teal"     | "approv"   | "REJECT" |
      | "programs/zero.teal"     | "clearp"   | "REJECT" |

  Scenario Outline: Dryrun test case with global state delta assert succeed
    Given dryrun test case with <program> of type <kind>
    Then global delta assert with <key>, <value> and <action> is succeed
    Scenarios:
      | program                     | kind     | key            | action | value      |
      | "programs/globalwrite.teal" | "approv" | "Ynl0ZXNrZXk=" | 1      | "dGVzdA==" |
      | "programs/globalwrite.teal" | "approv" | "aW50a2V5"     | 2      | "11"       |
      | "programs/globalwrite.teal" | "clearp" | "Ynl0ZXNrZXk=" | 1      | "dGVzdA==" |
      | "programs/globalwrite.teal" | "clearp" | "aW50a2V5"     | 2      | "11"       |

  Scenario Outline: Dryrun test case with global state delta assert failed
    Given dryrun test case with <program> of type <kind>
    Then global delta assert with <key>, <value> and <action> is failed
    Scenarios:
      | program                     | kind     | key            | action | value  |
      | "programs/globalwrite.teal" | "clearp" | "Ynl0ZXNrZXk=" | 1      | "test" |
      | "programs/globalwrite.teal" | "approv" | "aW50a2V5"     | 2      | "12"   |

  Scenario Outline: Dryrun test case with local state delta assert succeed
    Given dryrun test case with <program> of type <kind>
    Then local delta assert for <addr> of accounts <index> with <key>, <value> and <action> is succeed
    Scenarios:
      | program                    | kind     | addr                                                         | index | key            | action | value      |
      | "programs/localwrite.teal" | "approv" | "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAY5HFKQ" | 0     | "Ynl0ZXNrZXk=" | 1      | "dGVzdA==" |
      | "programs/localwrite.teal" | "approv" | "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAY5HFKQ" | 0     | "aW50a2V5"     | 2      | "11"       |
      | "programs/localwrite.teal" | "clearp" | "6Z3C3LDVWGMX23BMSYMANACQOSINPFIRF77H7N3AWJZYV6OH6GWTJKVMXY" | 1     | "Ynl0ZXNrZXk=" | 1      | "dGVzdA==" |
      | "programs/localwrite.teal" | "clearp" | "6Z3C3LDVWGMX23BMSYMANACQOSINPFIRF77H7N3AWJZYV6OH6GWTJKVMXY" | 1     | "aW50a2V5"     | 2      | "11"       |
