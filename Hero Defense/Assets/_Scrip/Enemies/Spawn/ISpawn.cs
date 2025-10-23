using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawn
{
    // Phương thức chính để bắt đầu quá trình sinh enemy
    void StartSpawning();

    // Phương thức để dừng quá trình sinh enemy
    void StopSpawning();

    // Phương thức để sinh một enemy duy nhất
    void SpawnSingleEnemy();
}
