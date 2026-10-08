import AVFoundation
// Renders texts with an AVSpeechSynthesizer voice (Siri voices are not available to `say -v`).
// usage: swift tts.swift <voiceId> <tsv: text \t output.caf>
let args = CommandLine.arguments
guard let voice = AVSpeechSynthesisVoice(identifier: args[1]) else { print("no voice"); exit(2) }
print("voice", voice.name)
let lines = try! String(contentsOfFile: args[2], encoding: .utf8).split(separator: "\n")
let synth = AVSpeechSynthesizer()
for line in lines {
  let parts = line.split(separator: "\t", maxSplits: 1).map(String.init)
  let u = AVSpeechUtterance(string: parts[0]); u.voice = voice
  var file: AVAudioFile?
  let done = DispatchSemaphore(value: 0)
  synth.write(u) { buf in
    guard let pcm = buf as? AVAudioPCMBuffer else { return }
    if pcm.frameLength == 0 { done.signal(); return }
    if file == nil { file = try! AVAudioFile(forWriting: URL(fileURLWithPath: parts[1]), settings: pcm.format.settings, commonFormat: pcm.format.commonFormat, interleaved: pcm.format.isInterleaved) }
    try! file!.write(from: pcm)
  }
  while done.wait(timeout: .now()) == .timedOut { RunLoop.current.run(until: Date().addingTimeInterval(0.01)) }
  file = nil
  
}
