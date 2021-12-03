using System.Collections.Generic;

namespace Game
{
    public interface ICSharpCallLua
    {
        void OnReceiveMsg(ref List<byte[]> msg);
    }
}