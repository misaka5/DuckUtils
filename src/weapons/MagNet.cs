using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
    [BaggedProperty("isFatal", false)]
    public class MagNet : Gun
    {
		public static readonly ATProvider ExplosionShrapnel = ExplosionAT.From<ATShrapnel>().WithRange(2f, 6f);

		public static readonly float MaxRange = 100f;
		public static readonly float RangeAllocationSpeed = 60f;
		public static readonly float ExplosionRangeSquared = 15f * 15f;
		public static readonly float CapturingSpeed = 20.6667f;
		public static readonly float SpeedApproachMod = 0.08f;
		public static readonly float ExplosionTime = 1f;
		public static readonly float RandomFluctionations = 0.5f;

        private readonly SpriteMap spriteMap;
		private readonly SpriteMap lights;

		public StateBinding StateBinding { get; private set; }

        private byte stateIndex {
            get {
                return (byte)CurrentState;
            }

            set {
                CurrentState = (State)value;
            }
        }

		private State state = State.Idle;
        private State CurrentState {
			get {
				return state;
			}

			set {
				if(state != value) {
					UpdateNetState(value);
					state = value;
				}
			}
		}
		

		//state vars

		private float range;
		private Gun target;
		private float captureTime;

		private ImpactedFrom attachedSide;
		private MaterialThing attachedTo;
		private Vec2 attachedOwnerPos;

		private float AttachedAngle {
			get {
				switch(attachedSide) {
					case ImpactedFrom.Top: return -MathHelper.Pi;
					case ImpactedFrom.Bottom: return 0f;
					case ImpactedFrom.Left: return MathHelper.Pi / 2;
					case ImpactedFrom.Right: return -MathHelper.Pi / 2;
					default: return 0f;
				}
			}
		}
        
        public MagNet(float xval, float yval)
            : base(xval, yval)
        {
			StateBinding = new StateBinding("stateIndex");
			
			ammo = 5;
			_ammoType = new ATShrapnel();
			
			_type = "gun";
			_editorName = "MagNet";
            _bio = "h";

			graphic = spriteMap = new SpriteMap(DuckUtils.GetAsset("weapons/magnet.png"), 16, 16);
			lights = new SpriteMap(DuckUtils.GetAsset("weapons/magnet_light.png"), 32, 32);
			
			spriteMap.AddAnimation("active", 0.1f, true, 1, 2);
			spriteMap.AddAnimation("idle", 0.1f, true, 0);
			spriteMap.SetAnimation("idle");

			lights.AddAnimation("active", 0.1f, true, 1, 2);
			lights.AddAnimation("idle", 0.1f, true, 0);
			lights.SetAnimation("idle");

			center = new Vec2(8f, 8f);
			collisionOffset = new Vec2(-4f, -4f);
			collisionSize = new Vec2(8f, 8f);
        }

		public override void OnPressAction()
		{
			if(CurrentState == State.Idle) {
				CurrentState = State.Attaching;
				SFX.Play("pullPin");
				spriteMap.SetAnimation("active");
				lights.SetAnimation("active");

				canPickUp = false;
				if(duck != null) duck.doThrow = true;
			}
		}

		public override void OnHoldAction() {}
		public override void OnReleaseAction() {}

		public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
		{
			if(CurrentState == State.Attaching) {
				attachedTo = with;
				attachedSide = from;
				attachedOwnerPos = with.position;

				CurrentState = State.Active;
			}
		}

		private bool CheckAccessibility(PhysicsObject obj, bool targeted) {
			if(obj.removeFromLevel || !obj.active || obj.owner != null) return false;
			if(!targeted && Math.Abs(obj.hSpeed) + Math.Abs(obj.vSpeed) < 2f) return false;
			if(targeted && (position - obj.position).lengthSq > 3 * MaxRange * MaxRange) return false;
			
			Vec2 hit;
			MaterialThing t = Level.CheckRay<MaterialThing>(position, obj.position, this, out hit);
			return t == null || t.thickness <= 1f || t == obj;
		}

		private void ApproachTarget() {
			Vec2 t = Offset(Vec2.Unity * -3f);
			Vec2 delta = t - target.position;
			Vec2 grav = delta.normalized * (CapturingSpeed * (1f - delta.lengthSq / (MaxRange * MaxRange)));
								
			target.hSpeed += (grav.x - target.hSpeed) * SpeedApproachMod + Rando.Float(-RandomFluctionations, RandomFluctionations);
			target.vSpeed += (grav.y - target.vSpeed) * SpeedApproachMod + Rando.Float(-RandomFluctionations, RandomFluctionations);;
		}

		private void UpdateNetState(State value) {
			if(value == State.Exploded) {
				Explosion.Create(this, ExplosionShrapnel);
			}
		}

		public override void Update() 
        {
			base.Update();

			if(isServerForObject) {
				if(attachedTo != null) {
					if(attachedTo.removeFromLevel || (attachedOwnerPos - attachedTo.position).LengthSquared() > 0.1f || Math.Max(hSpeed, vSpeed) > 0.1f) {
						attachedTo = null;
						gravMultiplier = 1f;
						CurrentState = State.Attaching;
					} else {
						hSpeed = vSpeed = 0f;
						gravMultiplier = 0f;
						angle = AttachedAngle;
					}
				}

				switch(CurrentState) {
					case State.Active: 
						if(range < MaxRange) range += Maths.IncFrameTimer() * RangeAllocationSpeed;
						foreach(Gun obj in Level.CheckCircleAll<Gun>(position, range)) {
							if(obj != this && CheckAccessibility(obj, false)) {
								target = obj;
								Fondle(obj);

								CurrentState = State.Targeted;
								break;
							}
						}
						break;

					case State.Targeted:
						if(target != null && CheckAccessibility(target, true)) {
							Vec2 delta = position - target.position;

							if(delta.lengthSq < ExplosionRangeSquared) {
								CurrentState = State.Exploding;
							} else {
								ApproachTarget();
							}
						} else CurrentState = State.Active;
						break;

					case State.Exploding:
						if(captureTime < ExplosionTime && target != null) {
							ApproachTarget();

							if(!CheckAccessibility(target, true)) {
								CurrentState = State.Active;
							}
							
							captureTime += Maths.IncFrameTimer();
							if(Rando.Float(1f) < 2 / 60f) target.PressAction();
						} else {
							captureTime = 0f;
							CurrentState = State.Exploded;
						}
						break;

					case State.Exploded:
						if(target != null) Level.Remove(target);
						CurrentState = State.Active;

						if(--ammo <= 0) Level.Remove(this);
						break;
				}
			}
		}

		public override void Draw() {
			base.Draw();
			Draw(lights, -16f, -12f, -8);
		}
		
		public enum State : byte {
            Idle,
            Attaching,
			Active,
			Targeted,
			Exploding,
			Exploded
        }
    }
}