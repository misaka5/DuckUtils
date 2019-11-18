using DuckGame;

namespace DuckGame.DuckUtils
{
    public class DuckUtils : Mod
    {
        protected override void OnPostInitialize()
        {
            base.OnPostInitialize();

            if(configuration.isWorkshop) {
                DevConsole.Log("DuckUtils has been loaded as a workshop mod", Color.Purple);
            } else {
                DevConsole.Log("DuckUtils has been loaded as a local mod", Color.Yellow);
            }
        }

        public static string GetAsset(string localPath) {
            return Thing.GetPath<DuckUtils>(localPath);
        }
    }
}