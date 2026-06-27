using UnityEngine;

public class _Hero : HeroBase
{
    private void Awake()
    {
        SetFromData(heroData);
    }
}
