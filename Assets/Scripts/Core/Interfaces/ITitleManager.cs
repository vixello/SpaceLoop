using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface ITitleManager : IInitializable, ISaveable
    {
        public IReadOnlyList<RuntimeTitle> Titles { get; }
    }
}
