using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IUITitleTemplate
    {
        public string Id { get; }
        public void SetId(string id);   
        public void SetDisplayName(string displayName);
        public void SetUnlockState(bool isUnlocked);
        public void SetSelectedState(bool isSelected);
        public void SetDescription(string description);
        event Action<string> OnClicked;
    }
}
