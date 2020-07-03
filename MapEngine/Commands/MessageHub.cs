using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MapEngine.Commands
{
    public class MessageHub
    {
        private IContainer _container;
        private Dictionary<Type, List<(object, MethodInfo)>> _handlers;
        private Queue<ICommand> _messages = new Queue<ICommand>();

        public MessageHub(IContainer container)
        {
            _container = container;
        }

        public void Initialise()
        {
            _handlers = (from t in AssemblyClasses()
                         from messageType in GetCommandHandlers(t)
                         let handler = _container.Resolve(t)
                         select (messageType, handler))
                         .GroupBy(x => x.messageType)
                         .ToDictionary(k => k.Key, v => GetHandleMethods(v));
        }

        public void Post(ICommand command)
        {
            _messages.Enqueue(command);
        }

        public void Notify()
        {
            while (_messages.TryDequeue(out var message))
            {
                var handlers = _handlers[message.GetType()];
                foreach (var (handler, method) in handlers)
                {
                    method.Invoke(handler, new[] { message });
                }
            }
        }

        private IEnumerable<Type> AssemblyClasses() => Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract);

        private IEnumerable<Type> GetCommandHandlers(Type t) =>
            t.GetInterfaces()
            .Where(x => x.IsGenericType)
            .Where(x => x.GetGenericTypeDefinition() == typeof(IHandleCommand<>))
            .SelectMany(i => i.GenericTypeArguments);

        private List<(object handler, MethodInfo method)> GetHandleMethods(IGrouping<Type, (Type, object handler)> v) =>
                v.Select(x =>
                {
                    var method = x.handler.GetType().GetMethod("Handle", new[] { v.Key });
                    return (x.handler, method);
                }).ToList();
    }
}
