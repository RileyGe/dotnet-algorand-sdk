@compile
Feature: Compile
  Background:
    Given an algod v2 client

  Scenario Outline: Compile programs
    When I compile a teal program <program>
    Then it is compiled with <status> and <result> and <hash>
    Scenarios:
      | program                 | status | result     | hash |
      | "programs/one.teal"     | 200    | "AiABASI=" | "YOE6C22GHCTKAN3HU4SE5PGIPN5UKXAJTXCQUPJ3KKF5HOAH646MKKCPDA" |
      | "programs/invalid.teal" | 400    | ""         | "" |
