namespace Game
{
    public interface ILuaManager
    {
        void DoString(string value);

        void CollectGC();
    }
}