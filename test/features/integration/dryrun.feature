@dryrun
Feature: Dryrun
  Background:
    Given an algod v2 client

  Scenario Outline: Dryrun execution with binary and source programs
    When I dryrun a <kind> program <program>
    Then I get execution result <result>
    Scenarios:
      | program                  | kind       | result   |
      | "programs/one.teal.tok"  | "compiled" | "PASS"   |
      | "programs/zero.teal.tok" | "compiled" | "REJECT" |
      | "programs/one.teal"      | "source"   | "PASS"   |
      | "programs/zero.teal"     | "source"   | "REJECT" |
