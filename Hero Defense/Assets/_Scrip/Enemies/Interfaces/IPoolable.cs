public interface IPoolable
{
    EnemyType GetEnemyType();
    void Reset();  // Reset state khi return pool (e.g. index=0, health=full)
    void Init(WaypointPath path);  // Set path khi spawn
}