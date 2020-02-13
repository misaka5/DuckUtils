using DuckGame;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace DuckGame.DuckUtils
{
    public class DuckUtils : Mod, IUpdateable, IDrawable
    {
        public static DuckUtils Instance { get; private set; }

        public static event EventHandler Updated {
            add {
                Instance.events.Updated += value;
            }

            remove {
                Instance.events.Updated -= value;
            }
        }

        public static event EventHandler Drawing {
            add {
                Instance.events.Drawing += value;
            }

            remove {
                Instance.events.Drawing -= value;
            }
        }

        public static event EventHandler<LevelChangedEventArgs> LevelChanged {
            add {
                Instance.events.LevelChanged += value;
            }

            remove {
                Instance.events.LevelChanged -= value;
            }
        }

        public static double Time {
            get {
                return Instance.time;
            }
        }

        public bool Enabled {
            get {
                return true;
            }
        }

        public int UpdateOrder {
            get {
                return 1000;
            }
        }

        public bool Visible {
            get {
                return true;
            }
        }

        public int DrawOrder {
            get {
                return 1000;
            }
        }

        public event EventHandler<EventArgs> EnabledChanged;
	    public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
	    public event EventHandler<EventArgs> DrawOrderChanged;

        private readonly EventCore events = new EventCore();
        private double time = 0f;

        protected override void OnPreInitialize()
        {
            if(Instance != null) throw new InvalidProgramException("Something went horribly wrong! Two instances of the mod exist at the same time!");
            Instance = this;
        }

        protected override void OnPostInitialize()
        {
            base.OnPostInitialize();

            (typeof(Game).GetField("updateableComponents", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).GetValue(MonoMain.instance) as List<IUpdateable>).Add(this);
            (typeof(Game).GetField("drawableComponents", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).GetValue(MonoMain.instance) as List<IDrawable>).Add(this);

            if(configuration.isWorkshop) {
                DevConsole.Log(DCSection.Mod, "|DGYELLOW|DuckUtils has been loaded as a |DGPURPLE|workshop|DGYELLOW| mod");
                DevConsole.Log(DCSection.Mod, "|DGYELLOW|GitHub page: |DGPURPLE|https://github.com/misaka5/DuckUtils");
            } else {
                DevConsole.Log(DCSection.Mod, "|DGYELLOW|DuckUtils has been loaded as a |DGGREEN|local|DGYELLOW| mod");
            }
        }

        public void Update(GameTime gt) 
        {
            events.Update();
            time += gt.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(GameTime gt) 
        {
            events.Draw();
        }

        public static string GetAsset(string localPath) {
            return Thing.GetPath<DuckUtils>(localPath);
        }

        public class EventCore {
            public event EventHandler Updated;
            public event EventHandler Drawing;
            public event EventHandler<LevelChangedEventArgs> LevelChanged;

            private Level level;

            public EventCore() {
                level = Level.current;
            }

            public void Update() {
                if(Level.current != level) {
                    if(LevelChanged != null) LevelChanged.Invoke(this, new LevelChangedEventArgs(level, Level.current));
                }
                level = Level.current;

                if(Updated != null) Updated.Invoke(this, EventArgs.Empty);
            }

            public void Draw() {
                if(Drawing != null) Drawing.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public class LevelChangedEventArgs : EventArgs {

        public Level Level { get; private set; }
        public Level PrevLevel { get; private set; }

        public LevelChangedEventArgs(Level prevLevel, Level level) {
            PrevLevel = prevLevel;
            Level = level;
        }
    }
}