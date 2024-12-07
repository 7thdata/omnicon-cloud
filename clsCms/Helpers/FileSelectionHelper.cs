using System;
using System.Collections.Generic;

namespace clsCms.Helpers
{
    /// <summary>
    /// Provides helper methods for selecting files or other utility functions.
    /// </summary>
    public static class FileSelectionHelper
    {
        private static readonly List<string> ProfileImages = new List<string>
        {
            "/images/profile-image/f_f_event_98_s512_f_event_98_2bg.png",
            "/images/profile-image/f_f_object_81_s512_f_object_81_1bg.png",
            "/images/profile-image/f_f_object_87_s512_f_object_87_1bg.png",
            "/images/profile-image/f_f_object_88_s512_f_object_88_1bg.png",
            "/images/profile-image/f_f_object_89_s512_f_object_89_2bg.png",
            "/images/profile-image/f_f_object_95_s512_f_object_95_1bg.png",
            "/images/profile-image/f_f_object_97_s512_f_object_97_1bg.png",
            "/images/profile-image/f_f_object_105_s512_f_object_105_1bg.png",
            "/images/profile-image/f_f_object_108_s512_f_object_108_0bg.png",
            "/images/profile-image/f_f_object_149_s512_f_object_149_0bg.png",
            "/images/profile-image/f_f_object_152_s512_f_object_152_1bg.png",
            "/images/profile-image/f_f_object_153_s512_f_object_153_2bg.png",
            "/images/profile-image/f_f_object_158_s512_f_object_158_2bg.png",
            "/images/profile-image/f_f_object_169_s512_f_object_169_1bg.png",
            "/images/profile-image/f_f_object_170_s512_f_object_170_2bg.png",
            "/images/profile-image/f_f_object_174_s512_f_object_174_1bg.png"
        };

        /// <summary>
        /// Selects a random image file from the predefined list of profile images and returns its relative path.
        /// </summary>
        /// <returns>The relative file path of a randomly selected profile image.</returns>
        public static string GetRandomProfileImage()
        {
            var random = new Random();
            int index = random.Next(ProfileImages.Count);
            return ProfileImages[index];
        }
    }
}
