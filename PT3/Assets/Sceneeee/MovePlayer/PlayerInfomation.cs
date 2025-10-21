using UnityEngine;

public class PlayerInfomation : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;

    public int damage=10;
    void Start()
    {
        currentHP = maxHP;
        UIPlayer.Instance.UpdateHP(currentHP, maxHP);
    }

    public void TakeDame(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        // Gửi tín hiệu cho UI cập nhật
        UIPlayer.Instance.UpdateHP(currentHP, maxHP);
    }
}
