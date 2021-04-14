namespace Jimbot.EventDispatcher {
    public class Event<TImplementor> : IEvent where TImplementor : IEvent {
        /// <summary>
        /// Raise this this event on the Event Dispatcher.
        /// </summary>
        public void Call() {
            Dispatcher.Instance.Call<TImplementor>(this);
        }

        /// <summary>
        /// Register a callback for this event
        /// </summary>
        /// <param name="handler"></param>
        public static void Register(Callback<TImplementor> handler) {
            Dispatcher.Instance.Register(handler);
        }

        /// <summary>
        /// Unregister a callback for this event.
        /// </summary>
        /// <param name="handler"></param>
        public static void Unregister(Callback<TImplementor> handler) {
            Dispatcher.Instance.Unregister(handler);
        }
    }
}