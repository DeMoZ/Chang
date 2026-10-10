import SwiftUI

@main
struct ChangVoiceApp: App {
    @StateObject private var model = AppModel()

    var body: some Scene {
        WindowGroup("Chang Voice") {
            ContentView(model: model)
                .frame(minWidth: 820, minHeight: 600)
        }
    }
}

struct ContentView: View {
    @ObservedObject var model: AppModel

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            settings
            Divider()
            toolbar
            wordList
            Divider()
            footer
        }
        .padding(14)
    }

    // MARK: settings

    private var settings: some View {
        VStack(alignment: .leading, spacing: 8) {
            HStack {
                Text("Project").frame(width: 70, alignment: .leading)
                Text(model.projectURL?.path ?? "not found").lineLimit(1).truncationMode(.middle).foregroundStyle(.secondary)
                Spacer()
                Button("Choose…") { model.chooseProject() }
                Button("Reload words") { model.reloadProject() }
                    .help("Reads Vocabulary.asset again: download the configs from Google Sheets in Unity first")
            }
            HStack {
                Text("Language").frame(width: 70, alignment: .leading)
                Picker("", selection: $model.language) {
                    ForEach(model.languages, id: \.self) { Text($0).tag($0) }
                }
                .labelsHidden()
                .frame(width: 160)
            }
            HStack(alignment: .top, spacing: 24) {
                VStack(alignment: .leading, spacing: 6) {
                    ForEach(Gender.allCases) { gender in
                        HStack {
                            Toggle(gender.rawValue, isOn: binding(gender)).frame(width: 80, alignment: .leading)
                            Picker("", selection: voiceBinding(gender)) {
                                ForEach(model.languageVoices) { Text($0.title).tag(Optional($0.id)) }
                            }
                            .labelsHidden()
                            .frame(width: 240)
                        }
                    }
                }
                VStack(alignment: .leading, spacing: 6) {
                    HStack {
                        Toggle("Normal", isOn: binding(.normal)).frame(width: 80, alignment: .leading)
                        Slider(value: $model.normalRate, in: 0.3...0.6).frame(width: 140)
                        Text(String(format: "%.2f", model.normalRate)).monospacedDigit()
                    }
                    HStack {
                        Toggle("Slow", isOn: binding(.slow)).frame(width: 80, alignment: .leading)
                        Slider(value: $model.slowRate, in: 0.15...0.45).frame(width: 140)
                        Text(String(format: "%.2f", model.slowRate)).monospacedDigit()
                    }
                }
                VStack(alignment: .leading, spacing: 6) {
                    Toggle("Rewrite existing", isOn: $model.rewrite)
                        .help("Off: only the missing files of the checked words are voiced")
                }
            }
        }
        .disabled(model.isRunning)
    }

    private var toolbar: some View {
        HStack {
            TextField("Filter by key or text", text: $model.filter).frame(width: 240)
            Button("Missing") { model.select("missing") }.help("Check the words with a missing selected file")
            Button("All") { model.select("all") }
            Button("None") { model.select("none") }
            Spacer()
            Text("\(model.checked.count) of \(model.words.count) words · \(model.plannedCount) files to write").foregroundStyle(.secondary)
        }
        .disabled(model.isRunning)
    }

    // MARK: list

    private var wordList: some View {
        List {
            ForEach(model.sections, id: \.name) { section in
                Section(section.name) {
                    ForEach(section.words) { word in row(word) }
                }
            }
        }
        .listStyle(.inset(alternatesRowBackgrounds: true))
        .disabled(model.isRunning)
    }

    private func row(_ word: Word) -> some View {
        HStack(spacing: 10) {
            Toggle("", isOn: Binding(
                get: { model.checked.contains(word.id) },
                set: { if $0 { model.checked.insert(word.id) } else { model.checked.remove(word.id) } }))
                .labelsHidden()
            Text(word.key).frame(width: 220, alignment: .leading).lineLimit(1)
            Text(word.text).font(.system(size: 15)).frame(width: 200, alignment: .leading).lineLimit(1)
            Spacer()
            ForEach(Gender.allCases) { gender in
                ForEach(Speed.allCases) { speed in badge(word, Output(gender: gender, speed: speed)) }
            }
        }
    }

    /// M / F / M slow / F slow: green when the file exists (click plays it), grey when missing.
    private func badge(_ word: Word, _ output: Output) -> some View {
        let exists = model.exists(word, output)
        let selected = model.genders.contains(output.gender) && model.speeds.contains(output.speed)
        let label = (output.gender == .male ? "M" : "F") + (output.speed == .slow ? "s" : "")
        return Button { model.play(word, output) } label: {
            Text(label)
                .font(.system(size: 11, weight: .semibold))
                .frame(width: 26, height: 18)
                .background(RoundedRectangle(cornerRadius: 4).fill(exists ? Color.green.opacity(selected ? 0.8 : 0.35) : Color.gray.opacity(selected ? 0.35 : 0.12)))
                .foregroundStyle(exists ? .white : .secondary)
        }
        .buttonStyle(.plain)
        .help("\(output.gender.rawValue) \(output.speed.rawValue): \(exists ? "exists, click to play" : "missing")")
    }

    // MARK: footer

    private var footer: some View {
        VStack(alignment: .leading, spacing: 6) {
            HStack {
                if model.isRunning {
                    ProgressView(value: model.progress).frame(width: 260)
                    Button("Cancel") { model.cancel() }
                } else {
                    Button("Voice \(model.plannedCount) files") { model.generate() }
                        .keyboardShortcut(.defaultAction)
                        .disabled(model.plannedCount == 0)
                }
                Text(model.status).foregroundStyle(.secondary).lineLimit(1)
                Spacer()
            }
            ScrollViewReader { proxy in
                ScrollView {
                    Text(model.log.joined(separator: "\n"))
                        .font(.system(size: 11, design: .monospaced))
                        .frame(maxWidth: .infinity, alignment: .leading)
                        .textSelection(.enabled)
                        .id("log")
                }
                .frame(height: 90)
                .onChange(of: model.log.count) { proxy.scrollTo("log", anchor: .bottom) }
            }
        }
    }

    // MARK: bindings

    private func binding(_ gender: Gender) -> Binding<Bool> {
        Binding(get: { model.genders.contains(gender) },
                set: { if $0 { model.genders.insert(gender) } else { model.genders.remove(gender) } })
    }

    private func binding(_ speed: Speed) -> Binding<Bool> {
        Binding(get: { model.speeds.contains(speed) },
                set: { if $0 { model.speeds.insert(speed) } else { model.speeds.remove(speed) } })
    }

    private func voiceBinding(_ gender: Gender) -> Binding<String?> {
        Binding(get: { model.voiceIDs[gender] }, set: { model.voiceIDs[gender] = $0 })
    }
}
