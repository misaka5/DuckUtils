using DuckGame;

namespace DuckGame.DuckUtils
{
    public class DuckUtils : Mod
    {
        protected override void OnPostInitialize()
        {
            base.OnPostInitialize();

            if(configuration.isWorkshop) {
                DevConsole.Log(DCSection.Mod, "|DGYELLOW|DuckUtils has been loaded as a |DGPURPLE|workshop|DGYELLOW| mod");
                DevConsole.Log(DCSection.Mod, "|DGYELLOW|Mod's GitHub page: |DGPURPLE|https://github.com/misaka5/DuckUtils");
            } else {
                DevConsole.Log(DCSection.Mod, "|DGYELLOW|DuckUtils has been loaded as a |DGGREEN|local|DGYELLOW| mod");
            }
        }

        public static string GetAsset(string localPath) {
            return Thing.GetPath<DuckUtils>(localPath);
        }
    }
}