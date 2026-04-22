using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IUIPictureTemplate
    {
        public string Id { get; }
        public void SetId(string id);
        public void SetIamgeData(Sprite sprite, int levelToUnlock);
        public void SetUnlockStatus(bool isUnlocked);
        public void SetSelectedStatus(bool isSelected);
    }
}
