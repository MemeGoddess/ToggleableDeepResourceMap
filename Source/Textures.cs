using UnityEngine;
using Verse;

namespace ShowDeepResourceMap
{
    [StaticConstructorOnStartup]
    public static class Textures
    {
        public static Texture2D Mine = ContentFinder<Texture2D>.Get("Mine");
    }
}