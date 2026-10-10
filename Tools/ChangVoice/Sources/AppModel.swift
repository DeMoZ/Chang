import AppKit
import AVFoundation
import Foundation

enum Gender: String, CaseIterable, Identifiable {
    case male = "Male", female = "Female"
    var id: String { rawValue }
}

enum Speed: String, CaseIterable, Identifiable {
    case normal = "Normal", slow = "Slow"
    var id: String { rawValue }

    /// Root folder of the sounds under Assets/Project/Resources_Bundled
    var root: String { self == .normal ? "SoundWords" : "SoundWordsSlow" }
}

/// One output file of a word: a voice and a speed.
struct Output: Hashable {
    let gender: Gender
    let speed: Speed
}

struct VoiceInfo: Identifiable, Hashable, Decodable {
    let id: String
    let name: String
    let language: String
    let gender: String
    let siri: Bool

    var title: String { "\(name)\(siri ? " (Siri)" : "")\(gender.isEmpty ? "" : " · \(gender)")" }
}

@MainActor
final class AppModel: ObservableObject {
    // word sounds must have no silence around them: sentences are assembled from words (Docs/content-pipeline.md)
    private static let trim = "silenceremove=start_periods=1:start_threshold=-40dB:start_silence=0.02:detection=rms:window=0.02,areverse"
    private static let localeOfLanguage = ["Thai": "th", "English": "en", "Russian": "ru", "Vietnamese": "vi", "Lao": "lo",
                                           "ChineseSimplified": "zh", "Japanese": "ja", "Korean": "ko"]

    @Published var projectURL: URL? { didSet { UserDefaults.standard.set(projectURL?.path, forKey: "projectPath") } }
    @Published var languages: [String] = []
    @Published var language = "" { didSet { if language != oldValue { loadWords() } } }
    @Published var words: [Word] = []
    @Published var voices: [VoiceInfo] = []
    @Published var voiceIDs: [Gender: String] = [:]
    @Published var genders: Set<Gender> = [.male, .female] { didSet { resetChecks() } }
    @Published var speeds: Set<Speed> = [.normal] { didSet { resetChecks() } }
    @Published var normalRate: Double = 0.5
    @Published var slowRate: Double = 0.32
    @Published var rewrite = false
    @Published var checked: Set<String> = []
    @Published var filter = ""
    @Published private(set) var existing: Set<String> = []
    @Published private(set) var isRunning = false
    @Published private(set) var progress = 0.0
    @Published private(set) var status = ""
    @Published private(set) var log: [String] = []

    private var process: Process?
    private var cancelled = false
    private var player: AVAudioPlayer?

    init() {
        projectURL = Self.findProject()
        reloadProject()
        Task { await loadVoices() }
    }

    // MARK: project

    var soundsRoot: URL? { projectURL?.appendingPathComponent("Assets/Project/Resources_Bundled") }

    /// The app is built inside the project (Tools/ChangVoice/build); otherwise the last chosen folder.
    private static func findProject() -> URL? {
        var url = Bundle.main.bundleURL
        for _ in 0..<6 {
            url.deleteLastPathComponent()
            if FileManager.default.fileExists(atPath: url.appendingPathComponent("Assets/Project/Resources_Bundled").path) { return url }
        }
        return UserDefaults.standard.string(forKey: "projectPath").map { URL(fileURLWithPath: $0) }
    }

    func chooseProject() {
        let panel = NSOpenPanel()
        panel.canChooseDirectories = true
        panel.canChooseFiles = false
        panel.message = "The Chang Unity project folder"
        if panel.runModal() == .OK, let url = panel.url {
            projectURL = url
            reloadProject()
        }
    }

    func reloadProject() {
        guard let root = soundsRoot?.appendingPathComponent("BookConfigs") else { return }
        let folders = (try? FileManager.default.contentsOfDirectory(atPath: root.path)) ?? []
        languages = folders.filter { FileManager.default.fileExists(atPath: root.appendingPathComponent("\($0)/Vocabulary.asset").path) }.sorted()
        if !languages.contains(language) { language = languages.first ?? "" } else { loadWords() }
    }

    func loadWords() {
        guard let url = soundsRoot?.appendingPathComponent("BookConfigs/\(language)/Vocabulary.asset") else { return }
        do {
            words = try VocabularyReader.read(url)
            status = "\(words.count) words in \(language)/Vocabulary.asset"
        } catch {
            words = []
            status = "Can't read \(url.path): \(error.localizedDescription)"
        }
        pickDefaultVoices()
        refreshExisting()
        resetChecks()
    }

    // MARK: voices

    private func loadVoices() async {
        status = "Loading voices…"
        do {
            let output = try await Helper.run(["--list-json"])
            if let line = output.split(separator: "\n").last(where: { $0.hasPrefix("[") }) {
                voices = try JSONDecoder().decode([VoiceInfo].self, from: Data(line.utf8))
            }
            pickDefaultVoices()
            status = "\(words.count) words, \(voices.count) voices"
        } catch {
            status = "Voices: \(error.localizedDescription)"
        }
    }

    var languageVoices: [VoiceInfo] {
        let prefix = Self.localeOfLanguage[language] ?? String(language.prefix(2)).lowercased()
        return voices.filter { $0.language.hasPrefix(prefix) }.sorted { ($0.siri ? 0 : 1, $0.name) < ($1.siri ? 0 : 1, $1.name) }
    }

    /// A Siri voice of the gender first (Thai: Voice 1 is male, Voice 2 female), then any voice of the gender.
    private func pickDefaultVoices() {
        let candidates = languageVoices
        for gender in Gender.allCases where !candidates.contains(where: { $0.id == voiceIDs[gender] }) {
            voiceIDs[gender] = (candidates.first { $0.siri && $0.gender == gender.rawValue } ?? candidates.first { $0.gender == gender.rawValue })?.id
        }
    }

    // MARK: files

    func url(_ word: Word, _ output: Output) -> URL? {
        soundsRoot?.appendingPathComponent("\(output.speed.root)/\(language)/\(output.gender.rawValue)/\(word.relativePath).mp3")
    }

    private func fileID(_ word: Word, _ output: Output) -> String { "\(output.speed.root)/\(output.gender.rawValue)/\(word.id)" }

    func exists(_ word: Word, _ output: Output) -> Bool { existing.contains(fileID(word, output)) }

    var selectedOutputs: [Output] {
        Gender.allCases.filter(genders.contains).flatMap { g in Speed.allCases.filter(speeds.contains).map { Output(gender: g, speed: $0) } }
    }

    func refreshExisting() {
        var found = Set<String>()
        for word in words {
            for gender in Gender.allCases {
                for speed in Speed.allCases {
                    let output = Output(gender: gender, speed: speed)
                    if let url = url(word, output), FileManager.default.fileExists(atPath: url.path) { found.insert(fileID(word, output)) }
                }
            }
        }
        existing = found
    }

    /// A word is checked when one of its selected files is missing; the user can change it.
    func resetChecks() {
        checked = Set(words.filter { word in selectedOutputs.contains { !exists(word, $0) } }.map(\.id))
    }

    func select(_ which: String) {
        switch which {
        case "all": checked = Set(filteredWords.map(\.id))
        case "none": checked.subtract(filteredWords.map(\.id))
        default: resetChecks()
        }
    }

    var filteredWords: [Word] {
        let f = filter.trimmingCharacters(in: .whitespaces).lowercased()
        return f.isEmpty ? words : words.filter { $0.id.lowercased().contains(f) || $0.text.contains(f) }
    }

    var sections: [(name: String, words: [Word])] {
        var order: [String] = []
        var bySection: [String: [Word]] = [:]
        for word in filteredWords {
            if bySection[word.section] == nil { order.append(word.section) }
            bySection[word.section, default: []].append(word)
        }
        return order.map { ($0, bySection[$0]!) }
    }

    func play(_ word: Word, _ output: Output) {
        guard let url = url(word, output), exists(word, output) else { return }
        player = try? AVAudioPlayer(contentsOf: url)
        player?.play()
    }

    // MARK: generation

    /// The files to write: the selected outputs of the checked words, existing ones only with "Rewrite existing".
    var plannedCount: Int {
        words.filter { checked.contains($0.id) }.reduce(0) { sum, word in
            sum + selectedOutputs.filter { rewrite || !exists(word, $0) }.count
        }
    }

    func cancel() {
        cancelled = true
        process?.terminate()
    }

    func generate() {
        guard !isRunning, let ffmpeg = Self.ffmpeg else {
            if Self.ffmpeg == nil { append("ffmpeg not found: brew install ffmpeg") }
            return
        }

        var jobs: [(Output, String, [(Word, URL)])] = []
        for output in selectedOutputs {
            guard let voice = voiceIDs[output.gender] else { append("No \(output.gender.rawValue) voice for \(language)"); continue }
            let items = words.filter { checked.contains($0.id) && (rewrite || !exists($0, output)) }.compactMap { w in url(w, output).map { (w, $0) } }
            if !items.isEmpty { jobs.append((output, voice, items)) }
        }
        let total = jobs.reduce(0) { $0 + $1.2.count }
        guard total > 0 else { append("Nothing to voice"); return }

        isRunning = true
        cancelled = false
        progress = 0
        log = []
        let rates = (normal: Float(normalRate), slow: Float(slowRate))
        let foldersBefore = soundFolders()

        Task {
            var done = 0
            for (output, voice, items) in jobs where !cancelled {
                status = "\(output.gender.rawValue) · \(output.speed.rawValue): \(items.count) words"
                let temp = FileManager.default.temporaryDirectory.appendingPathComponent("chang-voice-\(UUID().uuidString)")
                try? FileManager.default.createDirectory(at: temp, withIntermediateDirectories: true)
                defer { try? FileManager.default.removeItem(at: temp) }

                let cafs = items.indices.map { temp.appendingPathComponent("\($0).caf") }
                let job: [String: Any] = [
                    "voice": voice,
                    "rate": output.speed == .slow ? rates.slow : rates.normal,
                    "items": zip(items, cafs).map { ["text": $0.0.0.speechText, "out": $0.1.path] },
                ]
                let jobURL = temp.appendingPathComponent("job.json")
                do {
                    try JSONSerialization.data(withJSONObject: job).write(to: jobURL)
                    let base = done
                    _ = try await Helper.run(["--render", jobURL.path], process: { self.process = $0 }) { line in
                        let parts = line.split(separator: " ")
                        if parts.count == 3, parts[0] == "DONE", let i = Double(parts[1]) {
                            Task { @MainActor in self.progress = (Double(base) + i * 0.5) / Double(total) }
                        }
                    }
                } catch {
                    if !cancelled { append("\(output.gender.rawValue) \(output.speed.rawValue): \(error.localizedDescription)") }
                    continue
                }

                for ((word, mp3), caf) in zip(items, cafs) where !cancelled {
                    let ok = await Self.convert(ffmpeg: ffmpeg, caf: caf, mp3: mp3)
                    done += 1
                    progress = Double(done) / Double(total)
                    append(ok ? "✓ \(output.gender.rawValue) \(output.speed.rawValue) \(word.relativePath) [\(word.speechText)]"
                              : "✗ \(word.relativePath): ffmpeg failed")
                }
            }

            refreshExisting()
            let newFolders = soundFolders().subtracting(foldersBefore).sorted()
            if !newFolders.isEmpty {
                append("New folders (Unity adds them to the Addressables groups on import):\n  " + newFolders.joined(separator: "\n  "))
            }
            status = cancelled ? "Cancelled after \(done) of \(total)" : "Done: \(done) files"
            isRunning = false
            process = nil
            resetChecks()
        }
    }

    private func append(_ line: String) { log.append(line) }

    /// Section folders of the sounds, to tell which ones are new.
    private func soundFolders() -> Set<String> {
        guard let root = soundsRoot else { return [] }
        var result = Set<String>()
        for speed in Speed.allCases {
            for gender in Gender.allCases {
                let dir = root.appendingPathComponent("\(speed.root)/\(language)/\(gender.rawValue)")
                for name in (try? FileManager.default.contentsOfDirectory(atPath: dir.path)) ?? [] where !name.hasSuffix(".meta") {
                    result.insert("\(speed.root)/\(language)/\(gender.rawValue)/\(name)")
                }
            }
        }
        return result
    }

    static var ffmpeg: String? {
        ["/opt/homebrew/bin/ffmpeg", "/usr/local/bin/ffmpeg"].first { FileManager.default.isExecutableFile(atPath: $0) }
    }

    /// Trims the silence and writes mp3 24 kHz mono 32 kbps, like MediaGenerator.
    private static func convert(ffmpeg: String, caf: URL, mp3: URL) async -> Bool {
        try? FileManager.default.createDirectory(at: mp3.deletingLastPathComponent(), withIntermediateDirectories: true)
        return await withCheckedContinuation { continuation in
            let p = Process()
            p.executableURL = URL(fileURLWithPath: ffmpeg)
            p.arguments = ["-y", "-loglevel", "error", "-i", caf.path, "-af", "\(trim),\(trim)", "-ar", "24000", "-ac", "1", "-b:a", "32k", mp3.path]
            p.terminationHandler = { continuation.resume(returning: $0.terminationStatus == 0) }
            do { try p.run() } catch { continuation.resume(returning: false) }
        }
    }
}
