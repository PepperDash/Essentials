namespace PepperDash.Essentials.Core
{
    public interface IDspPreset
    {
        void RunPreset(string name);

        void RunPreset(int id);
    }
}