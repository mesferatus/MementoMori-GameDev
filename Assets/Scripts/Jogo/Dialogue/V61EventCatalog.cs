using System;
using System.Collections.Generic;
using MementoMori.Core;
using UnityEngine;

namespace MementoMori.Dialogue
{
    /// <summary>Loads the unmodified V6.1 event JSON; trigger prose remains metadata until mapped to scene events.</summary>
    public static class V61EventCatalog
    {
        [Serializable] public sealed class Root { public string version; public Event[] events; public Choice[] choices; }
        [Serializable] public sealed class Event
        {
            public string id;
            public string name;
            public string location;
            public string trigger;
            public string direction;
            public string state_effects;
            public string status;
            public Line[] lines;
        }
        [Serializable] public sealed class Line
        {
            public string id;
            public string speaker;
            public string text;
            public string condition;
        }
        [Serializable] public sealed class Choice
        {
            public string id;
            public string @event;
            public string prompt;
            public Option[] options;
        }
        [Serializable] public sealed class Option
        {
            public string id;
            public string text;
            public string reply;
            public Effects effects;
        }
        [Serializable] public sealed class Effects
        {
            public int apego;
            public int discernimento;
            public int respeito_memoria;
            public int confianca_andrealphus;
        }

        private static Root cached;
        private static Dictionary<string, Event> byId;
        public static IReadOnlyList<Event> Events { get { Load(); return cached.events; } }
        public static IReadOnlyList<Choice> Choices { get { Load(); return cached.choices; } }

        public static bool TryGet(string eventId, out Event entry)
        {
            Load();
            return byId.TryGetValue(eventId ?? string.Empty, out entry);
        }

        public static DialogueData CreateDialogue(string eventId, string condition = "always")
        {
            if (!TryGet(eventId, out var entry)) return null;
            var playable = new List<DialogueLine>();
            foreach (var line in entry.lines)
            {
                if (line == null || !IsDialogueSpeaker(line.speaker)) continue;
                if (line.condition != "always" && line.condition != condition) continue;
                playable.Add(new DialogueLine
                {
                    Id = line.id, Speaker = DisplaySpeaker(line.speaker), Text = line.text,
                    Thought = line.speaker == "MELANTHA_MEMORIA", Narration = line.speaker == "INSCRIÇÃO",
                    LockMovement = true
                });
            }
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.Configure(eventId, playable.ToArray());
            return data;
        }

        public static DialogueData CreateDialogueSegment(string eventId, string sequenceId, int firstLine, int lineCount)
        {
            if (!TryGet(eventId, out var entry) || firstLine < 0 || lineCount < 1
                || firstLine + lineCount > entry.lines.Length) return null;
            var playable = new List<DialogueLine>();
            for (var i = firstLine; i < firstLine + lineCount; i++)
            {
                var line = entry.lines[i];
                if (line == null || !IsDialogueSpeaker(line.speaker)) continue;
                playable.Add(new DialogueLine
                {
                    Id = line.id, Speaker = DisplaySpeaker(line.speaker), Text = line.text,
                    Thought = line.speaker == "MELANTHA_MEMORIA", Narration = line.speaker == "INSCRIÇÃO",
                    LockMovement = true
                });
            }
            if (playable.Count == 0) return null;
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.Configure(sequenceId, playable.ToArray());
            return data;
        }

        public static bool TryApplyChoice(GameState state, string choiceId, string optionId, out string reply)
        {
            reply = null;
            if (state == null || string.IsNullOrWhiteSpace(choiceId) || string.IsNullOrWhiteSpace(optionId)) return false;
            Load();
            foreach (var choice in cached.choices)
            {
                if (choice.id != choiceId || state.GetCounter("choice." + choiceId) != 0) continue;
                for (var i = 0; i < choice.options.Length; i++)
                {
                    var option = choice.options[i];
                    if (option.id != optionId) continue;
                    state.SetCounter("choice." + choiceId, i + 1);
                    state.SetCounter("choice." + choiceId + ".option." + option.id, 1);
                    if (option.effects != null)
                    {
                        state.AddNarrativeValue("apego", option.effects.apego);
                        state.AddNarrativeValue("discernimento", option.effects.discernimento);
                        state.AddNarrativeValue("respeito_memoria", option.effects.respeito_memoria);
                        state.AddNarrativeValue("confianca_andrealphus", option.effects.confianca_andrealphus);
                    }
                    reply = option.reply;
                    return true;
                }
                return false;
            }
            return false;
        }

        private static bool IsDialogueSpeaker(string speaker) =>
            speaker != "SFX" && speaker != "AÇÃO" && speaker != "ESCOLHA" && speaker != "UI";

        private static string DisplaySpeaker(string speaker) => speaker != null && speaker.StartsWith("PORTA_") ? "Porta"
            : speaker != null && speaker.StartsWith("ESPELHO_") ? "Espelho"
            : speaker != null && speaker.StartsWith("FLOR_") ? "Flor"
            : speaker != null && speaker.StartsWith("VOZ_") ? "Voz" : speaker switch
        {
            "MELANTHA" or "MELANTHA_MEMORIA" => "Melantha",
            "ANDREALPHUS" => "Andrealphus",
            "INSCRIÇÃO" => "Inscrição",
            "GRIMÓRIO" => "Grimório",
            "OBJETO" => "Objeto",
            _ => speaker
        };

        private static void Load()
        {
            if (cached != null) return;
            var asset = Resources.Load<TextAsset>("Progression/V61Events");
            if (asset == null) throw new InvalidOperationException("V6.1 event JSON is missing from Resources/Progression.");
            var parsed = JsonUtility.FromJson<Root>(asset.text);
            if (parsed?.events == null || parsed.events.Length != 44)
                throw new InvalidOperationException("V6.1 event catalog must contain exactly 44 events.");
            var index = new Dictionary<string, Event>(StringComparer.Ordinal);
            var lineIds = new HashSet<string>(StringComparer.Ordinal);
            var lineCount = 0;
            foreach (var entry in parsed.events)
            {
                if (entry == null || string.IsNullOrEmpty(entry.id) || !index.TryAdd(entry.id, entry))
                    throw new InvalidOperationException("V6.1 event IDs must be present and unique.");
                if (entry.lines == null) throw new InvalidOperationException("V6.1 event has no lines array: " + entry.id);
                foreach (var line in entry.lines)
                {
                    if (line == null || string.IsNullOrEmpty(line.id) || !lineIds.Add(line.id))
                        throw new InvalidOperationException("V6.1 line IDs must be present and unique.");
                    lineCount++;
                }
            }
            if (lineCount != 302) throw new InvalidOperationException("V6.1 event catalog must contain exactly 302 lines.");
            if (parsed.choices == null || parsed.choices.Length != 9)
                throw new InvalidOperationException("V6.1 event catalog must contain exactly 9 choices.");
            var choiceIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var choice in parsed.choices)
            {
                if (choice == null || string.IsNullOrEmpty(choice.id) || !choiceIds.Add(choice.id)
                    || choice.options == null || choice.options.Length < 2)
                    throw new InvalidOperationException("V6.1 choices must have unique IDs and at least two options.");
            }
            cached = parsed;
            byId = index;
        }
    }
}
