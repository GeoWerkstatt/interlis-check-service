using Geowerkstatt.Ilicop.Models;
using System.Collections.Generic;

namespace Geowerkstatt.Ilicop.Services
{
    public interface IProfileService
    {
        /// <summary>
        /// Returns all available profiles.
        /// </summary>
        IEnumerable<Profile> GetProfiles();
    }
}
