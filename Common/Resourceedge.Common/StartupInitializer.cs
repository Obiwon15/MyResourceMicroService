using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Common
{
    public class StartupInitializer : IStartupInitializer
    {
        private readonly ISet<IInitializer> _initializers = new HashSet<IInitializer>();

        public void AddInitializer(IInitializer initializer) => _initializers.Add(initializer);

        public async Task InitializeAsync()
        {
            var validInitializer = _initializers.Where(x => x != null);
            if (_initializers.Any() && _initializers.Count > 0)
            {
                await Task.WhenAll(validInitializer.Select(x => x.InitializeAsync()));
            }
        }
    }
}
