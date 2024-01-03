using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SecretName
{
    public static class SecretName
    {
        // The potential override names to be given specific combinations of augments

        public static string GetGunName(this PlayerIdentity playerIdentity)
        {
            return GunFactory.GetGunName(playerIdentity.Body, playerIdentity.Barrel, playerIdentity.Extension);

        }

        public static string GetGunName(this PlayerManager playerManager)
        {
            return playerManager.identity.GetGunName();
        }

        public static string Capitalized(this string s)
        {
            return $"{s.Substring(0, 1).ToUpper()}{s.Substring(1).ToLower()}";
        }
    }
}
