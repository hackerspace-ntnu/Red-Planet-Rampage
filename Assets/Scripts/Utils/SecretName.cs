using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SecretName
{
    public static class SecretName
    {
        // The potential override names to be given specific combinations of augments
        /*
        [SerializeField]
        private SecretNameOverride[] overrideNames;
        */
        private static SecretNameOverride[] OverrideNames;
        /*
        /// <summary>
        /// Gets the secret final displaying name matching given augments.
        /// Final name can be overriden by the SecretName class overrideNames field in editor
        /// </summary>
        /// <param name="playerIdentity">The source of items, takes each of the public singular item fields</param>
        /// <returns></returns>
        public static string GetName(this PlayerIdentity playerIdentity)
        {
            OverrideNames = GunFactory.GetGunName(playerIdentity.Barrel, playerIdentity.Body, playerIdentity.Extension);
            foreach (SecretNameOverride overrides in OverrideNames)
            {
                if (overrides.body == playerIdentity.Body && overrides.barrel == playerIdentity.Barrel && overrides.extension == playerIdentity.Extension)
                {
                    return overrides.name;
                }
            }
            return $"The {playerIdentity.Body} {playerIdentity.Extension} {playerIdentity.Barrel}";
        }*/
    }
}
