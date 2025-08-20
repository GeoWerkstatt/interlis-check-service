using Geowerkstatt.Ilicop.Web.Contracts;
using System.Collections.Generic;

namespace Geowerkstatt.Ilicop.Web.Services
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
        public List<Profile> GetProfiles()
        {
            return new List<Profile>
            {
                new Profile
                {
                    Id = "DEFAULT",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = null, Text = "" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-0",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "de", Text = "Testprofil 0" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-1",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "en", Text = "Test Profile 1" },
                        new LocalisedText { Language = "de", Text = "Testprofil 1" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-2",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "de", Text = "Testprofil 2" },
                        new LocalisedText { Language = "en", Text = "Test Profile 2" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-3",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "it", Text = "Profilo di prova 3" },
                        new LocalisedText { Language = "en", Text = "Test Profile 3" },
                        new LocalisedText { Language = "de", Text = "Testprofil 3" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-4",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = null, Text = "Test Profile 4" },
                        new LocalisedText { Language = "de", Text = "Testprofil 4" },
                        new LocalisedText { Language = "it", Text = "Profilo di prova 4" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-5",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = null, Text = "Test Profile 5" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-6",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "de", Text = "Testprofil 6" },
                        new LocalisedText { Language = null, Text = "Test Profile 6" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-7",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "", Text = "Test Profile 7" },
                    },
                },
                new Profile
                {
                    Id = "test-profile-8",
                    Titles = new List<LocalisedText>
                    {
                        new LocalisedText { Language = "de", Text = "Testprofil 8" },
                        new LocalisedText { Language = "", Text = "Test Profile 8" },
                    },
                },
            };
        }
    }
}
