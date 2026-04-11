namespace Chaos
{
    public static partial class EntitySerializeRegister
    {
        static partial void Register();

        public static void Initialize()
        {
            Register();
        }
    }
}