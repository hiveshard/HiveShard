using System;
using System.Collections.Generic;
using System.Linq;

namespace HiveShard.Util
{
    public class DependencyBuilder
    {
        private List<Type> _dependencies = new();
        public DependencyBuilder Add<T>()
        {
            _dependencies.Add(typeof(T));
            return this;
        }

        public IEnumerable<Type> Build() => _dependencies.AsEnumerable();
    }
}