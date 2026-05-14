using System;

public static class ShopRefreshEvents
{
    public static event Action OnShopRefreshRequested;

    public static void RequestShopRefresh()
    {
        OnShopRefreshRequested?.Invoke();
    }
}