﻿namespace PepperDash.Essentials
{
	/// <summary>
	/// 
	/// </summary>
	public class UIUshortJoin
	{
        // Video Codec
        /// <summary>
        /// 1234: values 0 = Connect, 1 = End, 2 = Start Meeting
        /// </summary>
        public const uint VCStagingConnectButtonMode = 1234;

        /// <summary>
        /// 3812
        /// </summary>
        public const uint VolumeSlider1Value = 3812;
        /// <summary>
        /// 3822
        /// </summary>
        public const uint VolumeSlider2Value = 3822;
        /// <summary>
        /// 3832
        /// </summary>
        public const uint VolumeSlider3Value = 3832;
        /// <summary>
        /// 3842
        /// </summary>
        public const uint VolumeSlider4Value = 3842;
        /// <summary>
        /// 3852
        /// </summary>
        public const uint VolumeSlider5Value = 3852;
        /// <summary>
        /// 3862
        /// </summary>
        public const uint VolumeSlider6Value = 3862;

        /// <summary>
        /// 3922: 0-4, center->left. 5-8, center -> right.
        /// </summary>
        public const uint PresentationStagingCaretMode = 3922;

        /// <summary>
        /// 3923: 0-4, center->left. 5-8, center -> right.
        /// </summary>
        public const uint CallStagingCaretMode = 3923;

        /// <summary>
        /// 15024 - Modes 0: On hook, 1: Phone, 2: Video
        /// </summary>
        public const uint CallHeaderButtonMode = 15024;
	}
}