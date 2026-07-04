public interface IPoolable
{
    void OnSpawn();   // reset
    void OnDespawn(); // dọn dẹp trước khi ẩn đi
}