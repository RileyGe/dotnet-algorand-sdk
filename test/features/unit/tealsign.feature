@unit.tealsign
@unit
Feature: Tealsign
  Scenario Outline: Tealsign data using contract address and mnemonic
    Given base64 encoded data to sign "<data>"
    And program hash "<hash>"
    And mnemonic for private key "<mn>"
    When I perform tealsign
    Then the signature should be equal to "<golden>"

    Examples:
      | data                 | hash                                                       | mn                                                                                                                                                             | golden                                                                                   |
      | Ux8jntyBJQarjKGF8A== | 6Z3C3LDVWGMX23BMSYMANACQOSINPFIRF77H7N3AWJZYV6OH6GWTJKVMXY | wish used round alert sock survey mind dilemma giraffe usage hockey thing rifle trim swim fault sort denial phrase century hundred shoulder chat absent nation | lKvoxpnatNLbjJs5iGWwXK144AYlIpEVe+S7A8VcKVDMktGlhxJD2M0kz2cFTeN9xnna4DsSCPCS5hHmV2qXDA== |

  Scenario Outline: Tealsign data using program bytes and mnemonic
    Given base64 encoded data to sign "<data>"
    And base64 encoded program "<program>"
    And mnemonic for private key "<mn>"
    When I perform tealsign
    Then the signature should be equal to "<golden>"

    Examples:
      | data                 | program  | mn                                                                                                                                                             | golden                                                                                   |
      | Ux8jntyBJQarjKGF8A== | ASABASI= | wish used round alert sock survey mind dilemma giraffe usage hockey thing rifle trim swim fault sort denial phrase century hundred shoulder chat absent nation | lKvoxpnatNLbjJs5iGWwXK144AYlIpEVe+S7A8VcKVDMktGlhxJD2M0kz2cFTeN9xnna4DsSCPCS5hHmV2qXDA== |

  Scenario Outline: Tealsign data using program bytes and private key bytes
    Given base64 encoded data to sign "<data>"
    And base64 encoded program "<program>"
    And base64 encoded private key "<key>"
    When I perform tealsign
    Then the signature should be equal to "<golden>"

    Examples:
      | data                 | program  | key                                          | golden                                                                                   |
      | Ux8jntyBJQarjKGF8A== | ASABASI= | 5Pf7eGMA52qfMT4R4/vYCt7con/7U3yejkdXkrcb26Q= | lKvoxpnatNLbjJs5iGWwXK144AYlIpEVe+S7A8VcKVDMktGlhxJD2M0kz2cFTeN9xnna4DsSCPCS5hHmV2qXDA== |

  Scenario Outline: Tealsign data using contract address and private key bytes
    Given base64 encoded data to sign "<data>"
    And program hash "<hash>"
    And base64 encoded private key "<key>"
    When I perform tealsign
    Then the signature should be equal to "<golden>"

    Examples:
      | data                 | hash                                                       | key                                          | golden                                                                                   |
      | Ux8jntyBJQarjKGF8A== | 6Z3C3LDVWGMX23BMSYMANACQOSINPFIRF77H7N3AWJZYV6OH6GWTJKVMXY | 5Pf7eGMA52qfMT4R4/vYCt7con/7U3yejkdXkrcb26Q= | lKvoxpnatNLbjJs5iGWwXK144AYlIpEVe+S7A8VcKVDMktGlhxJD2M0kz2cFTeN9xnna4DsSCPCS5hHmV2qXDA== |
