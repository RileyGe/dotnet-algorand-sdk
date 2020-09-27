@unit.rekey
@unit
Feature: Rekey
  Scenario Outline: Create and sign a rekeying transaction
    Given payment transaction parameters <fee> <fv> <lv> "<gh>" "<to>" "<close>" <amt> "<gen>" "<note>"
    And mnemonic for private key "<mn>"
    When I create the flat fee payment transaction
    And I add a rekeyTo field with address "<rekeyTo>"
    And I sign the transaction with the private key
    Then the signed transaction should equal the golden "<golden>"

    Examples:
      | fee  | fv      | lv      | gh                                                     | to                                                         | close                                                      | amt  | gen          | note         | mn                                                                                                                                                                   | golden  | rekeyTo |
      | 1000 | 6513047 | 6514047 | wGHE2Pwdvd7S12BL5FaOP20EGYesN73ktiC1qzkkit8= | PGSKI4MR4UBLWUEUGI3MALSKHCM3VZEHV7D7IOYR5DIH3GKTWR46CGLACA | PGSKI4MR4UBLWUEUGI3MALSKHCM3VZEHV7D7IOYR5DIH3GKTWR46CGLACA | 1000 | mainnet-v1.0 | ChlGdKMXTs4= | cute elevator romance type flight broccoli hub engage hundred brick add cage crouch turtle cake service heart cube like hidden dizzy lonely include abandon oven     |  gqNzaWfEQNiXcIWUgBqo7owprXZ8DQzga5ws8ZOWaClqTNqixtIiHjqahZg0qiHcFdcqxFXzbH/Org1yLWihtR5fWrmEgAijdHhujKNhbXTNA+ilY2xvc2XEIHmkpHGR5QK7UJQyNsAuSjiZuuSHr8f0OxHo0H2ZU7R5o2ZlZc0D6KJmds4AY2GXo2dlbqxtYWlubmV0LXYxLjCiZ2jEIMBhxNj8Hb3e0tdgS+RWjj9tBBmHrDe95LYgtas5JIrfomx2zgBjZX+kbm90ZcQIChlGdKMXTs6jcmN2xCB5pKRxkeUCu1CUMjbALko4mbrkh6/H9DsR6NB9mVO0eaVyZWtlecQgAAh70b1fGJ9jrtxQg8p+Dk6dRMxB4KgrStW0KRxADXajc25kxCAACHvRvV8Yn2Ou3FCDyn4OTp1EzEHgqCtK1bQpHEANdqR0eXBlo3BheQ== | AAEHXUN5L4MJ6Y5O3RIIHST6BZHJ2RGMIHQKQK2K2W2CSHCABV3MFUFBGA |

  Scenario Outline: Create and sign transaction using a rekey-ed account
    Given payment transaction parameters <fee> <fv> <lv> "<gh>" "<to>" "<close>" <amt> "<gen>" "<note>"
    And mnemonic for private key "<mn>"
    When I create the flat fee payment transaction
    And I set the from address to "<fromAddress>"
    And I sign the transaction with the private key
    Then the signed transaction should equal the golden "<golden>"

    Examples:
      | fee  | fv      | lv      | gh                                                     | to                                                         | close                                                      | amt  | gen          | note         | mn                                                                                                                                                                   | golden  | fromAddress |
      | 1000 | 6523851 | 6524851 | wGHE2Pwdvd7S12BL5FaOP20EGYesN73ktiC1qzkkit8= | PGSKI4MR4UBLWUEUGI3MALSKHCM3VZEHV7D7IOYR5DIH3GKTWR46CGLACA | PGSKI4MR4UBLWUEUGI3MALSKHCM3VZEHV7D7IOYR5DIH3GKTWR46CGLACA | 1000 | mainnet-v1.0 | Pr/BIa6bbgo= | cute elevator romance type flight broccoli hub engage hundred brick add cage crouch turtle cake service heart cube like hidden dizzy lonely include abandon oven     |  g6RzZ25yxCAACHvRvV8Yn2Ou3FCDyn4OTp1EzEHgqCtK1bQpHEANdqNzaWfEQHOKZiZw33wKbcEYVC2q2p1rNQygF5iv98zCJDIvblbhaP+4C+b31/5yYi9hlCA9ZAr6csol+3y3/Yn7qMX+8QKjdHhui6NhbXTNA+ilY2xvc2XEIHmkpHGR5QK7UJQyNsAuSjiZuuSHr8f0OxHo0H2ZU7R5o2ZlZc0D6KJmds4AY4vLo2dlbqxtYWlubmV0LXYxLjCiZ2jEIMBhxNj8Hb3e0tdgS+RWjj9tBBmHrDe95LYgtas5JIrfomx2zgBjj7Okbm90ZcQIPr/BIa6bbgqjcmN2xCB5pKRxkeUCu1CUMjbALko4mbrkh6/H9DsR6NB9mVO0eaNzbmTEICzNBpDQeH70lV5lkLAuexyXDUji1uQ/x22ZvmHPLTxipHR5cGWjcGF5 | FTGQNEGQPB7PJFK6MWILALT3DSLQ2SHC23SD7R3NTG7GDTZNHRRDXVGL3Y |


