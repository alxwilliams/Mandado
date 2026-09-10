using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : BaseSystem
{
    private BaseMenu _activeMenu;

    public void SetNewMenu(BaseMenu newMenu)
    {
        if (_activeMenu != null && _activeMenu != newMenu && _activeMenu.Active)
        {
            _activeMenu.Show(false);
        }
        
        _activeMenu = newMenu;
    }
}
