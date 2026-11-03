using MementoMori.Core;

namespace MementoMori.Dialogue
{
    /// <summary>Later reactions approved by the author on 2026-11-03. Source IDs stay stable.</summary>
    public static class V61ChoiceReactivity
    {
        public static string SelectedOption(GameState state, string choiceId)
        {
            if (state == null) return null;
            foreach (var choice in V61EventCatalog.Choices)
            {
                if (choice.id != choiceId) continue;
                foreach (var option in choice.options)
                    if (state.GetCounter("choice." + choiceId + ".option." + option.id) > 0) return option.id;
                // Existing saves used the ordinal. New saves also keep the stable option ID.
                var index = state.GetCounter("choice." + choiceId) - 1;
                return index >= 0 && index < choice.options.Length ? choice.options[index].id : null;
            }
            return null;
        }

        public static DialogueLine Resolve(DialogueLine line, GameState state)
        {
            var replacement = line.Id switch
            {
                "FINAL_D004" => SelectedOption(state, "CH_Q_PHOTO") switch
                {
                    "observe" => "A foto também. Preciso olhar direito, como antes.",
                    "keep" => "A foto também. Ainda não sei o que falta nela.",
                    _ => null
                },
                "M07_D009" => SelectedOption(state, "CH_POE") switch
                {
                    "approach" => "Poe... vem cá. Você quer que eu pegue isso?",
                    "observe" => "Poe... vou esperar. Você quer que eu pegue isso?",
                    _ => null
                },
                "A2_D008" => SelectedOption(state, "CH_A1") switch
                {
                    "trust" => "Eu te pedi um caminho. Você podia falar uma coisa sem me deixar mais confusa?",
                    "challenge" => "Eu te pedi uma resposta. Você podia falar uma coisa sem me deixar mais confusa?",
                    _ => null
                },
                _ => null
            };
            if (replacement != null) line.Text = replacement;
            return line;
        }

        public static string GrimoireRecord(string entryId, GameState state)
        {
            if (entryId != "A02") return string.Empty;
            var selected = SelectedOption(state, "CH_Q_PHOTO");
            if (selected == null) return string.Empty;
            foreach (var choice in V61EventCatalog.Choices)
                if (choice.id == "CH_Q_PHOTO")
                    foreach (var option in choice.options)
                        if (option.id == selected) return "Minha decisão\n" + option.text + "\n\n“" + option.reply + "”";
            return string.Empty;
        }
    }
}
