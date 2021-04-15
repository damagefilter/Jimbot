using Jimbot.Di;
using Jimbot.Plugins.Builtin.Chatbot.Ai;
using Jimbot.Plugins.Builtin.Chatbot.Rules;
using Ninject;

namespace Jimbot.Plugins.Builtin.Chatbot {
    public class ChatbotPlugin : Plugin {
        private ConversationHandler handler;
        public override void ProvideResources(DiContainer diContainer) {
            diContainer.GetImplementation().Bind<ChatbotConfig>().ToMethod(x => ChatbotConfig.Load()).InSingletonScope();
            diContainer.GetImplementation().Bind<ChatbotRuleConfig>().ToMethod(x => ChatbotRuleConfig.Load(x.Kernel.Get<ChatbotConfig>())).InSingletonScope();
            diContainer.GetImplementation().Bind<Memory>().ToSelf().InSingletonScope();
            diContainer.GetImplementation().Bind<ConversationHandler>().ToSelf().InSingletonScope();
        }

        public override void Enable(DiContainer diContainer) {
            handler = diContainer.Get<ConversationHandler>();

        }

        public override void InstallRoutine(DiContainer di) {

        }

        public override void UpdateRoutine(string installedVersion, DiContainer di) {

        }

        protected override void InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "Chris";
            descriptor.Name = "Chatbot";
            descriptor.Version = "5.0";
        }
    }
}