using XLua;

namespace Game
{
    [LuaCallCSharp]
    public class ProedureTest :ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        public override string LuaScriptName
        {
            get
            {
                return "Procedure/ProcedureTest";
            }
        }
    }
}