import AppKit
import Foundation

enum HelperError: LocalizedError {
    case unavailable(String)
    case failed(String)

    var errorDescription: String? {
        switch self {
        case .unavailable(let m), .failed(let m): return m
        }
    }
}

/// Runs Resources/siri-tts.swift through `xcrun swift`: only Apple-signed programs get the Siri voices,
/// and the Swift interpreter from Xcode is one of them (the same trick as the SubDub app).
enum Helper {
    static var developerDir: URL? {
        [NSWorkspace.shared.urlForApplication(withBundleIdentifier: "com.apple.dt.Xcode"), URL(fileURLWithPath: "/Applications/Xcode.app")]
            .compactMap { $0 }
            .map { $0.appendingPathComponent("Contents/Developer") }
            .first { FileManager.default.fileExists(atPath: $0.appendingPathComponent("usr/bin").path) }
    }

    static var scriptURL: URL? { Bundle.main.url(forResource: "siri-tts", withExtension: "swift") }

    /// Runs the helper and returns its standard output; `onLine` gets every output line as it comes.
    static func run(_ arguments: [String], process onStart: (@MainActor (Process) -> Void)? = nil,
                    onLine: (@Sendable (String) -> Void)? = nil) async throws -> String {
        guard let developerDir else { throw HelperError.unavailable("Xcode is not installed: Siri voices need its Swift interpreter") }
        guard let scriptURL else { throw HelperError.unavailable("siri-tts.swift is missing in the app") }

        let process = Process()
        process.executableURL = URL(fileURLWithPath: "/usr/bin/xcrun")
        process.arguments = ["swift", scriptURL.path] + arguments
        var env = ProcessInfo.processInfo.environment
        env["DEVELOPER_DIR"] = developerDir.path
        process.environment = env
        let out = Pipe(), err = Pipe()
        process.standardOutput = out
        process.standardError = err
        await onStart?(process)

        let collected = Collector()
        out.fileHandleForReading.readabilityHandler = { handle in
            for line in collected.append(String(decoding: handle.availableData, as: UTF8.self)) { onLine?(line) }
        }
        return try await withCheckedThrowingContinuation { continuation in
            process.terminationHandler = { p in
                out.fileHandleForReading.readabilityHandler = nil
                let rest = String(decoding: out.fileHandleForReading.readDataToEndOfFile(), as: UTF8.self)
                for line in collected.append(rest + "\n") { onLine?(line) }
                if p.terminationStatus == 0 {
                    continuation.resume(returning: collected.text)
                } else {
                    let message = String(decoding: err.fileHandleForReading.readDataToEndOfFile(), as: UTF8.self)
                    continuation.resume(throwing: HelperError.failed(message.trimmingCharacters(in: .whitespacesAndNewlines)))
                }
            }
            do { try process.run() } catch { continuation.resume(throwing: error) }
        }
    }

    /// Thread-safe line splitter for the helper output.
    private final class Collector: @unchecked Sendable {
        private let lock = NSLock()
        private var buffer = ""
        private(set) var text = ""

        func append(_ chunk: String) -> [String] {
            lock.lock(); defer { lock.unlock() }
            text += chunk
            buffer += chunk
            var lines = buffer.components(separatedBy: "\n")
            buffer = lines.removeLast()
            return lines.filter { !$0.isEmpty }
        }
    }
}
