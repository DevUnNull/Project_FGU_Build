using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHeal = 10;
    public float currentHeal;

    public int damage = 10;
    public void Start()
    {
        currentHeal = MaxHeal;

    }

    public void takeDame (float dame)
    {
        currentHeal -= dame;
    }
}
