## Legacy IR Driver Bridging

```json
{
	"id": "1",
	"name": "Apple TV",
	"key": "appleTv-1",
	"type": "genericIrController",
	"uid": 3,
	"group": "devices",
	"properties": {
		"control": {
			"method": "ir",
			"irFile": "Apple_AppleTV_4th_Gen_Essentials.ir",
			"controlPortDevKey": "processor",
			"controlPortNumber": "1"
		}
	}
}
```

## Bridge Join Map IR Driver Bridging

```json
{
	"id": "1",
	"name": "Apple TV",
	"key": "appleTv-1",
	"type": "genericIrController",
	"uid": 3,
	"group": "devices",
	"properties": {
		"control": {
			"method": "ir",
			"irFile": "Apple_AppleTV_4th_Gen_Essentials.ir",
			"controlPortDevKey": "processor",
			"controlPortNumber": "1",
			"useBridgeJoinMap": true
		}
	}
}
```

Both methods will bridge the IR signals with `Standard Command` defined in the IR file.  

The `useBridgeJoinMap` property implements `GenericIrControllerJoinMap.cs` to standardized IR driver `Standard Command` signal joins.  This allows users to swap IR drivers that implement `Standard Command` while bridging IR signals consistently between drivers.  For example, when `useBridgeJoinMap` is present, channel up will be mapped to join-22 + device `joinstart` for any IR driver that has the signal marked as `Standard Command`.


## GenericIrControllerJoinMap (Example)

### Digitals

| Join Number | Join Span | Description | Type                | Capabilities |
| ----------- | --------- | ----------- | ------------------- | ------------ |
| 1           | 1         | PLAY        | Digital             | FromSIMPL    |
| 2           | 1         | STOP        | Digital             | FromSIMPL    |
| 3           | 1         | PAUSE       | Digital             | FromSIMPL    |
| 4           | 1         | FSCAN       | Digital             | FromSIMPL    |
| 5           | 1         | RSCAN       | Digital             | FromSIMPL    |
| 9           | 1         | POWER       | Digital             | FromSIMPL    |
| 10          | 1         | 0           | Digital             | FromSIMPL    |
| 11          | 1         | 1           | Digital             | FromSIMPL    |
| 12          | 1         | 2           | Digital             | FromSIMPL    |
| 13          | 1         | 3           | Digital             | FromSIMPL    |
| 14          | 1         | 4           | Digital             | FromSIMPL    |
| 15          | 1         | 5           | Digital             | FromSIMPL    |
| 16          | 1         | 6           | Digital             | FromSIMPL    |
| 17          | 1         | 7           | Digital             | FromSIMPL    |
| 18          | 1         | 8           | Digital             | FromSIMPL    |
| 19          | 1         | 9           | Digital             | FromSIMPL    |
| 21          | 1         | ENTER       | Digital             | FromSIMPL    |
| 22          | 1         | CH+         | Digital             | FromSIMPL    |
| 23          | 1         | CH-         | Digital             | FromSIMPL    |
| 27          | 1         | POWER_ON    | Digital             | FromSIMPL    |
| 28          | 1         | POWER_OFF   | Digital             | FromSIMPL    |
| 30          | 1         | LAST        | Digital             | FromSIMPL    |
| 41          | 1         | BACK        | Digital             | FromSIMPL    |
| 42          | 1         | GUIDE       | Digital             | FromSIMPL    |
| 43          | 1         | INFO        | Digital             | FromSIMPL    |
| 44          | 1         | MENU        | Digital             | FromSIMPL    |
| 45          | 1         | UP_ARROW    | Digital             | FromSIMPL    |
| 46          | 1         | DN_ARROW    | Digital             | FromSIMPL    |
| 47          | 1         | LEFT_ARROW  | Digital             | FromSIMPL    |
| 48          | 1         | RIGHT_ARROW | Digital             | FromSIMPL    |
| 49          | 1         | SELECT      | Digital             | FromSIMPL    |
| 54          | 1         | PAGE_UP     | Digital             | FromSIMPL    |
| 55          | 1         | PAGE_DOWN   | Digital             | FromSIMPL    |
| 61          | 1         | A           | Digital             | FromSIMPL    |
| 62          | 1         | B           | Digital             | FromSIMPL    |
| 63          | 1         | C           | Digital             | FromSIMPL    |
| 64          | 1         | D           | Digital             | FromSIMPL    |

### Analogs

| Join Number | Join Span | Description | Type                | Capabilities |
| ----------- | --------- | ----------- | ------------------- | ------------ |

### Serials

| Join Number | Join Span | Description | Type                | Capabilities |
| ----------- | --------- | ----------- | ------------------- | ------------ |


