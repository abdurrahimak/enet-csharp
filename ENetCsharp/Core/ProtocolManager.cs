using System;
using System.Collections.Generic;
using System.Linq;

namespace ENetCsharp
{
    public class ProtocolManager : IProtocolManager
    {
        private Dictionary<int, Type> _protocolTypesById;
        private Dictionary<Type, int> _protocolIdsByType;
        private Dictionary<int, IProtocol> _protocolInstancesById;
        private Dictionary<IProtocol, int> _protocolIdsByInstance;
        private int count;

        public ProtocolManager()
        {
            count = 1;
            _protocolTypesById = new Dictionary<int, Type>();
            _protocolIdsByType = new Dictionary<Type, int>();
            _protocolInstancesById = new Dictionary<int, IProtocol>();
            _protocolIdsByInstance = new Dictionary<IProtocol, int>();
        }

        public void RegisterProtocolType(Type packetType)
        {
            if (!_protocolTypesById.Values.Contains(packetType))
            {
                _protocolTypesById.Add(count, packetType);
                _protocolIdsByType.Add(packetType, count);
                RegisterProtocolType((IProtocol)Activator.CreateInstance(packetType));
                count++;
            }
        }

        private void RegisterProtocolType(IProtocol protocolInstance)
        {
            if (!_protocolInstancesById.Values.Contains(protocolInstance))
            {
                _protocolInstancesById.Add(count, protocolInstance);
                _protocolIdsByInstance.Add(protocolInstance, count);
            }
        }

        public Type GetProtocolType(int id)
        {
            return _protocolTypesById.ContainsKey(id) ? _protocolTypesById[id] : null;
        }

        public IProtocol GetProtocolInstance(int id)
        {
            Console.WriteLine($"GetProtocolInstance {id}");
            return _protocolInstancesById[id];
        }

        public IProtocol GetProtocolInstance(Type type)
        {
            return GetProtocolInstance(GetProtocolID(type));
        }

        public int GetProtocolID(Type type)
        {
            return _protocolIdsByType[type];
        }

        public int GetProtocolID(IProtocol protocol)
        {
            return _protocolIdsByType[protocol.GetType()];
        }

        public void Destroy()
        {
            count = 1;
            _protocolTypesById.Clear();
            _protocolIdsByType.Clear();
            _protocolInstancesById.Clear();
            _protocolIdsByInstance.Clear();
        }
    }

}
