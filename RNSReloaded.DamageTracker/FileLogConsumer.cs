using Reloaded.Hooks.Definitions;
using RNSReloaded.Interfaces;
using System.Text.Json;

namespace RNSReloaded.DamageTracker {
    internal unsafe class FileLogConsumer {
        private IRNSReloaded rnsReloaded;
        private string logFolder;
        private StreamWriter logWriter;

        public FileLogConsumer(ILogProducer producer, IRNSReloaded rnsReloaded, IReloadedHooks hooks, string logFolder) {
            this.rnsReloaded = rnsReloaded;
            this.logFolder = logFolder;

            producer.SubscribeAll(
                this.log,
                this.log,
                this.log,
                this.log,
                this.log,
                this.ChooseHallsHandler,
                this.log,
                this.log,
                this.EndFightHandler,
                this.log
            );

            Directory.CreateDirectory(logFolder);
            // Theoretically ChooseHalls should happen before any other event, leaving this empty
            // However, we need it just in case something does get called first, so that logWriter
            // isn't a null reference and crashes
            this.logWriter = File.CreateText(Path.Combine(this.logFolder, "init_log" + ".rnslog"));
        }

        private void log<T>(T data) {
            var dict = data.GetType().GetFields().ToDictionary(f => f.Name, f => f.GetValue(data));
            // This takes up a LOT of space and is almost always 0, so if it's 0 we just remove it to save space
            // As in, this probably saves something close to 10% of the total file size
            if (dict.ContainsKey("painShare") && dict["painShare"].ToString() == "0") {
                dict.Remove("painShare");
            }
            if (dict.ContainsKey("stock") && dict["stock"].ToString() == "0") {
                dict.Remove("stock");
            }
            dict.Add("event", data.GetType().Name.Replace("LogElement", ""));
            this.logWriter.WriteLine(JsonSerializer.Serialize(dict));
        }

        // Keep separate runs in separate files for clarity. Especially important because gameTime resets when runs do
        private void ChooseHallsHandler(LogElementChooseHalls elem) {
            this.logWriter.Close();
            this.logWriter = File.CreateText(Path.Combine(this.logFolder, DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".rnslog"));
            this.log(elem);
        }

        // Every time they move a notch, we want to flush the currently queued data to write
        // Since the game can and does crash, this guarantees us a full log except for the last fight
        // which could be partially written if game crashes in the middle of it
        private void EndFightHandler(LogElementEndFight elem) {
            this.log(elem);
            this.logWriter.Flush();
        }
    }
}
