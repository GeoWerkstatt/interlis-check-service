using Geowerkstatt.Ilicop.Models;
using System.Collections.Generic;

namespace Geowerkstatt.Ilicop.Services
{
    /// <summary>
    /// Dummy implementation of the IProfileService for testing purposes.
    /// </summary>
    public class DummyProfileService : IProfileService
    {
        /// <summary>
        /// Returns a static list of profiles for testing purposes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Profile> GetProfiles()
        {
            return new List<Profile>
            {
                new Profile
                {
                    Id = "test-profile-1",
                    LocalisedTitles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "en", Text = "Test Profile 1" },
                        new LocalisedText { Language = "de", Text = "Testprofil 1" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-2",
                    LocalisedTitles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "en", Text = "Test Profile 2" },
                        new LocalisedText { Language = "de", Text = "Testprofil 2" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-3",
                    LocalisedTitles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "en", Text = "Test Profile 3" },
                        new LocalisedText { Language = "de", Text = "Testprofil 3" },
                    },
                },
            };
        }
    }
}
