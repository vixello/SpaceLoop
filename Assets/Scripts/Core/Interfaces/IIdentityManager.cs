using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IIdentityManager
    {
        IdentityData GetIdentity();
        void ClearIdentity();
        bool IsUsernameSet();
    }
}
