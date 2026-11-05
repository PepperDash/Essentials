# Card Frame

Essentials v3 has no built-in card frame device. The `internalcardcage` type is provided by the [epi-crestron-io](https://github.com/PepperDash/epi-crestron-io) plugin, which supports Essentials v3.

```json
        {
                "key": "cardCage1",
                "uid": 1,
                "name": "Internal Card Cage",
                "type": "internalcardcage",
                "group": "cardCage",
                "properties": {
                  "cards": {
                    "1": "c3com3",
                    "2": "c3com3",
                    "3": ""
                  }
                }
            },
```
