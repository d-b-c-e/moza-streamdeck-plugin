# Troubleshooting

## Plugin Won't Load / Crashes on Startup

**Symptoms:** Plugin doesn't appear in Stream Deck, or shows briefly then disappears. Stream Deck logs show exit code `0xC0000005`.

**Cause:** Prior to v1.0.3, the plugin required .NET 8.0 Desktop Runtime to be installed separately. Missing runtime caused an access violation crash with no plugin-side log output.

**Fix:** Update to v1.0.3 or later. The plugin is now self-contained and bundles the .NET runtime — no separate install needed.

**Verify:** Check the Stream Deck application log for plugin load errors:
```
%APPDATA%\Elgato\StreamDeck\logs\StreamDeck.log
```

---

## All Actions Show "N/C"

**Symptoms:** Every Moza action on your Stream Deck shows "N/C" (Not Connected).

**Cause:** The plugin defers SDK initialization until the first button press or dial rotation. Until you interact with a Moza action, the display shows "N/C".

**Fix:** Press any Moza action button or rotate any Moza dial. All actions will refresh and show current values within a few seconds.

**If "N/C" persists after pressing a button:**
1. Ensure **Moza Pit House** is installed (the SDK requires it)
2. Ensure your wheel base is powered on and connected via USB
3. Check the plugin log for SDK errors:
   ```
   %APPDATA%\Elgato\StreamDeck\Plugins\com.dbce.moza-streamdeck.sdPlugin\pluginlog.log
   ```

---

## Actions Show "Error" After Button Press

**Symptoms:** Pressing a button briefly shows "Error" then reverts.

**Cause:** The Moza SDK returned an error for that operation. Common reasons:
- Wheel base is powered off or disconnected
- Pit House is not installed
- Another application has exclusive control of the device

**Fix:**
1. Ensure your wheel base is on and connected
2. Close Pit House if it's open (some operations conflict)
3. Try the button again — some errors are transient during SDK initialization

---

## Apply Preset Shows "(0, X skip)"

**Symptoms:** Applying a preset shows a count like "(3, 2 skip)" instead of all settings applied.

**Cause:** Some preset parameters are not supported by the SDK, or the preset contains values outside the valid range. The plugin applies what it can and skips the rest.

**Skipped parameters are expected** for these preset fields (no SDK support):
- `gameForceFeedbackFilter`
- `setGameDampingValue`, `setGameFrictionValue`, `setGameInertiaValue`, `setGameSpringValue`
- `softLimitStrength`, `softLimitStiffness`
- `constForceExtraMode`, `forceFeedbackMaping`
- `naturalInertiaV2`, `gearJoltLevel`

**Also:** `initialSpeedDependentDamping` (speed damping start point) is intentionally skipped when the preset value is 0, because the SDK rejects 0 for this parameter.

**Check detailed errors in the plugin log:**
```
%APPDATA%\Elgato\StreamDeck\Plugins\com.dbce.moza-streamdeck.sdPlugin\pluginlog.log
```

---

## Preset Dropdown Is Empty

**Symptoms:** The "Apply Preset" action's Property Inspector shows no presets in the dropdown.

**Cause:** The plugin looks for Pit House motor presets in these locations (checked in order):
1. `Documents\MOZA Pit House\Presets\Motor\`
2. `OneDrive\Documents\MOZA Pit House\Presets\Motor\`
3. `%USERPROFILE%\Documents\MOZA Pit House\Presets\Motor\`

**Fix:**
1. Open Moza Pit House and create at least one motor preset (or confirm existing presets exist)
2. Check that the `Presets\Motor\` folder exists and contains `.json` files
3. If your Documents folder is redirected (e.g., to OneDrive), ensure the path matches one of the candidates above
4. Presets with empty names are filtered out — ensure each preset has a name in Pit House

> **Note:** Only Motor presets appear. Steering Wheel presets (button mappings, RPM LEDs) and Pedal presets (pedal curves) are not shown because they contain settings that can't be applied via the SDK.

---

## Pit House Launches When Stream Deck Starts

**Symptoms:** Moza Pit House opens automatically every time Stream Deck starts.

**Cause:** This was fixed in v1.0.2. Prior versions initialized the Moza SDK immediately at plugin startup, which triggered Pit House to launch.

**Fix:** Update to v1.0.2 or later. The plugin now defers SDK initialization until the first user interaction (button press or dial rotation).

---

## Wheel Rotation Shows Wrong Value

**Symptoms:** The Wheel Rotation action displays a value that doesn't match Pit House.

**Cause:** The Moza SDK has separate "hardware" and "game" steering angle limits. The plugin displays and adjusts the game limit. If a preset sets different values for each, the displayed value may differ from what Pit House shows (which may show the hardware limit).

**Fix:** This is expected behavior. Use the "Set Rotation" action to set a specific value, or use "Apply Preset" to synchronize both limits from a preset.

---

## Log File Locations

| Log | Location |
|-----|----------|
| Plugin log | `%APPDATA%\Elgato\StreamDeck\Plugins\com.dbce.moza-streamdeck.sdPlugin\pluginlog.log` |
| Stream Deck app log | `%APPDATA%\Elgato\StreamDeck\logs\StreamDeck.log` |

To access these quickly, paste the path into Windows Explorer's address bar or Run dialog (Win+R).

---

## Reporting Issues

If your issue isn't covered above:

1. Reproduce the problem
2. Collect both log files listed above
3. Note your plugin version, Stream Deck software version, and wheel base model
4. Open an issue at the [GitHub repository](https://github.com/d-b-c-e/moza-streamdeck-plugin/issues)
