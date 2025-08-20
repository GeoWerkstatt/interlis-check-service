using Geowerkstatt.Ilicop.Web.Contracts;
using System.Collections.Generic;

namespace Geowerkstatt.Ilicop.Services
{
    public interface IProfileService
    {
        /// <summary>
        /// Returns all available profiles.
        /// </summary>
        List<Profile> GetProfiles();
    }
}
