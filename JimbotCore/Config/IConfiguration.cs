namespace Jimbot.Config {
    /// <summary>
    /// Configuration interface.
    /// For configurations, implement the abstract type Configuration&lt;T&gt;
    ///
    /// This is just an interface to make the LSB loading work.
    /// </summary>
    public interface IConfiguration {
        /// <summary>
        /// Saves the current data within this config to a file.
        /// </summary>
        void Save();

        /// <summary>
        /// Called after loading the config.
        /// In here you should check each of your config entries and make sure they have values assigned.
        /// Ie. if this config is new (all the config keys are empty), provide sensible defaults.
        /// These will be written to disk if the configuration didn't exist yet.
        /// </summary>
        void Ensure();

    }
}