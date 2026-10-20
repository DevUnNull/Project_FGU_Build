/// <summary>
/// Vật phẩm nhận được khi chọn lối sống tốt ở Scene 1, dùng 1 lần trong trận cùng ngày.
/// Thêm loại mới: thêm vào đây, ItemCatalog (Resources) và hiệu ứng trong BattleBuffs.Use.
/// </summary>
public enum LifestyleItem
{
    None,
    WaterBottle, // Bình nước: ATP sinh ra nhanh gấp đôi một lúc
    VitaminC,    // Vitamin C: tế bào gây thêm sát thương một lúc
    GoodSleep,   // Giấc ngủ ngon: hồi máu tim + hồi đầy máu tế bào
    WarmSoup     // Bữa ăn ấm: nhận ngay ATP
}
