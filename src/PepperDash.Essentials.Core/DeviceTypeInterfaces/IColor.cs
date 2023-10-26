namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IColor
    {
        void Red(bool pressRelease);
        void Green(bool pressRelease);
        void Yellow(bool pressRelease);
        void Blue(bool pressRelease);
    }
}