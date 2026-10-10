# Chang Voice

> **Not used.** The word sounds are voiced in Unity: **Chang → Utilities → Word Sounds (Siri)**
> (`WordSoundsConfig`, `Assets/Project/Configs/WordSounds.asset`). The desktop app in `Sources/` is kept for reference only.

**Still used:** `Resources/siri-tts.swift`, the helper both tools speak through. macOS only gives Siri voices to
Apple-signed programs; the Swift interpreter from Xcode is one, so the helper runs as `xcrun swift siri-tts.swift …`:

- `--list-json` lists the voices: `[{id, name, language, gender, siri}]`
- `--render <job.json>` speaks `{voice, rate, items: [{text, out}]}` into `.caf` files and prints `DONE <i> <n>` after each

## The desktop app (unused)

A SwiftUI app with the same features as the Unity inspector, built the way the SubDub app is (no Xcode project):

```
Tools/ChangVoice/build.sh open
```

It reads the words from `Assets/Project/Resources_Bundled/BookConfigs/<Language>/Vocabulary.asset` and writes
`SoundWords/<Language>/<Voice>/<Section>/<Key>.mp3` (slow: `SoundWordsSlow/…`) with ffmpeg. Needs Xcode and ffmpeg.
