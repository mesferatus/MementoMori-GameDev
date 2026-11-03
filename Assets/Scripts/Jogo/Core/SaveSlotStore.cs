using System;
using System.IO;
using UnityEngine;

namespace MementoMori.Core
{
    /// <summary>Versioned disk storage. Slot zero is reserved for autosave.</summary>
    public sealed class SaveSlotStore
    {
        private readonly string directory;
        public SaveSlotStore(string directory = null)
        {
            this.directory = directory ?? Path.Combine(Application.persistentDataPath, "Saves");
        }

        private string PathFor(int slot)
        {
            if (slot < 0 || slot > 3) throw new ArgumentOutOfRangeException(nameof(slot));
            return Path.Combine(directory, "slot-" + slot + ".json");
        }

        public bool Exists(int slot) => File.Exists(PathFor(slot));

        public void Save(int slot, GameState.PersistentSnapshot snapshot)
        {
            if (snapshot == null || snapshot.schemaVersion != 1 || !IsPlayableScene(snapshot.scene))
                throw new ArgumentException("Invalid save snapshot", nameof(snapshot));
            var destination = PathFor(slot);
            Directory.CreateDirectory(directory);
            var temporary = destination + ".tmp";
            var backup = destination + ".bak";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false)))
                {
                    writer.Write(JsonUtility.ToJson(snapshot));
                    writer.Flush();
                    stream.Flush(true);
                }
                if (File.Exists(destination)) File.Replace(temporary, destination, backup);
                else File.Move(temporary, destination);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        public bool TryLoad(int slot, out GameState.PersistentSnapshot snapshot, out string error)
        {
            snapshot = null;
            error = null;
            var path = PathFor(slot);
            if (!File.Exists(path)) { error = "Slot vazio."; return false; }
            if (TryRead(path, out snapshot)) return true;
            if (TryRead(path + ".bak", out snapshot))
            {
                error = "Cópia de segurança recuperada; salve novamente para atualizar o slot.";
                return true;
            }
            error = "Salvamento incompatível ou inválido.";
            return false;
        }

        private static bool TryRead(string path, out GameState.PersistentSnapshot snapshot)
        {
            snapshot = null;
            if (!File.Exists(path)) return false;
            try
            {
                var candidate = JsonUtility.FromJson<GameState.PersistentSnapshot>(File.ReadAllText(path));
                if (candidate == null || candidate.schemaVersion != 1 || !IsPlayableScene(candidate.scene)) return false;
                snapshot = candidate;
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
                || exception is ArgumentException || exception is NotSupportedException) { return false; }
        }

        public static bool IsPlayableScene(string scene) => scene == "Quarto" || scene == "Labirinto"
            || scene == "DominioLua" || scene == "FinalBeta";
    }
}
