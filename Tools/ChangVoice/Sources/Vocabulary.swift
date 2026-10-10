import Foundation

/// A word of the project vocabulary that has a sound.
struct Word: Identifiable, Hashable {
    /// The sound key, e.g. "Thai/Vocabulary/Fruits/Mango"
    let id: String
    let section: String
    let key: String
    /// The learn-language text (LearnWord)
    let text: String

    /// "<Section>/<Key>": the sound path under SoundWords/<Language>/<Voice>/
    var relativePath: String {
        let parts = id.split(separator: "/", maxSplits: 2, omittingEmptySubsequences: false)
        return parts.count == 3 ? String(parts[2]) : "\(section)/\(key)"
    }

    /// The text without the marks that are not read aloud: "ไป...มา", "ร้าน + ...", "(แล้ว)เจอกันใหม่";
    /// a slash between alternatives is a pause: "ตรงไป / ตรงมา".
    var speechText: String {
        var text = self.text
        for pattern in [#"\.{2,}"#, "…", #"\+"#, #"[()\[\]]"#] {
            text = text.replacingOccurrences(of: pattern, with: " ", options: .regularExpression)
        }
        text = text.replacingOccurrences(of: #"\s*/\s*"#, with: ", ", options: .regularExpression)
        text = text.replacingOccurrences(of: #"\s+"#, with: " ", options: .regularExpression)
        return text.trimmingCharacters(in: CharacterSet(charactersIn: " ,"))
    }
}

/// Reads the words from Assets/Project/Resources_Bundled/BookConfigs/<Language>/Vocabulary.asset
/// (the config the Google Sheets download writes): Section, SoundKey, Key and LearnWord of every word.
enum VocabularyReader {
    static func read(_ url: URL) throws -> [Word] {
        let lines = try String(contentsOf: url, encoding: .utf8).components(separatedBy: "\n")
        var words: [Word] = []
        var fields: [String: String] = [:]

        func flush() {
            if let sound = fields["SoundKey"], !sound.isEmpty, let text = fields["LearnWord"], !text.isEmpty {
                words.append(Word(id: sound, section: fields["Section"] ?? "", key: fields["Key"] ?? "", text: text))
            }
            fields = [:]
        }

        var i = 0
        while i < lines.count {
            let line = lines[i]
            if line.hasPrefix("  - ") {
                flush()
            }

            // "    Field: value" of a word entry (the first field follows "  - ")
            let body = line.hasPrefix("  - ") ? "    " + line.dropFirst(4) : line
            if body.hasPrefix("    "), !body.hasPrefix("     "), let colon = body.firstIndex(of: ":") {
                let name = body[body.index(body.startIndex, offsetBy: 4)..<colon].trimmingCharacters(in: .whitespaces)
                var value = String(body[body.index(after: colon)...]).trimmingCharacters(in: .whitespaces)
                // a long scalar continues on the next, deeper indented lines
                while i + 1 < lines.count, lines[i + 1].hasPrefix("      "), isOpen(value) {
                    i += 1
                    let next = lines[i].trimmingCharacters(in: .whitespaces)
                    value = value.hasSuffix("\\") ? String(value.dropLast()) + next : value + " " + next
                }
                fields[name] = scalar(value)
            }
            i += 1
        }
        flush()
        return words
    }

    /// A quoted scalar whose closing quote has not come yet.
    private static func isOpen(_ value: String) -> Bool {
        guard let quote = value.first, quote == "\"" || quote == "'" else { return false }
        var escaped = false
        for ch in value.dropFirst() {
            if quote == "\"" && ch == "\\" && !escaped { escaped = true; continue }
            if ch == quote && !escaped { return false }
            escaped = false
        }
        return true
    }

    /// The value of a YAML scalar: plain, 'single' ('' is a quote) or "double" with escapes (\uXXXX, \", \\, \n).
    private static func scalar(_ raw: String) -> String {
        if raw.hasPrefix("'") && raw.hasSuffix("'") && raw.count >= 2 {
            return String(raw.dropFirst().dropLast()).replacingOccurrences(of: "''", with: "'")
        }
        guard raw.hasPrefix("\"") && raw.hasSuffix("\"") && raw.count >= 2 else { return raw }

        var out = ""
        var chars = Array(raw.dropFirst().dropLast())[...]
        while let ch = chars.popFirst() {
            guard ch == "\\", let next = chars.popFirst() else { out.append(ch); continue }
            switch next {
            case "u", "U":
                let length = next == "u" ? 4 : 8
                let hex = String(chars.prefix(length))
                chars = chars.dropFirst(length)
                if let code = UInt32(hex, radix: 16), let scalar = Unicode.Scalar(code) { out.unicodeScalars.append(scalar) }
            case "x":
                let hex = String(chars.prefix(2))
                chars = chars.dropFirst(2)
                if let code = UInt32(hex, radix: 16), let scalar = Unicode.Scalar(code) { out.unicodeScalars.append(scalar) }
            case "n": out.append("\n")
            case "t": out.append("\t")
            case "0": out.append("\0")
            default: out.append(next) // \" \\ \/ and the rest stand for themselves
            }
        }
        return out
    }
}
