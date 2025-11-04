// Import thư viện Unity cơ bản
using UnityEngine;

// Định nghĩa interface IHeroState - đây là contract chung cho tất cả các state trong Hero State Machine
// Interface này định nghĩa 3 phương thức cơ bản mà mọi state phải implement
public interface IHeroState 
{
    // Phương thức được gọi khi chuyển vào state này
    void EnterState();
    
    // Phương thức được gọi mỗi frame khi đang ở trong state này
    void UpdateState();
    
    // Phương thức được gọi khi thoát khỏi state này
    void ExitState();
}

