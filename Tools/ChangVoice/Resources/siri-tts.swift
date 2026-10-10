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

    let synthesizer = AVSpeechSynthesizer()
    for (index, item) in job.items.enumerated() {
        let utterance = AVSpeechUtterance(string: item.text)
        utterance.voice = voice
        utterance.rate = job.rate
        utterance.preUtteranceDelay = 0
        utterance.postUtteranceDelay = 0

        var file: AVAudioFile?
        var done = false
        var failure: String?
        synthesizer.write(utterance) { buffer in
            guard let pcm = buffer as? AVAudioPCMBuffer, pcm.frameLength > 0 else { done = true; return }
            do {
                if file == nil {
                    file = try AVAudioFile(forWriting: URL(fileURLWithPath: item.out), settings: pcm.format.settings,
                                           commonFormat: pcm.format.commonFormat, interleaved: pcm.format.isInterleaved)
                }
                try file?.write(from: pcm)
            } catch {
                failure = error.localizedDescription
                done = true
            }
        }
        while !done { RunLoop.current.run(until: Date().addingTimeInterval(0.01)) }
        file = nil
        if let failure { fail("\(item.out): \(failure)") }
        say("DONE \(index + 1) \(job.items.count)")
    }

default:
    fail("usage: swift siri-tts.swift --list-json | --render <job.json>")
}
