using System;

namespace WebApi.HealthChecks.Models
{
    internal class Registration
    {
        public Registration(IHealthCheck instance)
        {
            Instance = instance;
            IsSingleton = true;
        }

        public Registration(Type type)
        {
            Type = type;
            IsSingleton = false;
        }

        public IHealthCheck Instance { get; }

        public Type Type { get; set; }

        public bool IsSingleton { get; }
    }
}