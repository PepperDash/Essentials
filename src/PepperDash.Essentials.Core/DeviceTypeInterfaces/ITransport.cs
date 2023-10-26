namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface ITransport
	{
		void Play(bool pressRelease);
		void Pause(bool pressRelease);
		void Rewind(bool pressRelease);
		void FFwd(bool pressRelease);
		void ChapMinus(bool pressRelease);
		void ChapPlus(bool pressRelease);
		void Stop(bool pressRelease);
		void Record(bool pressRelease);
	}
}