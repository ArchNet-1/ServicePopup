<p align="center">
  <a href="https://archnet-plateform.fr"><img src="https://archnet-plateform.fr/ArchNetLogoWithBck.png" alt="ArchNet" width="240"></a>
</p>

# ArchNet · Popup Service

Alerts, yes / no questions and text prompts you `await` from code. One popup at a time: the others wait in line and open in order.

- Unity **6000.0** or newer · Needs `com.unity.ugui` and [ArchNet Core](https://github.com/ArchNet-1/ArchNetCore)
- Part of [ArchNet](https://archnet-plateform.fr), free and open-source Unity packages

## Add it to your game (one click)

With the [ArchNet Hub](https://github.com/ArchNet-1/ArchNetHub): *ArchNet > Hub > Services > Popups > Add to game*. The Hub installs what is needed and creates, in `Assets/ArchNet/Popups/`:

| File | What it is |
|---|---|
| `Scripts/GamePopups.cs` | Your class: put the questions your game asks often there (`ConfirmQuit()` is an example). |
| `Prefabs/Popups.prefab` | The popup screen (shade, window, title, message, text field, two buttons). Restyle it freely. It is placed under the ArchNet Bootstrap, so popups open from any scene. |

## Use

```csharp
var popups = Services.Get<GamePopups>();

await popups.Alert("Saved", "Your progress is safe.");

if (await popups.Confirm("Quit", "Leave the game?", "Quit", "Stay"))
    Application.Quit();

string name = await popups.Prompt("New save", "Name your save:", "My adventure");
if (name != null) Save(name); // null when cancelled
```

Call them from an `async` method (an `async void` button handler is fine). Enter in the text field confirms. `IsOpen` and `Waiting` tell what is on screen and in line. When the service is destroyed, open and waiting popups answer as cancelled.

**Open tool** (same card) opens the Popups window: open the prefab, and try each kind of popup in Play Mode.

## License

MIT, see [LICENSE.md](LICENSE.md).
