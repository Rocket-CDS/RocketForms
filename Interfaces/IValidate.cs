using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;

namespace RocketForms.Interfaces
{

    public abstract class IValidate
    {
        public abstract SimplisityInfo ValidateForm(SimplisityInfo postFormInfo, SimplisityInfo paramFormInfo);
        public abstract SimplisityInfo ValidatePin(SimplisityInfo postPinInfo, SimplisityInfo paramPinInfo);
    }

    public sealed class Validate
    {
        private static Dictionary<string, IValidate> _instances;
        private static object _lock = new object();
        public static IValidate GetInstance(string assembly, string nameSpaceClass)
        {
            var provKey = assembly + "," + nameSpaceClass;
            lock (_lock)
            {
                if (_instances == null) _instances = new Dictionary<string, IValidate>();
                if (!_instances.ContainsKey(provKey))
                {
                    var prov = CreateProvider(assembly, nameSpaceClass);
                    if (prov != null) _instances.Add(provKey, prov);
                }
            }
            return _instances[provKey];
        }
        private static IValidate CreateProvider(string assembly, string nameSpaceClass)
        {
            if (!string.IsNullOrEmpty(assembly) && !string.IsNullOrEmpty(nameSpaceClass))
            {
                try
                {
                    var targetAssembly = System.Reflection.Assembly.Load(assembly.Trim());

                    // Get the type
                    var type = targetAssembly.GetType(nameSpaceClass.Trim());
                    if (type != null)
                    {
                        // Create instance
                        var instance = Activator.CreateInstance(type);
                        return (IValidate)instance;
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.LogException(ex);
                    return null;
                }
            }
            return null;
        }
        private Validate()
        {
        }
    }

}
