using Geowerkstatt.Ilicop.Web.Contracts;
using System.Collections.Generic;

namespace Geowerkstatt.Ilicop.Services
{
    /// <summary>
    /// Service for getting INTERLIS validation profiles.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Returns all available profiles.
        /// </summary>
        List<Profile> GetProfiles();
    }
}
