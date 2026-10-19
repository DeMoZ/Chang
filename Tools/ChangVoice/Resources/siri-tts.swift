// Speaks word lists with the macOS voices, Siri voices included, for the Chang Voice app.
//
// macOS only gives Siri voices to programs signed by Apple. The Swift interpreter from Xcode is, so the app
// runs this script with it (`xcrun swift siri-tts.swift …`), like the SubDub app does.
//
// usage: swift siri-tts.swift --list-json             voices as JSON: [{id, name, language, gender, siri}]
//        swift siri-tts.swift --render <job.json>     job: {voice, rate, items: [{text, out}]}, writes each item
//                                                     as a .caf file and prints "DONE <i> <n>" after each one
import AVFoundation

func fail(_ message: String) -> Never {
    FileHandle.standardError.write(Data("error: \(message)\n".utf8))
    exit(1)
}

func say(_ line: String) {
    print(line)
    fflush(stdout)
}

let arguments = CommandLine.arguments.dropFirst()

switch arguments.first {
case "--list-json":
    let voices = AVSpeechSynthesisVoice.speechVoices().map { v -> [String: Any] in
        [
            "id": v.identifier,
            "name": v.name,
            "language": v.language,
            "gender": v.gender == .male ? "Male" : v.gender == .female ? "Female" : "",
            "siri": v.identifier.contains("gryphon") || v.identifier.contains(".siri."),
        ]
    }
    let data = try! JSONSerialization.data(withJSONObject: voices)
    say(String(decoding: data, as: UTF8.self))

case "--render":
    struct Item: Decodable { var text: String; var out: String }
    struct Job: Decodable { var voice: String; var rate: Float; var items: [Item] }

    guard let path = arguments.dropFirst().first,
          let job = try? JSONDecoder().decode(Job.self, from: Data(contentsOf: URL(fileURLWithPath: path))) else {
        fail("no job file")
    }
    guard let voice = AVSpeechSynthesisVoice(identifier: job.voice) else { fail("voice not found: \(job.voice)") }

    /// Speaks one item into its file; returns the seconds of audio written.
    /// A new synthesizer per item: a shared one sometimes delivers the end of an utterance into the next one.
    func render(_ item: Item) -> Double {
        let synthesizer = AVSpeechSynthesizer()
        let utterance = AVSpeechUtterance(string: item.text)
        utterance.voice = voice
        utterance.rate = job.rate
        utterance.preUtteranceDelay = 0
        utterance.postUtteranceDelay = 0

        var file: AVAudioFile?
        var seconds = 0.0
        var ended = false
        var lastBuffer = Date()
        var failure: String?
        synthesizer.write(utterance) { buffer in
            lastBuffer = Date()
            // an empty buffer marks the end, but more audio of the same utterance may still follow it
            guard let pcm = buffer as? AVAudioPCMBuffer, pcm.frameLength > 0 else { ended = true; return }
            do {
                if file == nil {
                    file = try AVAudioFile(forWriting: URL(fileURLWithPath: item.out), settings: pcm.format.settings,
                                           commonFormat: pcm.format.commonFormat, interleaved: pcm.format.isInterleaved)
                }
                try file?.write(from: pcm)
                seconds += Double(pcm.frameLength) / pcm.format.sampleRate
            } catch {
                failure = error.localizedDescription
                ended = true
            }
        }
        // the utterance is over when the end was marked and no buffer came for a while
        let started = Date()
        while !(ended && Date().timeIntervalSince(lastBuffer) > 0.3) && failure == nil {
            RunLoop.current.run(until: Date().addingTimeInterval(0.01))
            if Date().timeIntervalSince(started) > 30 {
                synthesizer.stopSpeaking(at: .immediate)
                failure = "the speech did not end in 30 s"
            }
        }
        file = nil
        if let failure { fail("\(item.out): \(failure)") }
        return seconds
    }

    // the synthesizer sometimes ends an utterance after a few milliseconds of audio: speak it again
    let minSeconds = 0.1
    let attempts = 3
    for (index, item) in job.items.enumerated() {
        var seconds = 0.0
        for _ in 1...attempts {
            try? FileManager.default.removeItem(atPath: item.out)
            seconds = render(item)
            if seconds >= minSeconds { break }
        }
        if seconds < minSeconds { fail("\(item.out): only \(seconds) s of audio after \(attempts) attempts") }
        say("DONE \(index + 1) \(job.items.count)")
    }

default:
    fail("usage: swift siri-tts.swift --list-json | --render <job.json>")
}
